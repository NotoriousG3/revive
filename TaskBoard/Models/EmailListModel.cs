using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace TaskBoard.Models;

public class EmailListModel : IEquatable<EmailListModel>
{
    [Key]
    public long Id { get; set; }
    public string Address { get; set; }

    public static Regex validCharactersRegex = new("^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$"); // Need to change for proper email regex

        
    public static bool Validate(EmailListModel email)
    {
        if (string.IsNullOrWhiteSpace(email.Address)) throw new ArgumentException("E-Mail address must not be empty");
        if (!validCharactersRegex.IsMatch(email.Address))
            throw new ArgumentException("E-Mail Address must be valid.");
        return true;
    }

    public bool Equals(EmailListModel? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Address == other.Address;
    }

    public override int GetHashCode()
    {
        return Address.GetHashCode();
    }

    public string ToExportString()
    {
        return Address;
    }
}