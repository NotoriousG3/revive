using Newtonsoft.Json;
using RandomDataGenerator.FieldOptions;
using RandomDataGenerator.Randomizers;
using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;

namespace TaskBoard;

public struct FakePerson
{
    [JsonProperty("name")] public string Name;
    public string FirstName => Name.Split(" ")[0];
    public string LastName => Name.Split(" ")[1];

    [JsonProperty("password")] public string Password;
    [JsonProperty("email_u")] public string EmailUser;
    [JsonProperty("email_d")] public string EmailDomain;

    public string Email;
}

public class FakePersonGenerator
{
    private readonly Random m_Random = new ();
    public string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[m_Random.Next(s.Length)]).ToArray());
    }
    
    private string GenerateEmail(string domain)
    {
        return RandomString(16) + "@" + domain;
    }

    private string GetRandomFirstName(Gender gender, string? nameTemplate = null)
    {
        if (!string.IsNullOrWhiteSpace(nameTemplate)) return nameTemplate;
        
        return Faker.Name.First();
    }
    
    private string GetRandomLastName(string? nameTemplate = null)
    {
        if (!string.IsNullOrWhiteSpace(nameTemplate)) return nameTemplate;
        
        return Faker.Name.Last();
    }

    public virtual FakePerson Generate(EmailModel? email, Gender gender, string? firstNameTemplate = null, string? lastNameTemplate = null)
    {
        FakePerson person = new();
        Random r = new();

        List<string> primeDomains = new List<string>(){"gmail.com","suddenlink.net","web.de","bell.net","orange.fr"};
        var firstName = GetRandomFirstName(gender, firstNameTemplate);
        var lastName = GetRandomLastName(lastNameTemplate);
        
        var randomizerTextRegex = RandomizerFactory.GetRandomizer(new FieldOptionsTextRegex { Pattern = "^[a-z]{1}[a-zA-Z0-9]{3}[!&%.,@#$^]{1}[a-zA-Z0-9]{3}" });
        
        person.Name = $"{firstName} {lastName}";
        person.Email = email == null ? GenerateEmail(primeDomains[r.Next(0,primeDomains.Count)]) : email.Address;
        

        var _password = randomizerTextRegex.Generate();

        if (!string.IsNullOrEmpty(_password))
            person.Password = _password;

        return person;
    }
}