namespace TaskBoard;

public static class TimeUtilities
{
    public static string ReadableTimeLeft(TimeSpan timespan)
    {
        var duration = timespan.Duration();
        
        // TotalDays and all such variables reports 0.X if there are no days
        // so for our effect, we need to compare to 1 otherwise it prints stuff like
        // 00 days
        if (duration.TotalDays > 1)
        {
            var val = duration.ToString("dd");
            return $"{val} day(s)";
        }

        if (duration.TotalHours > 1)
        {
            var hours = duration.ToString("hh");
            return $"{hours} hour(s)";
        }

        if (duration.TotalMinutes > 1)
        {
            var minutes = duration.ToString("mm");
            return $"{minutes} minute(s)";
        }

        var seconds = duration.ToString("ss");
        return $"{seconds} second(s)";
    }
}