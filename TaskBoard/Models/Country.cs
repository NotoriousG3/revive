namespace TaskBoard.Models;

public class Country
{
    private static Country Russia = new("RU", 4, "russia", 2, 0);
    private static Country UnitedStates = new("US", 1, "usa", 2, 187);
    private static Country UnitedKingdom = new("UK", 2, "england", 3, 16);
    private static Country GreatBritian = new("GB", 2, "england", 3, 16);
    private static Country Netherlands = new("NL", 3, "netherlands", 3, 48);

    private static Dictionary<string, Country> Map = new()
    {
        {Russia.ISO, Russia},
        {UnitedStates.ISO, UnitedStates},
        {UnitedKingdom.ISO, UnitedKingdom},
        {Netherlands.ISO, Netherlands},
        {GreatBritian.ISO, GreatBritian}
    };

    public readonly string FiveSimId;
    public readonly int SmsActivateId;
    public readonly string ISO;
    public readonly int SmsPoolId;
    public readonly int CodeLength;

    private Country(string iso, int smsPoolId, string fiveSimId, int codeLength, int smsactivateId)
    {
        ISO = iso;
        SmsPoolId = smsPoolId;
        FiveSimId = fiveSimId;
        CodeLength = codeLength;
        SmsActivateId = smsactivateId;
    }

    public static Country GetCountry(string isoCode)
    {
        foreach (var t in Map)
        {
            Console.WriteLine(t.Value.ISO);
        }
        Map.TryGetValue(isoCode.ToUpper(), out var value);
        Console.WriteLine($"isoCode: {isoCode}, value: {value.ISO}");
        return value;
    }
}