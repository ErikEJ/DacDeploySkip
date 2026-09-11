using Microsoft.SqlServer.Dac;
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace DacDeploySkip;

internal static class DeploymentOptionsSerializer
{
    private static readonly HashSet<string> ExcludedProperties = new(StringComparer.Ordinal)
    {
        "CreateNewDatabase",
        "DataOperationStateProvider",
        "EnableFastComparison",
        "LogDeployment"
    };

    internal static string Serialize(string publishProfilePath)
    {
        var deploymentOptions = DacProfile.Load(publishProfilePath).DeployOptions;
        var options = new SortedDictionary<string, string>(StringComparer.Ordinal);

        foreach (var property in deploymentOptions
            .GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.CanRead
                && property.PropertyType != typeof(string)
                && !ExcludedProperties.Contains(property.Name)))
        {
            var value = property.GetValue(deploymentOptions);
            if (value is IDictionary dictionary)
            {
                foreach (DictionaryEntry item in dictionary)
                {
                    options[$"{property.Name}:{item.Key}"] = Convert.ToString(item.Value, CultureInfo.InvariantCulture) ?? string.Empty;
                }
            }
            else if (value is IEnumerable values)
            {
                options[property.Name] = string.Join(
                    ",",
                    values.Cast<object>()
                        .Select(value => Convert.ToString(value, CultureInfo.InvariantCulture))
                        .Order(StringComparer.Ordinal));
            }
            else if (value != null && (property.PropertyType.IsValueType || property.PropertyType.IsEnum))
            {
                options[property.Name] = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            }
        }

        return string.Join(
            "\n",
            options.Select(option => $"{option.Key}={option.Value}"));
    }
}
