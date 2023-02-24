// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using RapiBarFetch.Models;
using System.Collections.Immutable;

namespace RapiBarFetch;

public class Asset
{
    internal Asset(Symbol symbol, Exchange exchange, string months, float oneTick)
    {
        Exchange = exchange;
        Symbol = symbol;
        Months = months.Select(m => m.ToMonth()).ToImmutableSortedSet();
        OneTick = oneTick;
    }

    public Symbol Symbol { get; }
    public Exchange Exchange { get; }
    public ImmutableSortedSet<Month> Months { get; }
    public float OneTick { get; }

    public override string ToString() => Symbol.ToString();
}