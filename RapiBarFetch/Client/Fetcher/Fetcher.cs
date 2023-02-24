// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using Ardalis.GuardClauses;
using com.omnesys.omne.om;
using com.omnesys.rapi;
using Serilog;
using static Serilog.Events.LogEventLevel;

namespace RapiBarFetch;

internal class Fetcher : IEventSink
{
    private readonly Queue<Job> jobs = new();
    private readonly AutoResetEvent shutdownGate = new(false);

    private readonly ILogger logger;
    private readonly Settings settings;

    private REngine engine = null!;
    private SessionCallbacks session = null!;

    public Fetcher(ILogger logger, Settings settings)
    {
        this.logger = Guard.Against.Null(logger);
        this.settings = Guard.Against.Null(settings);

        var q = from asset in settings.Assets
                from tradeDate in settings.TradeDates
                from barSize in settings.BarSizes
                select (asset, tradeDate, barSize);

        int skipped = 0;

        foreach (var (asset, tradeDate, barSize) in q)
        {
            var job = new Job(asset, tradeDate, barSize, settings.BarKinds);

            int count = 0;

            foreach (var barKind in job.BarKinds)
            {
                var fullPath = job.GetFullPath(
                    settings.SaveToPath, settings.InFolders, barKind);

                if (File.Exists(fullPath))
                    count++;
            }

            if (count == settings.BarKinds.Length)
            {
                skipped++;
                continue;
            }

            settings.Act(s => jobs.Enqueue(job));
        }

        logger.Write(jobs.Count == 0 ? Warning : Information,
            $"{jobs.Count:N0} Jobs Enqueued, {skipped:N0} Skipped");
    }

    public void Fetch()
    {
        if (jobs.Count == 0)
        {
            shutdownGate.Set();

            return;
        }

        session = new SessionCallbacks(logger, this);

        var now = DateTime.UtcNow;

        if (!Directory.Exists(settings.LogFilePath))
            Directory.CreateDirectory(settings.LogFilePath);

        var logFilePath = Path.GetFullPath(Path.Combine(
            settings.LogFilePath, $"RapiBarFetch_{now:yyyyMMdd_HHmmss}.log"));

        logger.Debug($"See \"{logFilePath}\" for the R|API+ engine log");

        var engineParams = new REngineParams
        {
            AppName = typeof(Fetcher).Namespace,
            AppVersion = "1.0.0",
            AdmCallbacks = new AdminCallbacks(logger),
            LogFilePath = logFilePath,
            DmnSrvrAddr = settings.DmnSrvrAddr,
            DomainName = settings.DomainName,
            LicSrvrAddr = settings.LicSrvrAddr,
            LocBrokAddr = settings.LocBrokAddr,
            LoggerAddr = settings.LoggerAddr
        };

        try
        {
            engine = new REngine(engineParams);

            logger.Debug("EngineStarted");

            engine.login(session,
                 Constants.DEFAULT_ENVIRONMENT_KEY,
                 settings.UserName,
                 settings.Password,
                 "login_agent_tpc",
                 Constants.DEFAULT_ENVIRONMENT_KEY,
                 string.Empty,
                 string.Empty,
                 string.Empty,
                 string.Empty,
                 Constants.DEFAULT_ENVIRONMENT_KEY,
                 settings.UserName,
                 settings.Password,
                 "login_agent_historyc");

            shutdownGate.WaitOne();

            logger.Debug("ShutdownInitiated");

            engine.logout();

            engine.shutdown();

            logger.Debug("ShutdownCompleted");
        }
        catch (OMException error)
        {
            ErrorAndSet($"OMException: {error.Message}");
        }
        catch (Exception error)
        {
            ErrorAndSet($"InternalError: {error.Message}");
        }
    }

    private void FetchAndSave()
    {
        if (jobs.Count > 0)
        {
            var job = jobs.Dequeue();

            session.InitBarSet(job);

            try
            {
                engine.replayBars(job.GetParams(job));
            }
            catch (OMException error)
            {
                ErrorAndSet($"OMException: {error.Message} (Job: {job})");
            }
            catch (Exception error)
            {
                ErrorAndSet($"InternalError: {error.Message} (Job: {job})");
            }
        }
        else
        {
            shutdownGate.Set();
        }
    }

    private void ErrorAndSet(string message)
    {
        logger.Error(message);

        shutdownGate.Set();
    }

    void IEventSink.SaveBarSet(BarSet barSet)
    {
        barSet.Save(logger, settings.SaveToPath, true);

        FetchAndSave();
    }

    void IEventSink.BadJobIgnored(Job job)
    {
        logger.Warning($"BadJobIgnored: {job}");

        FetchAndSave();
    }

    void IEventSink.LoginFailed() => shutdownGate.Set();

    void IEventSink.LoginSucceeded() => FetchAndSave();
}