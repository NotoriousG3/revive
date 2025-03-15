using System.Diagnostics;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.ContentModel;
using TaskBoard.Models;
using ILogger = Microsoft.Build.Framework.ILogger;

namespace TaskBoard.Tests;

public class ProxyManagerTests
{
    private IQueryable<Proxy> _data;
    
    private static string testDbName = nameof(ProxyManagerTests);
    private string _connectionString = $"Server=localhost; Port=3306; Uid=root; Pwd=; Database={testDbName}; Max Pool Size=2000";
    private ServiceCollection _collection;
    private IServiceProvider _provider;
    private Mock<ILogger<ProxyManager>> _loggerMock;
    
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var collection = new ServiceCollection();
        //collection.AddDbContext<ApplicationDbContext>(options =>
          //  options.UseMy(_connectionString, /*new MariaDbServerVersion(new Version(10,6,10))*/ServerVersion.AutoDetect(_connectionString)));

        _provider = collection.BuildServiceProvider();
        using var context = _provider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<ProxyManager>>();
        var factory = _provider.GetRequiredService<IServiceScopeFactory>();
        using var context = factory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Proxies.RemoveRange(context.Proxies);
        context.SaveChanges();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        var factory = _provider.GetRequiredService<IServiceScopeFactory>();
        using var context = factory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureDeleted();
    }

    private async Task<List<Proxy>> InsertProxies(int amount, ApplicationDbContext context, ProxyGroup? group = null)
    {
        for (var i = 0; i < amount; i++)
        {
            var proxy = new Proxy()
                { Address = new Uri("https://test.com:1000"), User = $"user{i}", Password = "test" };
            if (group != null)
            {
                proxy.Groups = new List<ProxyGroup>() { group };
            }
            
            context.Proxies.AddRange(proxy);
        }
        
        await context.SaveChangesAsync();
        return await context.Proxies.ToListAsync();
    }

    [Test]
    public async Task Take_Returns_Proxy()
    {
        var factory = _provider.GetRequiredService<IServiceScopeFactory>();

        List<TargetUser> expectedTargets;
        await using (var context = factory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>())
        {
            await InsertProxies(100, context);
        }
        
        var manager = new ProxyManager(factory, _loggerMock.Object);

        var t1 = await manager.Take();
        var t2 = await manager.Take();

        Assert.That(t1, Is.Not.EqualTo(t2));
    }
    
    [Test]
    public async Task Take_WithGroupId_Returns_Proxy_InGroup()
    {
        var factory = _provider.GetRequiredService<IServiceScopeFactory>();
        
        var group = new ProxyGroup() { Name = "group1" };
        var group2 = new ProxyGroup() { Name = "group2" };
        await using (var context = factory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>())
        {
            // insert first group
            await context.ProxyGroups.AddRangeAsync(new [] { group, group2 });
            await context.SaveChangesAsync();
            await InsertProxies(100, context, group);
            await InsertProxies(100, context, group2);

            // Refresh data
            group = await context.ProxyGroups.FindAsync(group.Id);
        }
        
        var manager = new ProxyManager(factory, _loggerMock.Object);

        Assert.That(manager.GetProxieCount(), Is.EqualTo(200));
        Assert.That(group.Proxies.Count, Is.EqualTo(100));

        var result = await manager.Take(group);
        Assert.That(result.Groups.First().Id, Is.EqualTo(group.Id));
    }

    [Test]
    [TestCase(10)]
    [TestCase(100)]
    [TestCase(1000)]
    [TestCase(10000)]
    public async Task Take_MultipleThreads_Works(int threadCount)
    {
        var factory = _provider.GetRequiredService<IServiceScopeFactory>();
        
        await using (var context = factory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>())
        {
            await InsertProxies(100, context);
        }
        
        var manager = new ProxyManager(factory, _loggerMock.Object);

        var list = new List<Task<Proxy>>();
        for (var i = 0; i < threadCount; i++)
        {
            list.Add(Task.Run(async () =>
            {
                var proxy = await manager.Take();
                var time = DateTime.Now;
                Console.WriteLine($"{time:O} - Took Proxy with Id {proxy.Id}");
                return proxy;
            }));
        }

        await Task.WhenAll(list);

        var results = list.Select(t => t.Result.Id).ToHashSet();
        Console.WriteLine($"Result Count: {results.Count}");
        Assert.That(results.Count, Is.GreaterThan(1));
    }
}