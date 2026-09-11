using System.IO.Compression;

namespace Test;

public class DeploymentOptionsTest
{
    [Fact]
    public async Task ChecksumIgnoresTargetDatabase()
    {
        var dacpacPath = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.dacpac");
        var firstProfilePath = Path.GetTempFileName();
        var secondProfilePath = Path.GetTempFileName();

        try
        {
            using (var archive = ZipFile.Open(dacpacPath, ZipArchiveMode.Create))
            {
                var model = archive.CreateEntry("model.xml");
                await using var writer = new StreamWriter(model.Open());
                await writer.WriteAsync("<Model />");
            }

            await File.WriteAllTextAsync(firstProfilePath, """
                <Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
                  <PropertyGroup>
                    <DropObjectsNotInSource>False</DropObjectsNotInSource>
                    <TargetDatabaseName>FirstDatabase</TargetDatabaseName>
                  </PropertyGroup>
                </Project>
                """);
            await File.WriteAllTextAsync(secondProfilePath, """
                <Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
                  <PropertyGroup>
                    <TargetDatabaseName>SecondDatabase</TargetDatabaseName>
                    <DropObjectsNotInSource>False</DropObjectsNotInSource>
                  </PropertyGroup>
                </Project>
                """);

            var service = new DacDeploySkip.DacpacChecksumService();

            var firstChecksum = await service.GetChecksumAsync(dacpacPath, firstProfilePath);
            var secondChecksum = await service.GetChecksumAsync(dacpacPath, secondProfilePath);

            Assert.Equal(firstChecksum, secondChecksum);
        }
        finally
        {
            File.Delete(dacpacPath);
            File.Delete(firstProfilePath);
            File.Delete(secondProfilePath);
        }
    }

    [Fact]
    public async Task ChecksumChangesWhenDeploymentOptionChanges()
    {
        var dacpacPath = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.dacpac");
        var firstProfilePath = Path.GetTempFileName();
        var secondProfilePath = Path.GetTempFileName();

        try
        {
            using (var archive = ZipFile.Open(dacpacPath, ZipArchiveMode.Create))
            {
                var model = archive.CreateEntry("model.xml");
                await using var writer = new StreamWriter(model.Open());
                await writer.WriteAsync("<Model />");
            }

            await File.WriteAllTextAsync(firstProfilePath,
                "<Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><PropertyGroup><BlockOnPossibleDataLoss>True</BlockOnPossibleDataLoss></PropertyGroup></Project>");
            await File.WriteAllTextAsync(secondProfilePath,
                "<Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><PropertyGroup><BlockOnPossibleDataLoss>False</BlockOnPossibleDataLoss></PropertyGroup></Project>");

            var service = new DacDeploySkip.DacpacChecksumService();

            var firstChecksum = await service.GetChecksumAsync(dacpacPath, firstProfilePath);
            var secondChecksum = await service.GetChecksumAsync(dacpacPath, secondProfilePath);

            Assert.NotEqual(firstChecksum, secondChecksum);
        }
        finally
        {
            File.Delete(dacpacPath);
            File.Delete(firstProfilePath);
            File.Delete(secondProfilePath);
        }
    }
}
