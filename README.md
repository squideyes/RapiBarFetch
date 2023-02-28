**RAPIBARFETCH** downloads historical minute and second bars via Rithmic's R|API+, and then it processes those bars into daily bar files; in a variety of formats.

It should be noted from the onset that the codebase lacks both unit and production tests, and as such, **RAPIBARFETCH SHOULD NOT BE USED IN PRODUCTION**!!!

A key motivation behind the creation of RAPIBARFETCH was the fact that Rithmic's own demo was fairly long in the tooth (it was  written for Visual Studio 2010!)  

As a contrast, RAPIBARFETCH is a high-quality modern example of how to work with R|API+ using C# 11 and .NET 7.0.  It does leverage the (.NET Framework 3.5-based) rapiplus.dll, but the "old" assembly appears to work just fine in a modern context; including cloud-based ASP.NET core applications.

In order to compile RAPIBARFETCH, **you'll need to obtain a copy of rapiplus.dll** from the Rithmic folks and then include the assembly in the RAPIBARFETCH project.  By convention, the .dll should be kept under the project in a "Resources" folder.

Before running RAPIBARFETCH, you'll need to set your R\|API+ Username & Password via a pair of Environment Variables (**EV**).  A number of set-and-forget values may also be specified in appsettings.json (**AS**).  Environment Variable names may be (optionally) prefixed by "RapiBarFetch__".

|Setting|Via|Notes|
|---|---|---|
|**UserName**|EV|An R\|API+ user-name|
|**Password**|EV|An R\|API+ password|
|**LogFilePath**|AS|The path to save log files to (i.e. Logs)|
|**SaveToPath**|AS|The path to save bar-set files to (i.e. RithmicData)|
|**InFolders**|AS|If true, the saved files will be organized in sub-folders|
|**DmnSrvrAddr**|AS|See the R\|API+ documentation for details|
|**DomainName**|AS|See the R\|API+ documentation for details|
|**LicSrvrAddr**|AS|See the R\|API+ documentation for details|
|**LocBrokAddr**|AS|See the R\|API+ documentation for details|
|**LoggerAddr**|AS|See the R\|API+ documentation for details|

To run RAPIBARFETCH, compile the code, using a recent version of  Visual Studio or Visual Studio Code, and then invoke the app via the command-line; per the following:

```plaintext
RAPIRAPIBARFETCH [[--assets=] [--dates=] [--kinds=] [--sizes=]] | --help

   assets  Assets to download barsets for or ALL (default)
   dates   Date(s), date-range or ALL (default)
   kinds   Bar-kinds to output; NINJA and/or CSV (default)
   sizes   Bar-sizes to fetch (i.e. S30,M5; M1 = default)

Only BP, CL, E7, ES, EU, GC, J7, JY, NQ, QM, QO, ZB, ZF and/or ZN bars
may be fetched, depending upon your FCM entitlements.

The valid set of possible dates is {MinDate} to {MaxDate}, exclusive 
of holidays and weekends. A date-range is signified by a double-dot 
(i.e. FROM..UNTIL). If FROM is ommitted, the minimal date will be 
assumed. If UNTIL is omitted, the maximal date will be asssumed. Bar
times will be converted to US/Eastern.
        
See appsettings.json for set-and-forget values like "SaveToPath" and 
"LogFilePath." The R|API+ "UserName" and "Password" must be sourced
from environment variables (with an optional RapiRAPIBARFETCH__" prefix), 
UserSecrets or the command-line.
```

**CAVEATS**: RapiFetchDemo is somewhat idiosyncratic in that it only fetches bars for a limited set of futures contracts, and it only assembles those bars into daily "bar-sets" for US/Eastern consumption.  Each bar-set gets fetched using a pre-calculated contract, and no effort has been made to make the data continuous.  Most notably, the code has only been tested using Rithmic's demo feed.  As such, it might seem that there's a bunch of missing data, but that's actually an artifact of fetching data from a fairly sparse testing feed.

**PERFORMANCE**: The main processing loop of the program has been deliberately designed to be queue-driven and NOT MULTI-THREADED, even though that would be a trivial thing to code.  If adopting this code for your own use, please give some consideration to NOT overloading the Rithmic history servers.

**CONTRIBUTING**: Pull-requests and constructive comments are always welcome.  Furthermore, the author is developing a R|API+ client to serve as a key component of a new trading platform.  That code is still ind development, but it's progressing nicely.  An open-source version of that code will be released when its more fully baked, but in the meantime it would help to have a colabborator or two.