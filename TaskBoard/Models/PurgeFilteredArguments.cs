namespace TaskBoard.Models;

public class PurgeFilteredArguments
{
    public string CountryCode { get; set; }
    public string Gender { get; set; }
    public string Race { get; set; }
    public string Added { get; set; }
    public string Searched { get; set; }
}

public class PurgeAccountFilteredArguments
{
    public string emailvalidation { get; set; }
    public string phonevalidation { get; set; }
    public string status { get; set; }
    public string hasadded { get; set; }
}