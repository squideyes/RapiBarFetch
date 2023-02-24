// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;

namespace RapiBarFetch;

public partial class Settings
{
    public Settings(IConfiguration config)
    {
        Asset[] GetAssets()
        {
            var value = config["Assets"]!;

            if (IsAll(value))
                return Known.Assets.Values.ToArray();

            return value.Split(',').Select(Enum.Parse<Symbol>)
                .Select(s => Known.Assets[s]).ToArray();
        }

        bool GetBool(string key) =>
            config[key].Get(v => v is not null && bool.Parse(v));

        string GetTrimmedValue(string key) =>
            Guard.Against.NullOrWhiteSpace(config[key]).Trim();

        BarSize[] GetBarSizes()
        {
            var value = config["Sizes"]!;

            if (string.IsNullOrWhiteSpace(value))
                return new BarSize[] { BarSize.Create(Period.Minutes, 1) };

            var sizes = value.Split(',').Select(BarSize.Parse).ToArray();

            Guard.Against.OutOfRange(sizes.Length, nameof(value), 1, 10);

            return sizes;
        }

        BarKind[] GetBarKinds()
        {
            var value = config["Kinds"]!;

            if (value == null)
            {
                return new BarKind[] { BarKind.CSV };
            }
            else
            {
                return value.Split(',').Select(
                    v => Enum.Parse<BarKind>(v, true)).ToArray();
            }
        }

        DateOnly[] GetTradeDates()
        {
            var value = config["Dates"]!;

            if (IsAll(value))
                return Known.TradeDates.ToArray();

            var tradeDates = new List<DateOnly>();

            if (value.Contains(".."))
            {
                var index = value.IndexOf("..");

                if (index == -1)
                    throw new ArgumentOutOfRangeException(nameof(value));

                var minDateString = value[..index];
                var maxDateString = value[(index + 2)..];

                if (!DateOnly.TryParse(minDateString, out DateOnly minDate))
                    minDate = Known.MinBarDate;

                if (minDate < Known.MinBarDate)
                    minDate = Known.MinBarDate;

                if (!DateOnly.TryParse(maxDateString, out DateOnly maxDate))
                    maxDate = Known.MaxBarDate;

                if (maxDate > Known.MaxBarDate)
                    maxDate = Known.MaxBarDate;

                Guard.Against.InvalidInput(
                    minDate, nameof(minDate), v => v <= maxDate);

                for (var date = minDate; date <= maxDate; date = date.AddDays(1))
                {
                    if (Known.TradeDates.Contains(date))
                        tradeDates.Add(date);
                }
            }
            else
            {
                tradeDates.AddRange(value.Split(',').Select(DateOnly.Parse)
                    .Where(Known.TradeDates.Contains).ToHashSet());
            }

            return tradeDates.ToArray();
        }

        string GetValidatedPath(string key)
        {
            var value = Guard.Against.NullOrWhiteSpace(config[key]!);

            if (value.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                throw new ArgumentOutOfRangeException(nameof(value));

            return value;
        }

        Assets = GetAssets();
        BarSizes = GetBarSizes();
        DmnSrvrAddr = GetTrimmedValue("DmnSrvrAddr");
        DomainName = GetTrimmedValue("DomainName");
        BarKinds = GetBarKinds();
        InFolders = GetBool("InFolders");
        LicSrvrAddr = GetTrimmedValue("LicSrvrAddr");
        LocBrokAddr = GetTrimmedValue("LocBrokAddr");
        LogFilePath = GetValidatedPath("LogFilePath");
        LoggerAddr = GetTrimmedValue("LoggerAddr");
        Password = GetTrimmedValue("Password");
        SaveToPath = GetValidatedPath("SaveToPath");
        TradeDates = GetTradeDates();
        UserName = GetTrimmedValue("UserName");
    }

    public Asset[] Assets { get; }
    public DateOnly[] TradeDates { get; }
    public BarSize[] BarSizes { get; }
    public BarKind[] BarKinds { get; }
    public string UserName { get; }
    public string Password { get; }
    public string SaveToPath { get; }
    public bool InFolders { get; }
    public string DmnSrvrAddr { get; }
    public string DomainName { get; }
    public string LicSrvrAddr { get; }
    public string LocBrokAddr { get; }
    public string LoggerAddr { get; }
    public string LogFilePath { get; }

    private static bool IsAll(string value)
    {
        return value is null || value.Equals(
            "ALL", StringComparison.OrdinalIgnoreCase);
    }
}