using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskBoard.Models;

public class MediaFile
{
    [Key]
    public long Id { get; set; }
    public string Name { get; set; }
    public string ServerPath { get; set; }
    public long SizeBytes { get; set; }
    public double SizeMb => Utilities.BytesToMb(SizeBytes);
    public string SizeString => Utilities.BytesToString(SizeBytes);
    public DateTime LastAccess { get; set; }
    
    public ICollection<WorkRequest>? WorkRequests { get; set; }

    [NotMapped]
    public long UsedByScheduled => WorkRequests?.Count(r => r.IsScheduled) ?? 0;
    public long UsedByRunning(WorkRequestTracker workTracker) => WorkRequests?.Count(w => workTracker.GetTrackedWork(w, out _)) ?? 0;
}