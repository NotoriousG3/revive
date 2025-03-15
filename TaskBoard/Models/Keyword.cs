using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace TaskBoard.Models;

public class Keyword : IEquatable<Keyword>
{
    [Key]
    public long Id { get; set; }
    public string Name { get; set; }

    public static Regex validCharactersRegex = new(@"['/\\<>%\$]");

    public static bool Validate(Keyword kword)
    {
        if (string.IsNullOrWhiteSpace(kword.Name)) throw new ArgumentException("Keyword must not be empty");
        
        if (validCharactersRegex.IsMatch(kword.Name))
            throw new ArgumentException("Keyword can only contain letters or numbers.");
        
        return true;
    }

    public bool Equals(Keyword? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public string ToExportString()
    {
        return Name;
    }
}