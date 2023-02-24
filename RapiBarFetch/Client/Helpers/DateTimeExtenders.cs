// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using Ardalis.GuardClauses;
using NodaTime;
using NodaTime.TimeZones;
using static NodaTime.TimeZones.Resolvers;

namespace RapiBarFetch;

public static class DateTimeExtenders
{
    private static readonly DateTimeZone easternTimeZone =
        DateTimeZoneProviders.Tzdb.GetZoneOrNull("America/New_York")!;

    private static readonly DateTimeZone utcTimeZone =
        DateTimeZoneProviders.Tzdb.GetZoneOrNull("UTC")!;

    private static readonly ZoneLocalMappingResolver resolver =
        CreateMappingResolver(ReturnLater, ReturnStartOfIntervalAfter);

    public static DateTime ToUtcFromEastern(this DateTime value)
    {
        Guard.Against.InvalidInput(
            value, nameof(value), v => v.Kind == DateTimeKind.Unspecified);

        var local = LocalDateTime.FromDateTime(value);

        var zoned = easternTimeZone.ResolveLocal(local, resolver);

        return zoned.ToDateTimeUtc();
    }

    public static DateTime ToEasternFromUtc(this DateTime value)
    {
        Guard.Against.InvalidInput(
            value, nameof(value), v => v.Kind != DateTimeKind.Utc);

        var local = LocalDateTime.FromDateTime(value);
        
        var zoned = utcTimeZone.ResolveLocal(local, resolver);
        
        var eastern = zoned.ToInstant().InZone(easternTimeZone);

        return eastern.ToDateTimeUnspecified();
    }
}