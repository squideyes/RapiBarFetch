// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using Microsoft.Extensions.Configuration;
using RapiBarFetch;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

if (!TryGetSettings(args, out Settings settings))
    return ExitCode.SettingsParseError;

var logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(
        theme: AnsiConsoleTheme.Code,
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    var fetcher = new Fetcher(logger, settings);

    fetcher.Fetch();

    return ExitCode.Success;
}
catch (com.omnesys.omne.om.OMException error)
{
    logger.Error(error.Message);
    
    return ExitCode.RapiError;
}
catch (Exception error)
{
    logger.Error(error.Message);

    return ExitCode.InternalError;
}

static bool TryGetSettings(string[] args, out Settings settings)
{
    var mappings = new Dictionary<string, string>()
    {
        { "-a", "Assets" },
        { "-d", "Dates" },
        { "-k", "Kinds" },
        { "-s", "Sizes" }
    };

    try
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false, false)
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables("RapiBarFetch__")
            .AddCommandLine(args, mappings)
            .Build();

        settings = new Settings(config);

        return true;
    }
    catch
    {
        DisplayUsage();

        settings = null!;

        return false;
    }
}

static void DisplayUsage()
{
    Console.WriteLine($"""
        RAPIBARFETCH [[--assets=] [--dates=] [--kinds=] [--sizes=]] | --help

           assets  Assets to download barsets for or ALL (default)
           dates   Date(s), date-range or ALL (default)
           kinds   Bar-kinds to output; NINJA and/or CSV (default)
           sizes   Bar-sizes to fetch (i.e. S30,M5; M1 = default)
        
        Only BP, CL, E7, ES, EU, GC, J7, JY, NQ, QM, QO, ZB, ZF and/or ZN bars
        may be fetched, depending upon your FCM entitlements.

        The valid set of possible dates is {Known.MinBarDate:MM/dd/yyyy} to {Known.MaxBarDate:MM/dd/yyyy}, exclusive 
        of holidays and weekends. A date-range is signified by a double-dot 
        (i.e. FROM..UNTIL). If FROM is ommitted, the minimal date will be 
        assumed. If UNTIL is omitted, the maximal date will be asssumed. Bar
        times will be converted to US/Eastern.
                
        See appsettings.json for set-and-forget values like "SaveToPath" and 
        "LogFilePath." The R|API+ "UserName" and "Password" must be sourced
        from environment variables (with an optional RapiBarFetch__" prefix), 
        UserSecrets or the command-line.
        """);
}