namespace SnapWebManager;

public class TimeProvider
{
    public virtual DateTime UtcNow()
    {
        return DateTime.UtcNow;
    }
}