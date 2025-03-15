namespace TaskBoard.Models.SnapchatActionModels;

public class PostDirectSingleSnap
{
    public long MediaFileId { get; set; }
    public string? PostDirectSwipeUpUrl { get; set; }
    public float SecondsBeforeStart { get; set; }
    public int CurrentLinkIndex { get; set; }
    public int AmountOfSnaps { get; set; }

    public string? GetRandomUrl(int rotateAmount)
    {
        Console.WriteLine(PostDirectSwipeUpUrl);
        
        if (PostDirectSwipeUpUrl is not null && PostDirectSwipeUpUrl.Split(',').Length > 0)
        {
            if (rotateAmount > 0)
            {
                if (AmountOfSnaps >= rotateAmount)
                {
                    CurrentLinkIndex++;
                }

                if (CurrentLinkIndex > PostDirectSwipeUpUrl.Split(',').Length)
                {
                    CurrentLinkIndex = 0;
                }

                AmountOfSnaps++;
                
                return PostDirectSwipeUpUrl.Split(',')[CurrentLinkIndex];
            }
            
            Random r = new();

            return PostDirectSwipeUpUrl.Split(',')[r.Next(0, PostDirectSwipeUpUrl.Split(',').Length)];
        }

        return PostDirectSwipeUpUrl;
    }
    public async Task<MediaFile?> GetMediaPath(IServiceProvider provider)
    {
        await using var context = provider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var file = await context.MediaFiles.FindAsync(MediaFileId);
        return file;
    }
}