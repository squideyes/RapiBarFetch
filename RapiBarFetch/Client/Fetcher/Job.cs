// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using Ardalis.GuardClauses;
using com.omnesys.rapi;
using System.Text;

namespace RapiBarFetch;

public class Job
{
    internal Job(Asset asset, DateOnly tradeDate, BarSize barSize, BarKind[] barKinds)
    {
        Asset = asset;
        TradeDate = tradeDate;
        BarSize = barSize;
        BarKinds = barKinds;
    }

    public Asset Asset { get; }
    public DateOnly TradeDate { get; }
    public BarSize BarSize { get; }
    public BarKind[] BarKinds { get; }

    public override string ToString() =>
        $"{Asset},{TradeDate:yyyyMMdd},{BarSize},{string.Join("+", BarKinds)}";

    public string GetFileName(BarKind barKind)
    {
        var sb = new StringBuilder();

        sb.Append("RAPI");
        sb.AppendDelimited(Asset.Symbol, '_');
        sb.AppendDelimited(BarSize, '_');
        sb.AppendDelimited(TradeDate.ToString("yyyyMMdd"), '_');
        sb.AppendDelimited(barKind.ToString().ToUpper(), '_');
        sb.Append("_EST.csv");

        return sb.ToString();
    }

    public string GetFullPath(string basePath, bool inFolders, BarKind barKind)
    {
        if (!inFolders)
            return Path.Combine(basePath, GetFileName(barKind));

        return Path.GetFullPath(Path.Combine(
            basePath,
            barKind.ToString().ToUpper(),
            BarSize.ToString(),
            Asset.ToString(),
            TradeDate.Year.ToString(),
            GetFileName(barKind)));
    }

    public ReplayBarParams GetParams(Job job)
    {
        var (from, until) = GetFromUntil(TradeDate);

        static int GetUnixTime(DateTime dateTime) =>
            (int)dateTime.Subtract(Known.UnixEpoch).TotalSeconds;

        var contract = Known.GetContract(job.Asset, job.TradeDate);

        var rbp = new ReplayBarParams
        {
            Context = job,
            EndSsboe = GetUnixTime(until),
            EndUsecs = until.Millisecond,
            Exchange = Asset.Exchange.ToString(),
            StartSsboe = GetUnixTime(from),
            Symbol = contract.ToString()
        };

        if (BarSize.Period == Period.Seconds)
        {
            rbp.Type = BarType.Second;
            rbp.SpecifiedSeconds = 1;
        }
        else
        {
            rbp.Type = BarType.Minute;
            rbp.SpecifiedMinutes = 1;
        }

        return rbp;
    }

    private static (DateTime, DateTime) GetFromUntil(DateOnly tradeDate)
    {
        Guard.Against.InvalidInput(tradeDate,
            nameof(tradeDate), v => Known.TradeDates.Contains(v));

        var anchor = tradeDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);

        var from = anchor.AddHours(-6);
        var until = anchor.Add(new TimeSpan(0, 16, 59, 59, 999));

        return (from.ToUtcFromEastern(), until.ToUtcFromEastern());
    }
}