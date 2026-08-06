namespace SargentNexus.Client.Workflow;

public interface IClientClock
{
    string FormatLocalDayAge(DateTime createdAtUtc);
}

public sealed class ClientClock : IClientClock
{
    private readonly TimeProvider _timeProvider;

    public ClientClock(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public string FormatLocalDayAge(DateTime createdAtUtc)
    {
        return LocalDayAgeFormatter.Format(createdAtUtc, _timeProvider.GetUtcNow(), TimeZoneInfo.Local);
    }
}

public static class LocalDayAgeFormatter
{
    public static string Format(DateTime createdAtUtc, DateTimeOffset nowUtc, TimeZoneInfo timeZone)
    {
        var normalizedCreatedAtUtc = createdAtUtc.Kind == DateTimeKind.Utc
            ? createdAtUtc
            : DateTime.SpecifyKind(createdAtUtc, DateTimeKind.Utc);
        var createdDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(normalizedCreatedAtUtc, timeZone));
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(nowUtc, timeZone).DateTime);
        var days = Math.Max(0, today.DayNumber - createdDate.DayNumber);

        return days == 1 ? "1 day ago" : $"{days} days ago";
    }
}