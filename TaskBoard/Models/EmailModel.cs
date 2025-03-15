using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskBoard.Models;

public class EmailModel : IDisposable, IEquatable<EmailModel>
{
    [NotMapped] public EmailManager? EmailManager;

    [Key] public string Address { get; set; }

    public string? Password { get; set; }
    
    [ForeignKey("Account")]
    public long? AccountId { get; set; }
    public SnapchatAccountModel? Account { get; set; }
    public string Domain => Address.Split("@")[1];
    public bool IsFake { get; set; }

    public void Dispose()
    {
        EmailManager?.ReleaseEmail(this);
        GC.SuppressFinalize(this);
    }

    public bool Equals(EmailModel? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Address == other.Address;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((EmailModel)obj);
    }

    public override int GetHashCode()
    {
        return Address.GetHashCode();
    }
}