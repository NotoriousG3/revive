using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TaskBoard;

[Flags]
public enum RandomOptions
{
    UseLowercase = 1 << 0,
    UseUppercase = 1 << 1,
    UseNumbers = 1 << 2
}

public static class EnumerableExtension
{
    public static T PickRandom<T>(this IEnumerable<T> source)
    {
        return source.PickRandom(1).Single();
    }

    public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
    {
        return source.Shuffle().Take(count);
    }

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        return source.OrderBy(x => Guid.NewGuid());
    }
    
    static Random _R = new Random();
    public static T RandomEnumValue<T>()
    {
        var v = Enum.GetValues(typeof(T));
        return (T)v.GetValue(_R.Next(v.Length));
    }
}

public class Utilities
{
    private static readonly string _numbers = "123456789";
    private static readonly string _letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private readonly Random _random;
    public static string ConnectionString = $"Server={Environment.GetEnvironmentVariable("MYSQL_IP")}; Port=3306; Uid={Environment.GetEnvironmentVariable("MYSQL_LOGIN")}; Pwd={Environment.GetEnvironmentVariable("MYSQL_PASSWORD")}; Database={Environment.GetEnvironmentVariable("MYSQL_DATABASE")}; Max Pool Size=2000";
    public static List<string> ArabicCountries = new List<string>() { "MY","TR","BH","IQ","JO","KW","LB","OM","QA","SA","SY","AE","YE","AF","EG","MA","SD" };
    
    public static long BytesToMbConversionLiteral = 1048576;

    public Utilities()
    {
        _random = new Random();
    }

    public int RandomInt(int min, int max)
    {
        return _random.Next(min, max);
    }

    public async Task<double> GetCpuUsageForProcess()
    {
        var startTime = DateTime.UtcNow;
        var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
        await Task.Delay(500);

        var endTime = DateTime.UtcNow;
        var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
        var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
        var totalMsPassed = (endTime - startTime).TotalMilliseconds;
        var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
        return cpuUsageTotal * 100;
    }

    public double GetRamUsage()
    {
        Process currentProc = Process.GetCurrentProcess();
        var allocationInMB = currentProc.PrivateMemorySize64 / (1024 * 1024);
        return allocationInMB;
    }
    
    public string GenerateRandomPhoneNumber(string country)
    {
        var lorem = new Bogus.DataSets.PhoneNumbers(locale: country);
        
        var format = lorem.PhoneNumberFormat(2);
        
        return lorem.PhoneNumber(format);
    }
    
    public string Random(int length, RandomOptions options = RandomOptions.UseLowercase | RandomOptions.UseNumbers | RandomOptions.UseUppercase)
    {
        var characters = new List<string>();
        if (options.HasFlag(RandomOptions.UseLowercase))
            characters.Add(_letters.ToLower());

        if (options.HasFlag(RandomOptions.UseUppercase))
            characters.Add(_letters);

        if (options.HasFlag(RandomOptions.UseNumbers))
            characters.Add(_numbers);

        var chars = string.Join("", characters);
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
    }

    public int RandomNext(int max)
    {
        return _random.Next(max);
    }

    public bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Normalize the domain
            email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                RegexOptions.None, TimeSpan.FromMilliseconds(200));

            // Examines the domain part of the email and normalizes it.
            string DomainMapper(Match match)
            {
                // Use IdnMapping class to convert Unicode domain names.
                var idn = new IdnMapping();

                // Pull out and process domain name (throws ArgumentException on invalid)
                var domainName = idn.GetAscii(match.Groups[2].Value);

                return match.Groups[1].Value + domainName;
            }
        }
        catch (RegexMatchTimeoutException e)
        {
            return false;
        }
        catch (ArgumentException e)
        {
            return false;
        }

        try
        {
            return Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    public List<HashSet<string>> SplitHash(HashSet<string> items, int sliceSize = 30)
    {
        List<HashSet<string>> list = new List<HashSet<string>>();
        int i = 0;
        while (i < items.Count)
        {
            HashSet<string> set = new HashSet<string>();
            foreach (var item in items.Skip(i).Take(sliceSize))
            {
                set.Add(item);
            }
            list.Add(set);
            i += sliceSize;
        }
        return list;
    }
    
    public List<List<T>> Split<T>(List<T> items, int sliceSize = 200)
    {
        List<List<T>> list = new List<List<T>>();
        for (int i = 0; i < items.Count; i += sliceSize)
            list.Add(items.GetRange(i, Math.Min(sliceSize, items.Count - i)));
        return list;
    }
    
    public List<List<T>> SplitHash<T>(HashSet<T> items, int sliceSize = 200)
    {
        List<T> itemsList = items.ToList();
        List<List<T>> list = new List<List<T>>();
        for (int i = 0; i < itemsList.Count; i += sliceSize)
            list.Add(itemsList.GetRange(i, Math.Min(sliceSize, itemsList.Count - i)));
        return list;
    }
    
    public static double BytesToMb(long bytes)
    {
        //1024^2 == 1048676
        return bytes / BytesToMbConversionLiteral ;
    }

    public static string BytesToString(long bytes)
    {
        string[] suf = { "B", "Kb", "Mb", "Gb", "Tb", "Pb", "Eb" };
        if (bytes == 0)
            return "0" + suf[0];
        var absoluteBytes = Math.Abs(bytes);
        var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        var num = Math.Round(bytes / Math.Pow(1024, place), 1);
        return (Math.Sign(absoluteBytes) * num) + suf[place];
    }
}