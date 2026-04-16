using Microsoft.Extensions.Options;
using Timekeeping.Api.Options;

namespace Timekeeping.Api.Services;

public interface IOfficeTimeProvider
{
    TimeZoneInfo OfficeTimeZone { get; }
    DateOnly GetWorkDateUtc(DateTime utc);
    DateTime ToOfficeLocal(DateTime utc);
}

public sealed class OfficeTimeProvider(IOptions<TimekeepingOptions> options) : IOfficeTimeProvider
{
    private readonly TimeZoneInfo _tz = TimeZoneInfo.FindSystemTimeZoneById(options.Value.OfficeTimeZoneId);

    public TimeZoneInfo OfficeTimeZone => _tz;

    public DateOnly GetWorkDateUtc(DateTime utc)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(utc, _tz);
        return DateOnly.FromDateTime(local);
    }

    public DateTime ToOfficeLocal(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(utc, _tz);
}
