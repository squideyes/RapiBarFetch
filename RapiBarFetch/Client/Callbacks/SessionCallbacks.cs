// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using com.omnesys.rapi;
using Serilog;
using static com.omnesys.rapi.ConnectionId;

namespace RapiBarFetch;

internal class SessionCallbacks : RCallbacks
{
    private readonly HashSet<ConnectionId> connectionIds = new();

    private readonly ILogger logger;
    private readonly IEventSink eventSink;

    private BarSet barSet = null!;

    public SessionCallbacks(ILogger logger, IEventSink eventSink)
    {
        this.logger = logger;
        this.eventSink = eventSink;
    }

    internal void InitBarSet(Job job) => barSet = new BarSet(job);

    public override void Alert(AlertInfo info)
    {
        if (info.AlertType == AlertType.LoginFailed)
        {
            logger.Warning($"Login Failed (Code: {info.RpCode}, Message: {info.Message})");

            eventSink.LoginFailed();
        }
        else if (info.AlertType == AlertType.LoginComplete)
        {
            switch (info.ConnectionId)
            {
                case MarketData:
                    logger.Debug($"LoginSuccess: {info.ConnectionId}");
                    connectionIds.Add(MarketData);
                    break;
                case History:
                    logger.Debug($"LoginSuccess: {info.ConnectionId}");
                    connectionIds.Add(History);
                    break;
                default:
                    logger.Warning($"UnexpectedLogin (ConnectionId: {info.ConnectionId}, Code: {info.RpCode}, Message: {info.Message})");
                    eventSink.LoginFailed();
                    return;
            }

            if (connectionIds.Contains(MarketData) && connectionIds.Contains(History))
                eventSink.LoginSucceeded();
        }
    }

    public override void Bar(BarInfo info)
    {
        var closeOn = Known.UnixEpoch.AddSeconds(info.EndSsboe)
            .AddMilliseconds(info.CloseSsm).ToEasternFromUtc();

        var bar = new Bar()
        {
            CloseOn = closeOn,
            Open = info.OpenPrice,
            High = info.HighPrice,
            Low = info.LowPrice,
            Close = info.ClosePrice
        };

        barSet.Add(bar);
    }

    public override void BarReplay(BarReplayInfo info)
    {
        if (barSet.Count == 0)
            eventSink.BadJobIgnored(barSet.Job);
        else
            eventSink.SaveBarSet(barSet);
    }
}