using System.Xml.Linq;

namespace DacDeploySkip;

internal static class DeploymentOptionsSerializer
{
    private static readonly HashSet<string> ExcludedProperties = new(StringComparer.Ordinal)
    {
        "TargetConnectionString",
        "TargetDatabaseName"
    };

    internal static string Serialize(string publishProfilePath)
    {
        var document = XDocument.Load(publishProfilePath);
        var options = new SortedDictionary<string, string>(StringComparer.Ordinal);

        foreach (var property in document
            .Descendants()
            .Where(element => element.Parent?.Name.LocalName == "PropertyGroup"
                && !ExcludedProperties.Contains(element.Name.LocalName)))
        {
            options[property.Name.LocalName] = property.Value;
        }

        foreach (var variable in document
            .Descendants()
            .Where(element => element.Name.LocalName == "SqlCmdVariable"))
        {
            var name = variable.Attribute("Include")?.Value;
            var value = variable.Elements().FirstOrDefault(element => element.Name.LocalName == "Value")?.Value;
            if (!string.IsNullOrEmpty(name) && value != null)
            {
                options[$"SqlCmdVariable:{name}"] = value;
            }
        }

        return string.Join(
            "\n",
            options.Select(option => $"{option.Key}={option.Value}"));
    }
}
