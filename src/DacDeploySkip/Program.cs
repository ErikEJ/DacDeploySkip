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

Console.WriteLine("This tool helps skip deployment of a .dacpac to a SQL database if it has already been deployed.");
Console.WriteLine("https://github.com/ErikEJ/DacDeploySkip");
Console.WriteLine("Usage:");
Console.WriteLine("  dacdeployskip check \"<dacpacPath>\" \"<connectionString>\" [-namekey]");
Console.WriteLine("  dacdeployskip mark \"<dacpacPath>\" \"<connectionString>\" [-namekey]");

return 1;
