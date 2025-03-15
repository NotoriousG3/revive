using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using _5sim.Objects;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace TaskBoard.Models;

public class Proxy : IEquatable<Proxy>
{
    public Proxy()
    {
    }

    public Proxy(WebProxy webProxy)
    {
        try
        {
            var credentials = webProxy.Credentials.GetCredential(new Uri(webProxy.Address.AbsoluteUri), "Basic");

            User = credentials?.UserName;
            Password = credentials?.Password;
        }
        catch
        {
            Address = webProxy.Address;
        }
        finally
        {
            Address = webProxy.Address;   
        }
    }

    public int Id { get; set; }
    public Uri Address { get; set; }
    public string? User { get; set; }
    public string? Password { get; set; }
    [NotMapped]
    public string? GroupName { get; set; }
    [NotMapped]
    public long? GroupId { get; set; }
    public DateTime? LastUsed { get; set; }
    public ICollection<SnapchatAccountModel>? Accounts { get; set; } = new List<SnapchatAccountModel>();
    public virtual ICollection<ProxyGroup>? Groups { get; set; }
    public long AccountsCount => Accounts.Count;

    public bool Equals(Proxy? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Address == other.Address && User == other.User && Password == other.Password;
    }

    public static void Validate(Proxy proxy)
    {
        if (string.IsNullOrWhiteSpace(proxy.Address?.ToString())) throw new ArgumentException("Address must be provided and be a valid URL");
        if (!Uri.IsWellFormedUriString(proxy.Address.ToString(), UriKind.Absolute)) throw new ArgumentException("Invalid URL");
        if (proxy.Address.Scheme != Uri.UriSchemeHttp)
            throw new ArgumentException("Only HTTP scheme is allowed");
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Address, User, Password);
    }

    public string ToExportString()
    {
        return $"{Address.ToString().Replace("http://", "").Replace("/","")}:{User}:{Password}";
    }
    
    public WebProxy ToWebProxy() => new WebProxy()
    {
        Address = Address,
        Credentials = new NetworkCredential(User, Password)
    };
}