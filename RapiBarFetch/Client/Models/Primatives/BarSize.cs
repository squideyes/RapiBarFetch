// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using System.Text.RegularExpressions;
using Ardalis.GuardClauses;

namespace RapiBarFetch;

public partial class BarSize
{
    private static readonly Regex parser = GetParser();

    private BarSize(Period period, int quantity)
    {
        Period = period;
        Quantity = quantity;
    }

    public Period Period { get; }
    public int Quantity { get; }

    public override string ToString() => $"{Period.ToCode()}{Quantity}";

    public static BarSize Create(Period period, int quantity)
    {
        Guard.Against.EnumOutOfRange(period);
        Guard.Against.OutOfRange(quantity, nameof(quantity), 1, 60);

        return new BarSize(period, quantity);
    }

    public static BarSize Parse(string value)
    {
        var match = parser.Match(value);

        var period = match.Groups["P"].Value switch
        {
            "S" => Period.Seconds,
            "M" => Period.Minutes,
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };

        int quantity = int.Parse(match.Groups["N"].Value);

        return Create(period, quantity);
    }

    [GeneratedRegex(@"^(?<P>(S|M))(?<N>\d{1,3})$", RegexOptions.IgnoreCase)]
    private static partial Regex GetParser();
}