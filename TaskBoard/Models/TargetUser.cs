using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace TaskBoard.Models;

public class TargetUser : IEquatable<TargetUser>
{
    [Key]
    public long Id { get; set; }
    public string Username { get; set; }
    public string? UserID { get; set; }
    public string? CountryCode { get; set; }
    public string? Gender { get; set; }
    public string? Race { get; set; }
    public bool Added { get; set; }
    public bool Used { get; set; } = false;
    public bool Searched { get; set; } = false;

    private static Regex validCharactersRegex = new(@"^[a-zA-Z0-9\._\-]+$");

    public static bool Validate(TargetUser user)
    {
        if (string.IsNullOrWhiteSpace(user.Username)) throw new ArgumentException("Username must not be empty");
        if (!validCharactersRegex.IsMatch(user.Username))
            throw new ArgumentException("Username can only contain letters, numbers or underscore (_) characters");
        return true;
    }

    public bool Equals(TargetUser? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Username == other.Username;
    }

    public override int GetHashCode()
    {
        return Username.GetHashCode();
    }

    public string ToExportString()
    {
        return Username + ":" + UserID + ":" + CountryCode + ":" + Gender + ":" + Race;
    }
}