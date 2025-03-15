using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using Newtonsoft.Json;
using SnapWebManager.Data;
using SnapWebModels;

namespace SnapWebManager.Tests;

public class InvoiceWatcherTests
{
    private Mock<ApplicationDbContext> _contextMock;
    private Mock<ILogger<InvoiceWatcher>> _loggerMock;
    private Mock<TimeProvider> _timeProviderMock;

    private IServiceProvider _serviceProvider;
    private static SnapWebClientModel _client;

    private static PurchaseInfo _extraAccountsPurchaseInfo = new() {ModuleId = SnapWebModuleId.ExtraAccounts100, Quantity = 1};
    private static PurchaseInfo _webAccessPurchaseInfo = new() {ModuleId = SnapWebModuleId.SnapwebAccess, Quantity = 1};
    private static PurchaseInfo _sendMessagePurchaseInfo = new() {ModuleId = SnapWebModuleId.SendMessage, Quantity = 1};
    private static PurchaseInfo _postDirectPurchaseInfo = new() {ModuleId = SnapWebModuleId.PostDirect, Quantity = 1};
    private static PurchaseInfo _findUsersViaSearchInfo = new() {ModuleId = SnapWebModuleId.FindUsersViaSearch, Quantity = 1};
    private static PurchaseInfo _EmailScraper = new() {ModuleId = SnapWebModuleId.EmailScraper, Quantity = 1};
    private static PurchaseInfo _PhoneScraper = new() {ModuleId = SnapWebModuleId.PhoneScraper, Quantity = 1};

    private static InvoiceModel _testInvoice;
    private static InvoiceModel _testInvoice2;

    private InvoiceWatcher _testInstance;
    
    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<ApplicationDbContext>();
        _loggerMock = new Mock<ILogger<InvoiceWatcher>>();
        _timeProviderMock = new Mock<TimeProvider>();

        _client = new SnapWebClientModel
        {
            ClientId = "test",
            MaxManagedAccounts = 1,
            AccessDeadline = new DateTime(0)
        };
        
        _testInvoice = new InvoiceModel
        {
            Client = _client,
            Id = "test",
            PurchaseInfoString = "[]",
            Status = "New",
        };
        
        _testInvoice2 = new InvoiceModel
        {
            Client = _client,
            Id = "test2",
            PurchaseInfoString = "[]",
            Status = "New"
        };

        var services = new ServiceCollection();
        services.AddSingleton<ILogger<InvoiceWatcher>>(_loggerMock.Object);
        services.AddSingleton<ApplicationDbContext>(_contextMock.Object);
        services.AddSingleton<TimeProvider>(_timeProviderMock.Object);

        _serviceProvider = services.BuildServiceProvider();

