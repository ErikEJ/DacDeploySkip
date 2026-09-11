using System.Xml.Linq;

namespace DacDeploySkip;

internal static class DeploymentOptionsSerializer
{
    private static readonly HashSet<string> IncludedProperties = new(
    [
        "AdditionalDeploymentContributorArguments",
        "AdditionalDeploymentContributorPaths",
        "AdditionalDeploymentContributors",
        "AllowDropBlockingAssemblies",
        "AllowExternalLanguagePaths",
        "AllowExternalLibraryPaths",
        "AllowIncompatiblePlatform",
        "AllowTableRecreation",
        "AllowUnsafeRowLevelSecurityDataMovement",
        "AzureSharedAccessSignatureToken",
        "AzureStorageBlobEndpoint",
        "AzureStorageContainer",
        "AzureStorageKey",
        "AzureStorageRootPath",
        "BackupDatabaseBeforeChanges",
        "BlockOnPossibleDataLoss",
        "BlockWhenDriftDetected",
        "CommandTimeout",
        "CommentOutSetVarDeclarations",
        "CompareUsingTargetCollation",
        "CreateNewDatabase",
        "DatabaseLockTimeout",
        "DatabaseSpecification",
        "DataOperationStateProvider",
        "DeployDatabaseInSingleUserMode",
        "DisableAndReenableDdlTriggers",
        "DisableIndexesForDataPhase",
        "DisableParallelismForEnablingIndexes",
        "DoNotAlterChangeDataCaptureObjects",
        "DoNotAlterReplicatedObjects",
        "DoNotDropDatabaseWorkloadGroups",
        "DoNotDropObjectTypes",
        "DoNotDropWorkloadClassifiers",
        "DoNotEvaluateSqlCmdVariables",
        "DropConstraintsNotInSource",
        "DropDmlTriggersNotInSource",
        "DropExtendedPropertiesNotInSource",
        "DropIndexesNotInSource",
        "DropObjectsNotInSource",
        "DropPermissionsNotInSource",
        "DropRoleMembersNotInSource",
        "DropStatisticsNotInSource",
        "EnclaveAttestationProtocol",
        "EnclaveAttestationUrl",
        "ExcludeObjectTypes",
        "GenerateSmartDefaults",
        "HashObjectNamesInLogs",
        "IgnoreAnsiNulls",
        "IgnoreAuthorizer",
        "IgnoreColumnCollation",
        "IgnoreColumnOrder",
        "IgnoreComments",
        "IgnoreCryptographicProviderFilePath",
        "IgnoreDatabaseWorkloadGroups",
        "IgnoreDdlTriggerOrder",
        "IgnoreDdlTriggerState",
        "IgnoreDefaultSchema",
        "IgnoreDmlTriggerOrder",
        "IgnoreDmlTriggerState",
        "IgnoreExtendedProperties",
        "IgnoreFileAndLogFilePath",
        "IgnoreFilegroupPlacement",
        "IgnoreFileSize",
        "IgnoreFillFactor",
        "IgnoreFullTextCatalogFilePath",
        "IgnoreIdentitySeed",
        "IgnoreIncrement",
        "IgnoreIndexOptions",
        "IgnoreIndexPadding",
        "IgnoreKeywordCasing",
        "IgnoreLockHintsOnIndexes",
        "IgnoreLoginSids",
        "IgnoreNotForReplication",
        "IgnoreObjectPlacementOnPartitionScheme",
        "IgnorePartitionSchemes",
        "IgnorePermissions",
        "IgnorePostDeployScript",
        "IgnorePreDeployScript",
        "IgnoreQuotedIdentifiers",
        "IgnoreRoleMembership",
        "IgnoreRouteLifetime",
        "IgnoreSemicolonBetweenStatements",
        "IgnoreSensitivityClassifications",
        "IgnoreTableOptions",
        "IgnoreTablePartitionOptions",
        "IgnoreUserSettingsObjects",
        "IgnoreWhitespace",
        "IgnoreWithNocheckOnCheckConstraints",
        "IgnoreWithNocheckOnForeignKeys",
        "IgnoreWorkloadClassifiers",
        "IncludeCompositeObjects",
        "IncludeTransactionalScripts",
        "IsAlwaysEncryptedParameterizationEnabled",
        "LongRunningCommandTimeout",
        "NoAlterStatementsToChangeClrTypes",
        "PerformIndexOperationsOnline",
        "PopulateFilesOnFileGroups",
        "PreserveIdentityLastValues",
        "RebuildIndexesOfflineForDataPhase",
        "RegisterDataTierApplication",
        "RestoreSequenceCurrentValue",
        "RunDeploymentPlanExecutors",
        "ScriptDatabaseCollation",
        "ScriptDatabaseCompatibility",
        "ScriptDatabaseOptions",
        "ScriptDeployStateChecks",
        "ScriptFileSize",
        "ScriptNewConstraintValidation",
        "ScriptRefreshModule",
        "TreatVerificationErrorsAsWarnings",
        "UnmodifiableObjectWarnings",
        "VerifyCollationCompatibility",
        "VerifyDeployment"
    ], StringComparer.Ordinal);

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
                && IncludedProperties.Contains(element.Name.LocalName)
                && !ExcludedProperties.Contains(element.Name.LocalName)))
        {
            options[property.Name.LocalName] = property.Value;
        }

        foreach (var variable in document
            .Descendants()
            .Where(element => element.Name.LocalName == "SqlCmdVariable"))
        {
            var name = variable.Attribute("Include")?.Value;
            var value = variable.Elements()
                .FirstOrDefault(element => element.Name.LocalName is "Value" or "DefaultValue")
                ?.Value;
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
