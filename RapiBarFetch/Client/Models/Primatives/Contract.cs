// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using System.Text.RegularExpressions;

namespace RapiBarFetch;

public partial class Contract : IEquatable<Contract>, IComparable<Contract>
{
    public static readonly int MinYear = 2020;
    public static readonly int MaxYear = 2029;

    private static readonly Regex parser = Parser();

    public Asset Asset { get; }
    public Month Month { get; }
    public int Year { get; }
    public List<DateOnly> TradeDates { get; } = new();

    internal Contract(Asset asset, Month month, int year)
    {
        Asset = asset;
        Month = month;
        Year = year;

        var contractMonths = asset.GetContractMonths(month);

        var current = GetRollDate(month, year);

        var prior = new DateTime(year, (int)month, 1)
            .AddMonths(-contractMonths)
            .Get(d => GetRollDate((Month)d.Month, d.Year));

        for (var date = prior; date < current; date = date.AddDays(1))
        {
            if (!Known.TradeDates.TryGetValue(date, out DateOnly tradeDate))
                continue;

            TradeDates.Add(tradeDate);
        }
    }

    public bool Equals(Contract? other)
    {
        if (other is null)
            return false;

        return AsTuple().Equals(other.AsTuple());
    }

    public override bool Equals(object? other) =>
        other is Contract contract && Equals(contract);

    override public int GetHashCode() => HashCode.Combine(Asset, Month, Year);

    public int CompareTo(Contract? other) =>
        other is null ? 1 : AsTuple().CompareTo(other.AsTuple());

    public override string ToString() =>
        $"{SymbolAs.GetRithmicSymbol(Asset.Symbol)}{Month.ToCode()}{Year - 2020}";

    private (Asset, int, Month) AsTuple() => (Asset, Year, Month);

    internal static Contract Parse(string value)
    {
        var match = parser.Match(value);

        static void ThrowParseError() =>
            throw new ArgumentOutOfRangeException(nameof(value));

        if (!match.Success)
            ThrowParseError();

        var symbol = Enum.Parse<Symbol>(match.Groups["S"].Value, true);

        if (!Known.Assets.ContainsKey(symbol))
            ThrowParseError();

        var asset = Known.Assets[symbol];
        var month = match.Groups["M"].Value[0].ToMonth();
        var year = 2000 + int.Parse(match.Groups["Y"].Value);

        return new Contract(asset, month, year);
    }

    public static bool TryParse(string value, out Contract contract) =>
        Safe.TryGetValue(() => Parse(value), out contract);

    public static bool IsValue(string value)
    {
        var match = parser.Match(value);

        if (!match.Success)
            return false;

        var symbol = Enum.Parse<Symbol>(match.Groups["S"].Value, true);

        return Known.Assets.ContainsKey(symbol);
    }

    internal static DateOnly GetRollDate(Month month, int year)
    {
        byte WEEK = 2;

        var date = new DateOnly(year, (int)month, 1);

        date = date.AddDays(-(date.DayOfWeek - DayOfWeek.Thursday));

        date = date.Day > (byte)DayOfWeek.Saturday
            ? date.AddDays(7 * WEEK) : date.AddDays(7 * (WEEK - 1));

        return date.AddDays(4);
    }

    [GeneratedRegex("^(?<S>[A-Z0-9]{2,6})(?<M>[FGHJKMNQUVXZ])(?<Y>2\\d)$")]
    private static partial Regex Parser();

    public static bool operator ==(Contract lhs, Contract rhs)
    {
        if (lhs is null)
            return rhs is null;

        return lhs.Equals(rhs);
    }

    public static bool operator !=(Contract lhs, Contract rhs) => !(lhs == rhs);

    public static bool operator <(Contract lhs, Contract rhs) => lhs.CompareTo(rhs) < 0;

    public static bool operator <=(Contract lhs, Contract rhs) => lhs.CompareTo(rhs) <= 0;

    public static bool operator >(Contract lhs, Contract rhs) => lhs.CompareTo(rhs) > 0;

    public static bool operator >=(Contract lhs, Contract rhs) => lhs.CompareTo(rhs) >= 0;
}