        _testInstance = new InvoiceWatcher(_serviceProvider, _loggerMock.Object, _timeProviderMock.Object);
    }

    [Test]
    public void AddAccessModules_Invoices_Without_AccessModules_DoesNothing()
    {
        _testInvoice.PurchaseInfoString = JsonConvert.SerializeObject(new List<PurchaseInfo>() {_sendMessagePurchaseInfo});
        var invoices = new List<InvoiceModel>() {_testInvoice};
        _testInstance.AddAccessModules(invoices);
        
        Assert.That(_client.MaxManagedAccounts, Is.EqualTo(1));
        Assert.That(_client.AccessDeadline.Ticks, Is.EqualTo(new DateTime(0).Ticks));
    }
    
    [Test]
    public void AddAccessModules_Invoices_With_ExtraAccounts100_Modifies_MaxManagedAccounts()
    {
        _testInvoice.PurchaseInfoString = JsonConvert.SerializeObject(new List<PurchaseInfo>() {_extraAccountsPurchaseInfo}); 
        var invoices = new List<InvoiceModel>() { _testInvoice };
        _testInstance.AddAccessModules(invoices);
        
        Assert.That(_client.MaxManagedAccounts, Is.EqualTo(101));
        Assert.That(_client.AccessDeadline.Ticks, Is.EqualTo(new DateTime(0).Ticks));
    }
    
    [Test]
    public void AddAccessModules_InvoiceWithSnapWebAccess_WhenOverAccessDeadline_SetsAccessDeadlineFromDate()
    {
        var currentDate = DateTime.UtcNow;
        _client.AccessDeadline = currentDate.AddDays(-7);
        var expectedDate = currentDate.AddDays(7 * _webAccessPurchaseInfo.Quantity);
        
        _timeProviderMock.Setup(m => m.UtcNow()).Returns(currentDate);
        _testInvoice.PurchaseInfoString = JsonConvert.SerializeObject(new List<PurchaseInfo>() {_webAccessPurchaseInfo}); 
        var invoices = new List<InvoiceModel>() {_testInvoice};
        _testInstance.AddAccessModules(invoices);
        
        Assert.That(_client.MaxManagedAccounts, Is.EqualTo(1));
        Assert.That(_client.AccessDeadline, Is.EqualTo(expectedDate));
    }
    
    [Test]
    public void AddAccessModules_InvoiceWithSnapWebAccess_WhenTimeLeftToAccessDeadline_SetsAccessDeadlineFromClient()
    {
        var currentDate = DateTime.UtcNow;
        _client.AccessDeadline = currentDate.AddDays(2);
        var expectedDate = _client.AccessDeadline.AddDays(7 * _webAccessPurchaseInfo.Quantity);
        
        _timeProviderMock.Setup(m => m.UtcNow()).Returns(currentDate);
        _testInvoice.PurchaseInfoString = JsonConvert.SerializeObject(new List<PurchaseInfo>() {_webAccessPurchaseInfo}); 
        var invoices = new List<InvoiceModel>() {_testInvoice};
        _testInstance.AddAccessModules(invoices);
        
        Assert.That(_client.MaxManagedAccounts, Is.EqualTo(1));
        Assert.That(_client.AccessDeadline, Is.EqualTo(expectedDate));
    }

    [Test]
    public void SyncModules_NoCompletedInvoices_DoesNothing()
    {
        var invoices = new List<InvoiceModel>() {_testInvoice, _testInvoice2};
        _contextMock.Setup(m => m.Invoices).ReturnsDbSet(invoices);

        _testInstance.SyncModules().Wait();
        _contextMock.Verify(m => m.AttachRange(It.IsAny<IEnumerable<object>>()), Times.Never);
    }

    [Test]
    public void SyncModules_WithCompletedInvoices_NoMissingModules_OnlyCalls_AddAccessModules()
    {
        // Set as complete
        _testInvoice.Status = "Settled";
        _testInvoice2.Status = "Settled";
        
        // Put purchase info
        _testInvoice.PurchaseInfoString = JsonConvert.SerializeObject(new List<PurchaseInfo>() {_extraAccountsPurchaseInfo});
        _testInvoice2.PurchaseInfoString = JsonConvert.SerializeObject(new List<PurchaseInfo>() {_sendMessagePurchaseInfo});
        var invoices = new List<InvoiceModel>() {_testInvoice, _testInvoice2};
        
        // Setup db returns, since we want no missing modules, allowed modules must also be set
        _contextMock.Setup(m => m.Invoices).ReturnsDbSet(invoices);
        _contextMock.Setup(m => m.AllowedModules).ReturnsDbSet(new List<AllowedModules>()
        {
            new() { ModuleId = SnapWebModuleId.SendMessage, Client = _client }
        });
        
        // We test through a mock so that we don't go into the other functions
        var testMock = new Mock<InvoiceWatcher>(_serviceProvider, _loggerMock.Object, _timeProviderMock.Object);
        testMock.Setup(m => m.SyncModules()).CallBase();

        testMock.Object.SyncModules().Wait();
        testMock.Verify(m => m.AddAccessModules(It.IsAny<IEnumerable<InvoiceModel>>()), Times.Once);
        testMock.Verify(m => m.CreateModulesToAdd(It.IsAny<List<InvoiceModel>>(), It.IsAny<List<AllowedModules>>()), Times.Never);
    }

    private static IEnumerable<TestCaseData> SyncModules_TestCaseSource()
    {
        yield return new TestCaseData(new List<InvoiceModel> { new(){ Client = new SnapWebClientModel() { ClientId = "test", Threads = 1 }, Status = "Settled", PurchaseInfoString = JsonConvert.SerializeObject(new List<PurchaseInfo>() {_sendMessagePurchaseInfo})} });
        yield return new TestCaseData(new List<InvoiceModel>
        {
            new(){ Client = new SnapWebClientModel() { ClientId = "test", Threads = 1 }, Status = "Settled", PurchaseInfoString = JsonConvert.SerializeObject(new List<PurchaseInfo>() {_sendMessagePurchaseInfo})},
            new(){ Client = new SnapWebClientModel() { ClientId = "test", Threads = 1 }, Status = "Settled", PurchaseInfoString = JsonConvert.SerializeObject(new List<PurchaseInfo>() {_postDirectPurchaseInfo})},
        });
        yield return new TestCaseData(new List<InvoiceModel>
        {
            new(){ Client = new SnapWebClientModel() { ClientId = "test", Threads = 1 }, Status = "Settled", PurchaseInfoString = JsonConvert.SerializeObject(new List<PurchaseInfo>() {_sendMessagePurchaseInfo, _extraAccountsPurchaseInfo})},
        });
    }

    [Test]
    [TestCaseSource(nameof(SyncModules_TestCaseSource))]
    public void SyncModules_WithCompletedInvoices_WithMissingModules_Calls_CreateModulesToAdd(List<InvoiceModel> invoices)
    {
        // Setup db returns, since we want missing modules, allowedmodules is empty
        _contextMock.Setup(m => m.Invoices).ReturnsDbSet(invoices);
        _contextMock.Setup(m => m.AllowedModules).ReturnsDbSet(new List<AllowedModules>());
        
        // We test through a mock so that we don't go into the other functions
        var testMock = new Mock<InvoiceWatcher>(_serviceProvider, _loggerMock.Object, _timeProviderMock.Object);
        testMock.Setup(m => m.SyncModules()).CallBase();

        testMock.Object.SyncModules().Wait();
        testMock.Verify(m => m.CreateModulesToAdd(It.Is<List<InvoiceModel>>(l => l.Count == invoices.Count), It.IsAny<List<AllowedModules>>()), Times.Once);
    }

    [Test]
    public void CreateModulesToAdd_WithAccessModelsOnly_ReturnsEmptyList()
    {
        _testInvoice.PurchaseInfoString = JsonConvert.SerializeObject(new List<PurchaseInfo>() {_extraAccountsPurchaseInfo, _webAccessPurchaseInfo}); 
        var missing = new List<InvoiceModel>() { _testInvoice };

        var result = _testInstance.CreateModulesToAdd(missing, new List<AllowedModules>());
        Assert.IsEmpty(result);
    }

    [Test]
    public void CreateModulesToAdd_WithMixedModules_ReturnsAddsModuleEntry()
    {
        _testInvoice.PurchaseInfoString = JsonConvert.SerializeObject(new List<PurchaseInfo>() {_extraAccountsPurchaseInfo, _sendMessagePurchaseInfo}); 
        var missing = new List<InvoiceModel>() { _testInvoice };

        var result = _testInstance.CreateModulesToAdd(missing, new List<AllowedModules>()).ToList();
        Assert.That(result.Count, Is.EqualTo(1));
        var entry = result.FirstOrDefault();
        Assert.That(entry.ModuleId == SnapWebModuleId.SendMessage && entry.Client == _client);
    }
}
