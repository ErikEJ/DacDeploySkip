// Commands: dacdeployskip check "dacpacPath" "connectionString"
// Commands: dacdeployskip mark "dacpacPath" "connectionString"
using DacDeploySkip;

var skipper = new DacpacChecksumService();

if ((args.Length == 3 || args.Length == 4) && args[0] == "check")
{
    bool useFileName = false;
    if (args.Length == 4 && args[3].Equals("-namekey", StringComparison.OrdinalIgnoreCase))
    {
        useFileName = true;
    }

    var deployed = await skipper.CheckIfDeployedAsync(args[1], args[2], useFileName);

    return deployed ? 0 : 1;
}

if ((args.Length == 3 || args.Length == 4) && args[0] == "mark")
{
    bool useFileName = false;
    if (args.Length == 4 && args[3].Equals("-namekey", StringComparison.OrdinalIgnoreCase))
    {
        useFileName = true;
    }

    await skipper.SetChecksumAsync(args[1], args[2], useFileName);
    return 0;
}

Console.WriteLine("No valid arguments provided.");
Console.WriteLine("Usage:");
Console.WriteLine("  dacdeployskip check \"<dacpacPath>\" \"<connectionString>\"");
Console.WriteLine("  dacdeployskip mark \"<dacpacPath>\" \"<connectionString>\"");

return 1;
