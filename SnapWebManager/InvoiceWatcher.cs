using Microsoft.EntityFrameworkCore;
using SnapWebManager.Data;
using SnapWebModels;
using TaskBoard.PayServerApi;

namespace SnapWebManager;

public class InvoiceWatcher : IHostedService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<InvoiceWatcher> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeProvider _timeProvider;
    private Timer _checkInvoicesTimer;

    public InvoiceWatcher(IServiceProvider provider, ILogger<InvoiceWatcher> logger, TimeProvider timeProvider)
    {
        _logger = logger;
        _serviceProvider = provider;
        _timeProvider = timeProvider;
    }

    public void Dispose()
    {
        _checkInvoicesTimer?.Dispose();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Invoice Watcher service");
        _checkInvoicesTimer = new Timer(CheckInvoices, cancellationToken, TimeSpan.Zero, TimeSpan.FromMinutes(1));
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Invoice Watcher service");
        _checkInvoicesTimer?.Change(Timeout.Infinite, 0);
    }

    public async void CheckInvoices(object? state)
    {
        var scope = _serviceProvider.CreateScope();
        var payServerClient = scope.ServiceProvider.GetRequiredService<PayServerClient>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var invoices = await context.Invoices.ToListAsync();
        var nonCompletedInvoices = invoices.Where(i => i.ParsedStatus != InvoiceStatus.Settled && i.ParsedStatus != InvoiceStatus.Expired && i.ParsedStatus != InvoiceStatus.Invalid);
        if (!nonCompletedInvoices.Any())
        {
            await SyncModules();
            return;
        }

        var dbChanged = false;

        foreach (var invoice in nonCompletedInvoices)
        {
            var response = await payServerClient.GetInvoiceAsync(invoice.Id);
            if (invoice.ParsedStatus == response.ParsedStatus) continue;
            invoice.Status = response.Status;
            context.Update(invoice);
            dbChanged = true;
        }

        if (dbChanged)
            await context.SaveChangesAsync();

        await SyncModules();
    }

    internal virtual void AddAccessModules(IEnumerable<InvoiceModel> invoices)
    {
        var invoicesWithAccessModules = invoices.Where(i => i.PurchaseInfos.Any(pi =>
        {
            var moduleInfo = SnapWebModule.DefaultModules.FirstOrDefault(m => m.Id == pi.ModuleId);
            return moduleInfo != null && moduleInfo.Category == SnapWebModuleCategory.Access;
        }));

        foreach (var invoice in invoicesWithAccessModules)
        {
            var modules = invoice.PurchaseInfos.Where(pi =>
            {
                var moduleInfo = SnapWebModule.DefaultModules.FirstOrDefault(m => m.Id == pi.ModuleId);
                return moduleInfo?.Category == SnapWebModuleCategory.Access;
            });

            foreach (var purchaseInfo in modules)
                switch (purchaseInfo.ModuleId)
                {
                    case SnapWebModuleId.ExtraAccounts100:
                        // Adjust accounts
                        invoice.Client.MaxManagedAccounts += (int) purchaseInfo.Quantity * 100;
                        break;
                    case SnapWebModuleId.SnapwebAccess:
                    {
                        var now = _timeProvider.UtcNow();
                        invoice.Client.AccessDeadline = invoice.Client.AccessDeadline <= now ? now.AddDays(7 * purchaseInfo.Quantity) : invoice.Client.AccessDeadline.AddDays(7 * purchaseInfo.Quantity);
                        break;
                    }
                    case SnapWebModuleId.ExtraStorage1:
                        invoice.Client.MaxQuotaMb += Literals.GbToMbConversionLiteral;
                        break;
                    case SnapWebModuleId.ExtraStorage2:
                        invoice.Client.MaxQuotaMb += 5 * Literals.GbToMbConversionLiteral;
                        break;
                    case SnapWebModuleId.ExtraStorage3:
                        invoice.Client.MaxQuotaMb += 10 * Literals.GbToMbConversionLiteral;
                        break;
                    case SnapWebModuleId.ExtraConcurrencySlot:
                        invoice.Client.Threads += (int) purchaseInfo.Quantity * 1;
                        break;
                    case SnapWebModuleId.ExtraWorkSlot:
                        invoice.Client.MaxTasks += (int) purchaseInfo.Quantity * 1;
                        break;
                }
        }
    }

    internal virtual IEnumerable<AllowedModules> CreateModulesToAdd(List<InvoiceModel> missingModulesInvoices, List<AllowedModules> allowedModules)
    {
        var entries = new List<AllowedModules>();
        foreach (var invoice in missingModulesInvoices)
        foreach (var invoicePurchaseInfo in invoice.PurchaseInfos)
        {
            // These don't go into allowedmodules
            var moduleInfo = SnapWebModule.DefaultModules.FirstOrDefault(m => m.Id == invoicePurchaseInfo.ModuleId);
            if (moduleInfo == null || moduleInfo.Category != SnapWebModuleCategory.Functionality) continue;

            if (allowedModules.Any(m => m.Client.ClientId == invoice.Client.ClientId && m.ModuleId == invoicePurchaseInfo.ModuleId)) continue;
            entries.Add(new AllowedModules {Client = invoice.Client, ModuleId = invoicePurchaseInfo.ModuleId});
        }

        return entries;
    }

    internal virtual async Task SyncModules()
    {
        var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var invoices = await context.Invoices.Include(i => i.Client).ToListAsync();
        var completedInvoices = invoices.Where(i => i.ParsedStatus == InvoiceStatus.Settled && i.SnapwebStatus == SnapwebStatus.Unprocessed).ToList();

        if (!completedInvoices.Any()) return;

        context.AttachRange(completedInvoices);
        AddAccessModules(completedInvoices);

        var allowedModules = await context.AllowedModules.Include(m => m.Client).ToListAsync();
        context.AttachRange(allowedModules);
        var missingModulesInvoices = completedInvoices.Where(c => c.PurchaseInfos.Any(pi => SnapWebModule.DefaultModules.FirstOrDefault(m => m.Id == pi.ModuleId && m.Category != SnapWebModuleCategory.Access) != null)).ToList();
        missingModulesInvoices = missingModulesInvoices.Where(i => !allowedModules.Any(m => i.PurchaseInfos.Any(pi => pi.ModuleId == m.ModuleId) && m.Client.ClientId == i.Client.ClientId)).ToList();

        if (!missingModulesInvoices.Any())
        {
            completedInvoices.ForEach(i => i.SnapwebStatus = SnapwebStatus.Processed);
            await context.SaveChangesAsync();
            return;
        }

        var modulesToAdd = CreateModulesToAdd(missingModulesInvoices, allowedModules).ToList();
        await context.AddRangeAsync(modulesToAdd);
        completedInvoices.ForEach(i => i.SnapwebStatus = SnapwebStatus.Processed);
        await context.SaveChangesAsync();
    }
}