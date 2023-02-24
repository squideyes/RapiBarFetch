// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using System.Text;
using static System.DayOfWeek;

namespace RapiBarFetch;

public static class MiscExtenders
{
    private static readonly string MONTH_CODES = "FGHJKMNQUVXZ";

    public static string ToCode(this Month month) =>
        MONTH_CODES[(int)month - 1].ToString();

    public static Month ToMonth(this char value) =>
        (Month)(MONTH_CODES.IndexOf(value) + 1);

    public static int GetContractMonths(this Asset asset, Month month)
    {
        var months = asset.Months!.ToList();

        var index = months.IndexOf(month);

        if (index > 0)
            return month - months[index - 1];
        else
            return (int)months[0] + (MONTH_CODES.Length - (int)months.Last());
    }

    public static void Act<T>(this T value, Action<T> action) => action(value);

    public static R Get<T, R>(this T value, Func<T, R> func) => func(value);

    public static string ToCode(this Period period)
    {
        return period switch
        {
            Period.Seconds => "S",
            Period.Minutes => "M",
            _ => throw new ArgumentOutOfRangeException(nameof(period))
        };
    }

    public static void AppendDelimited<T>(
        this StringBuilder sb, T value, char delimiter = ',')
    {
        if (sb.Length > 0)
            sb.Append(delimiter);

        sb.Append(value);
    }

    public static void EnsureFolderExists(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentOutOfRangeException(nameof(value));

        var folder = Path.GetDirectoryName(value);

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder!);
    }

    public static bool IsWeekday(this DateOnly date) =>
        date.DayOfWeek >= Monday && date.DayOfWeek <= Friday;
}