// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using com.omnesys.rapi;
using Serilog;
using System.Text;

namespace RapiBarFetch;

internal class AdminCallbacks : AdmCallbacks
{
    private readonly ILogger logger;

    internal AdminCallbacks(ILogger logger)
    {
        this.logger = logger;
    }

    public override void Alert(AlertInfo info)
    {
        var sb = new StringBuilder();

        sb.Append("ADMIN ALERT (");
        sb.Append($"AlertType: {info.AlertType}");
        sb.Append($"Code: {info.RpCode}");
        sb.Append($"Message: {info.Message}");

        if (!string.IsNullOrWhiteSpace(info.Symbol))
        {
            sb.Append($"Exchange: {info.Exchange}");
            sb.Append($"Symbol: {info.Symbol}");
        }

        sb.Append($"ConnectionId: {info.ConnectionId}");
        sb.Append(")");

        logger.Warning(sb.ToString());
    }
}