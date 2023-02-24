// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using RapiBarFetch.Models;
using System.Collections.Immutable;

namespace RapiBarFetch;

using CBATD = Dictionary<(Asset, DateOnly), Contract>;

public static class Known
{
    public static readonly DateTime UnixEpoch = new(1970, 1, 1);
    public static readonly DateOnly MinTradeDate = new(2019, 12, 16);

    private static readonly CBATD cbatd;

    static Known()
    {
        TradeDates = GetTradeDates();
        MinBarDate = TradeDates.First();
        MaxBarDate = TradeDates[^3];
        Assets = GetAssets();
        Contracts = GetContracts(Assets.Values.ToList());

        cbatd = GetContractsByAssetTradeDate();
    }

    public static ImmutableSortedSet<DateOnly> TradeDates { get; }
    public static DateOnly MinBarDate { get; }
    public static DateOnly MaxBarDate { get; }
    public static IReadOnlyDictionary<Symbol, Asset> Assets { get; }
    public static IReadOnlyDictionary<Asset, List<Contract>> Contracts { get; }

    public static Contract GetContract(Asset asset, DateOnly tradeDate) =>
        cbatd[(asset, tradeDate)];

    private static Dictionary<Symbol, Asset> GetAssets()
    {
        var assets = new List<Asset>();

        void Add(Symbol symbol, Exchange exchange, string months, float oneTick) =>
            assets.Add(new Asset(symbol, exchange, months, oneTick));

        Add(Symbol.ES, Exchange.CME, "HMUZ", 0.25f);
        Add(Symbol.NQ, Exchange.CME, "HMUZ", 0.25f);
        Add(Symbol.CL, Exchange.NYMEX, "FGHJKMNQUVXZ", 0.01f);
        Add(Symbol.QM, Exchange.NYMEX, "FGHJKMNQUVXZ", 0.01f);
        Add(Symbol.ZB, Exchange.CBOT, "HMUZ", 0.03125f);
        Add(Symbol.ZN, Exchange.CBOT, "HMUZ", 0.015625f);
        Add(Symbol.GC, Exchange.COMEX, "GJMQVZ", 0.1f);
        Add(Symbol.QO, Exchange.COMEX, "FGJMQVZ", 0.25f);
        Add(Symbol.EU, Exchange.CME, "HMUZ", 0.00005f);
        Add(Symbol.E7, Exchange.CME, "HMUZ", 0.0001f);
        Add(Symbol.JY, Exchange.CME, "HMUZ", 0.0000005f);
        Add(Symbol.J7, Exchange.CME, "HMUZ", 0.000001f);
        Add(Symbol.BP, Exchange.CME, "HMUZ", 0.0001f);
        Add(Symbol.ZF, Exchange.CBOT, "HMUZ", 0.0078125f);

        return assets.ToDictionary(a => a.Symbol);
    }

    private static ImmutableSortedSet<DateOnly> GetTradeDates()
    {
        var holidays = HolidayHelper.GetHolidays();

        var dates = new SortedSet<DateOnly>();

        var maxTradeDate = DateOnly.FromDateTime(DateTime.UtcNow.ToLocalTime());

        for (var date = MinTradeDate; date <= maxTradeDate; date = date.AddDays(1))
        {
            if (date.IsWeekday() && !holidays.Contains(date))
                dates.Add(date);
        }

        return dates.ToImmutableSortedSet();
    }

    private static Dictionary<Asset, List<Contract>> GetContracts(List<Asset> assets)
    {
        static List<Contract> GetContracts(Asset asset)
        {
            var contracts = new List<Contract>();

            for (int year = Contract.MinYear; year <= Contract.MaxYear; year++)
            {
                foreach (var month in asset.Months!)
                    contracts.Add(new Contract(asset, month, year));
            }

            return contracts;
        }

        var contracts = new Dictionary<Asset, List<Contract>>();

        foreach (var asset in assets)
            contracts.Add(asset, GetContracts(asset));

        return contracts;
    }

    private static CBATD GetContractsByAssetTradeDate()
    {
        var cbatd = new CBATD();

        foreach (var asset in Assets.Values)
        {
            foreach (var contract in Contracts[asset])
            {
                foreach (var tradeDate in contract.TradeDates)
                    cbatd.Add((asset, tradeDate), contract);
            }
        }

        return cbatd;
    }
}