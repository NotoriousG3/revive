using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace TaskBoard.Models;

public class PhoneListModel : IEquatable<PhoneListModel>
{
    [Key]
    public long Id { get; set; }
    public string Number { get; set; }
    public string CountryCode { get; set; }

    public static Regex validCharactersRegex = new(@"^\d{10}$"); // Need to change for proper phone regex

    public static bool Validate(PhoneListModel phone)
    {
        if (string.IsNullOrWhiteSpace(phone.Number)) throw new ArgumentException("Phone number must not be empty");
        if (!validCharactersRegex.IsMatch(phone.Number))
            throw new ArgumentException("Phone number can only contain letters or numbers.");
        return true;
    }

    public bool Equals(PhoneListModel? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Number == other.Number;
    }

    public override int GetHashCode()
    {
        return Number.GetHashCode();
    }

    public string ToExportString()
    {
        return Number;
    }
}