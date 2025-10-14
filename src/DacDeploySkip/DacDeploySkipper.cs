﻿using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;

namespace DacDeploySkip;

internal class DacpacChecksumService
{
    public async Task<bool> CheckIfDeployedAsync(string dacpacPath, string targetConnectionString, CancellationToken cancellationToken = default)
    {
        var targetDatabaseName = GetDatabaseName(targetConnectionString);
        
        using (var connection = new SqlConnection(targetConnectionString))
        {
            try
            {
                // Try to connect to the target database to see it exists and fail fast if it does not.
                await connection.OpenAsync(SqlConnectionOverrides.OpenWithoutRetry, cancellationToken);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is SqlException)
            {
                Console.WriteLine($"Target database {targetDatabaseName} is not available.");
                return false;
            }

            var dacpacId = GetStringChecksum(dacpacPath);

            var dacpacChecksum = await GetChecksumAsync(dacpacPath);

            var deployed = await CheckExtendedPropertyAsync(connection, dacpacId, dacpacChecksum, cancellationToken);

            if (deployed)
            {
                Console.WriteLine($"The .dacpac with id {dacpacId} and checksum {dacpacChecksum} has already been deployed to database {targetDatabaseName}.");
                return true;
            }

            Console.WriteLine($"The .dacpac with id {dacpacId} and checksum {dacpacChecksum} has not been deployed to database {targetDatabaseName}.");
            return false;
        }
    }

    public async Task SetChecksumAsync(string dacpacPath, string targetConnectionString,  CancellationToken cancellationToken = default)
    {
        var targetDatabaseName = GetDatabaseName(targetConnectionString);

        var dacpacId = GetStringChecksum(dacpacPath);

        var dacpacChecksum = await GetChecksumAsync(dacpacPath);
        
        using (var connection = new SqlConnection(targetConnectionString))
        {
            await connection.OpenAsync(SqlConnectionOverrides.OpenWithoutRetry, cancellationToken);

            await UpdateExtendedPropertyAsync(connection, dacpacId, dacpacChecksum, cancellationToken);

            Console.WriteLine($"The .dacpac with id {dacpacId} and checksum {dacpacChecksum} has been registered in database {targetDatabaseName}.");
        }
    }

    private static string GetDatabaseName(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        return builder.InitialCatalog;
    }

    private static async Task<string> GetChecksumAsync(string file)
    {
        var output = Path.Join(Path.GetTempPath(), Path.GetRandomFileName());

        System.IO.Compression.ZipFile.ExtractToDirectory(file, output);

        using var stream = File.OpenRead(Path.Join(output, "model.xml"));
        using var sha = SHA256.Create();
        var checksum = await sha.ComputeHashAsync(stream);

        // Clean up the extracted files
        try
        {
            Directory.Delete(output, true);
        }
        catch
        {
            // Ignore any errors during cleanup
        }

        return BitConverter.ToString(checksum).Replace("-", string.Empty);
    }

    private static string GetStringChecksum(string text)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);
        using var sha = SHA256.Create();
        var checksum = sha.ComputeHash(bytes);
        return BitConverter.ToString(checksum).Replace("-", string.Empty);
    }

    private static async Task<bool> CheckExtendedPropertyAsync(SqlConnection connection, string dacpacId, string dacpacChecksum, CancellationToken cancellationToken)
    {
        var command = new SqlCommand(
            @$"SELECT CAST(1 AS BIT) FROM fn_listextendedproperty(NULL, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT)
            WHERE [value] = @Expected
            AND [name] = @dacpacId;",
            connection);

        command.Parameters.AddRange(GetParameters(dacpacChecksum, dacpacId));

        var result = await command.ExecuteScalarAsync(cancellationToken);

        return result == null ? false : (bool)result;
    }

    private static async Task UpdateExtendedPropertyAsync(SqlConnection connection, string dacpacId, string dacpacChecksum, CancellationToken cancellationToken)
    {
        var command = new SqlCommand($@"
            IF EXISTS
            (
                SELECT 1 FROM fn_listextendedproperty(null, default, default, default, default, default, default)
                WHERE [name] = @dacpacId
            )
            BEGIN
                EXEC sp_updateextendedproperty @name = @dacpacId, @value = @Expected;
            END 
            ELSE 
            BEGIN
                EXEC sp_addextendedproperty @name = @dacpacId, @value = @Expected;
            END;",
            connection);

        command.Parameters.AddRange(GetParameters(dacpacChecksum, dacpacId));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static SqlParameter[] GetParameters(string dacpacChecksum, string dacpacId)
    {
        return
        [
            new SqlParameter("@Expected", SqlDbType.VarChar)
            {
                Value = dacpacChecksum
            },
            new SqlParameter("@dacpacId", SqlDbType.NVarChar, 128)
            {
                Value = dacpacId
            },
        ];
    }
}