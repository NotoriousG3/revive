using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NuGet.ContentModel;
using TaskBoard.Models;

namespace TaskBoard.Tests;

public class TargetUserManagerTests
{
    private IQueryable<TargetUser> _data;
    
    private string _connectionString = $"Server=localhost; Port=3306; Uid=root; Pwd=; Database=test; Max Pool Size=2000";
    private ServiceCollection _collection;
    private IServiceProvider _provider;
    private Mock<Utilities> _utilitiesMock;
    
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var collection = new ServiceCollection();
        //collection.AddDbContext<ApplicationDbContext>(options =>
         //   options.UseMySql(_connectionString, /*new MariaDbServerVersion(new Version(10,6,10))*/ServerVersion.AutoDetect(_connectionString)));

        _provider = collection.BuildServiceProvider();
        using var context = _provider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }

    [SetUp]
    public void SetUp()
    {
        _utilitiesMock = new Mock<Utilities>();
        var factory = _provider.GetRequiredService<IServiceScopeFactory>();
        using var context = factory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.TargetUsers.RemoveRange(context.TargetUsers);
        context.SaveChanges();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        var factory = _provider.GetRequiredService<IServiceScopeFactory>();
        using var context = factory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureDeleted();
    }

    private async Task<List<TargetUser>> InsertUsers(int amount, ApplicationDbContext context)
    {
        for (var i = 0; i < amount; i++)
        {
            context.TargetUsers.AddRange(new TargetUser() { Username = $"user{i}"});
        }
        
        await context.SaveChangesAsync();
        return await context.TargetUsers.ToListAsync();
    }

    [Test]
    public async Task Add_Performance_Check()
    {
        var factory = _provider.GetRequiredService<IServiceScopeFactory>();

        await using (var context = factory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>())
        {
            await InsertUsers(500000, context);
        }

        var targetManager = new TargetManager(factory, _utilitiesMock.Object);
        var timer = new Stopwatch();
        timer.Start();
        await targetManager.Add(new TargetUser() { Username = "test" });
        timer.Stop();
        Assert.That(timer.ElapsedMilliseconds < 1000);
    }

    // TODO: Need to find a way to run a test that measures ram usage
    /*[Test]
    public async Task GetRandomAmountOfUsers_Check()
    {
        var utilities = new Mock<Utilities>();

        var factory = _provider.GetRequiredService<IServiceScopeFactory>();
        await using (var context = factory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>())
        {
            await InsertUsers(1500000, context);
        }

        var targetManager = new TargetManager(factory, utilities.Object);

        var target = targetManager.GetRandomAmountOfUsers(70);
        Assert.True(true);
    }*/

    [Test]
    public async Task GetWorkTargetUsers_Returns_Correct_TargetUsers()
    {
        var factory = _provider.GetRequiredService<IServiceScopeFactory>();
        var firstWork = new WorkRequest()
        {
            Arguments = ""
        };
        var expectedCount = 3;

        List<TargetUser> expectedTargets;
        await using (var context = factory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>())
        {
            await InsertUsers(10, context);
            expectedTargets = context.TargetUsers.Take(expectedCount).ToList();
            context.Add(firstWork);
            await context.SaveChangesAsync();
            foreach (var target in expectedTargets)
            {
                context.Add(new ChosenTarget() { TargetUserId = target.Id, WorkId = firstWork.Id });
            }
            await context.SaveChangesAsync();
        }
        
        var targetManager = new TargetManager(factory, _utilitiesMock.Object);
        
        var results = await targetManager.GetWorkTargetUsers(firstWork);
        
        Assert.That(results.Count(), Is.EqualTo(expectedCount));
        CollectionAssert.AreEquivalent(expectedTargets, results);
    }

    private static TargetUser targetAdded = new() { Username = "added", Added = true };
    private static TargetUser targetNone  = new() { Username = "normal"};
    private static TargetUser targetCountryAny = new() { Username = "country1", CountryCode = "bla" };
    private static TargetUser targetCountryEn = new() { Username = "countryEn", CountryCode = "en" };
    private static TargetUser targetCountryEnRaceRa = new() { Username = "countryEnRaceRa", CountryCode = "en", Race = "ra" };
    private static TargetUser targetCountryEnRaceHi = new() { Username = "countryEnRaceRa", CountryCode = "en", Race = "hi" };
    private static TargetUser targetCountryEnRaceRaGenderGe = new() { Username = "countryEnRaceRaGenderGe", CountryCode = "en", Race = "ra", Gender = "ge" };
    private static TargetUser targetCountryEnGenderGe = new() { Username = "countryEnGenderGe", CountryCode = "en", Gender = "ge" };
    private static TargetUser targetRaceRaGenderGe = new() { Username = "RaceRaGenderGe", Race = "ra", Gender = "ge" };
    private static TargetUser targetAllAny = new() { Username = "AllAny", CountryCode = "bla", Race = "ra", Gender = "ge" };
    private static TargetUser targetCountryArabicIq = new() { Username = "CountryArabicIq", CountryCode = "IQ" };
    private static TargetUser targetUsed = new() { Username = "Used", Used = true };

    private async Task SetupGetRandomTargetNamesContext(IServiceScopeFactory factory, List<TargetUser> users)
    {
        await using var context = factory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.AddRange(users);
        await context.SaveChangesAsync();
    }

    private static IEnumerable<TestCaseData> TestCases()
    {
        yield return new TestCaseData(new List<TargetUser>() { targetNone, targetUsed, targetAdded }, new List<TargetUser>() { targetAdded }, 1, true, null, null, null);
        yield return new TestCaseData(new List<TargetUser>() { targetNone, targetAdded }, new List<TargetUser>() { targetNone, targetAdded }, 2, false, null, null, null);
        yield return new TestCaseData(new List<TargetUser>() { targetCountryAny }, new List<TargetUser>() { targetCountryAny }, 2, false, "ANY", null, null);
        yield return new TestCaseData(new List<TargetUser>() { targetCountryAny, targetNone }, new List<TargetUser>() { targetNone, targetCountryAny }, 2, false, "ANY", null, null);
        yield return new TestCaseData(new List<TargetUser>() { targetCountryAny, targetCountryEn }, new List<TargetUser>() { targetCountryEn }, 2, false, "en", null, null);
        yield return new TestCaseData(new List<TargetUser>() { targetCountryAny, targetCountryEnRaceHi, targetCountryEnRaceRa }, new List<TargetUser>() { targetCountryEnRaceRa }, 2, false, "en", "ra", null);
        yield return new TestCaseData(new List<TargetUser>() { targetNone, targetCountryEnRaceRa, targetCountryEnRaceRaGenderGe }, new List<TargetUser>() { targetCountryEnRaceRaGenderGe }, 2, false, "en", "ra", "ge");
        yield return new TestCaseData(new List<TargetUser>() { targetNone, targetCountryEn, targetCountryEnGenderGe }, new List<TargetUser>() { targetCountryEnGenderGe }, 2, false, "en", null, "ge");
        yield return new TestCaseData(new List<TargetUser>() { targetCountryEn, targetRaceRaGenderGe }, new List<TargetUser>() { targetRaceRaGenderGe }, 2, false, null, "ra", "ge");
        yield return new TestCaseData(new List<TargetUser>() { targetCountryEn, targetRaceRaGenderGe, targetCountryAny, targetCountryEnGenderGe }, new List<TargetUser>() { targetCountryEnGenderGe, targetRaceRaGenderGe }, 2, false, "ANY", "ANY", "ge");
        yield return new TestCaseData(new List<TargetUser>() { targetCountryEn, targetRaceRaGenderGe, targetCountryAny, targetAllAny }, new List<TargetUser>() { targetCountryEn, targetRaceRaGenderGe, targetCountryAny, targetAllAny }, 4, false, "ANY", "ANY", "ANY");
        yield return new TestCaseData(new List<TargetUser>() { targetCountryEn, targetUsed }, new List<TargetUser>() { targetCountryEn }, 4, false, null, null, null);
        yield return new TestCaseData(new List<TargetUser>() { targetCountryArabicIq, targetAllAny }, new List<TargetUser>() { targetCountryArabicIq }, 4, false, "ARABIC COUNTRIES", null, null);
    }
    
    [TestCaseSource(nameof(TestCases))]
    public async Task GetRandomTargetNames_Tests(List<TargetUser> users, List<TargetUser> expectedUsers, int requestAmount, bool added, string country, string race, string gender)
    {
        var factory = _provider.GetRequiredService<IServiceScopeFactory>();
        await SetupGetRandomTargetNamesContext(factory, users);

        var targetManager = new TargetManager(factory, _utilitiesMock.Object);
        
        var results = await targetManager.GetRandomTargetNames(requestAmount, false, added, country, race, gender);
        
        Assert.That(results.Count(), Is.EqualTo(expectedUsers.Count));
        CollectionAssert.AreEquivalent(expectedUsers, results);
    }
}