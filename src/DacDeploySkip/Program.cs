using DacDeploySkip;

var skipper = new DacpacChecksumService();

if (args.Length >= 3 && args[0] == "check" && TryParseOptions(args, out var useFileName, out var publishProfilePath))
{
    var deployed = await skipper.CheckIfDeployedAsync(args[1], args[2], useFileName, publishProfilePath: publishProfilePath);

    return deployed ? 0 : 1;
}

if (args.Length >= 3 && args[0] == "mark" && TryParseOptions(args, out useFileName, out publishProfilePath))
{
    await skipper.SetChecksumAsync(args[1], args[2], useFileName, publishProfilePath: publishProfilePath);
    return 0;
}

Console.WriteLine("This tool helps skip deployment of a .dacpac to a SQL database if it has already been deployed.");
Console.WriteLine("https://github.com/ErikEJ/DacDeploySkip");
Console.WriteLine("Usage:");
Console.WriteLine("  dacdeployskip check \"<dacpacPath>\" \"<connectionString>\" [-namekey] [-profile \"<publishProfilePath>\"]");
Console.WriteLine("  dacdeployskip mark \"<dacpacPath>\" \"<connectionString>\" [-namekey] [-profile \"<publishProfilePath>\"]");

return 1;

static bool TryParseOptions(string[] args, out bool useFileName, out string? publishProfilePath)
{
    useFileName = false;
    publishProfilePath = null;

    for (var index = 3; index < args.Length; index++)
    {
        if (args[index].Equals("-namekey", StringComparison.OrdinalIgnoreCase))
        {
            useFileName = true;
        }
        else if (args[index].Equals("-profile", StringComparison.OrdinalIgnoreCase)
            && ++index < args.Length)
        {
            publishProfilePath = args[index];
        }
        else
        {
            return false;
        }
    }

    return true;
}
