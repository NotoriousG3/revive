using Newtonsoft.Json;

namespace TaskBoard.Models;

public class PredictKlass
{
    public string name { get; set; }
    public string q { get; set; }
    public string gender { get; set; }
    public int total_names { get; set; }
    public int probability { get; set; }
    public string country { get; set; }
    public bool status { get; set; }
    public string duration { get; set; }
    public int used_credits { get; set; }
    public int remaining_credits { get; set; }
    public int expires { get; set; }
    public string server { get; set; }
}

public class EthnicityProbability
{
    public double A { get; set; }
    public double AR { get; set; }
    public double B { get; set; }
    public double H { get; set; }
    public double N { get; set; }
    public double P { get; set; }
    public double W { get; set; }
}

public class DiversityData
{
    public string fullname { get; set; }
    public string gender { get; set; }

    [JsonProperty("gender probability")]
    public double GenderProbability { get; set; }
    public string ethnicity { get; set; }

    [JsonProperty("ethnicity probability")]
    public EthnicityProbability EthnicityProbability { get; set; }
}