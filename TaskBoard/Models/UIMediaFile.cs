namespace TaskBoard.Models;

public class UIMediaFile
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string SizeString { get; set; }
    public long UsedByRunning { get; set; }
    public long UsedByScheduled { get; set; }

    public static IEnumerable<UIMediaFile> ToEnumerable(IEnumerable<MediaFile> files, WorkRequestTracker tracker)
    {
        var result = new List<UIMediaFile>();
        foreach (var f in files)
        {
            result.Add(new()
            {
                Id = f.Id,
                Name = f.Name,
                SizeString = f.SizeString,
                UsedByScheduled = f.UsedByScheduled,
                UsedByRunning = f.WorkRequests?.Count(w => tracker.GetTrackedWork(w, out _)) ?? 0
            });
        }

        return result;
    }
}