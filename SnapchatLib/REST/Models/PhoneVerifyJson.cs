namespace SnapchatLib.REST.Models;

public class PhoneVerifyJson
{
    public string message_format { get; set; }
    public bool logged { get; set; }
    public string param { get; set; }
    public string action { get; set; }
    public string message { get; set; }
}