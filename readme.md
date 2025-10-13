# Dac Deploy Skip

Tool to determine if a deployment of a specific .dacpac file is required based on metadata present in the target database.

## Getting started

The tool runs on any system with the .NET 8 or .NET 10 runtime installed. 

### Installing the tool

```bash
dotnet tool install -g ErikEJ.DacFX.DacDeploySkip
```

### Basic usage

```bash
dacdeployskip check "<path to .dacpac>" "SQL Server connection string" 
```

This command will return 0 if the .dacpac has already been deployed, otherwise 1.

```bash
dacdeployskip mark "<path to .dacpac>" "SQL Server connection string" 
```

This command will add metadata to the target database to register the .dacpac as deployed.

### Usage in Azure DevOps pipeline

```yaml

steps:

- script: dotnet tool install -g Microsoft.SqlPackage
  displayName: Install sqlpackage CLI

- script: dotnet tool install -g ErikEJ.DacFX.DacDeploySkip
  displayName: Install DacDeploySkip CLI

- script: |
    dacdeployskip check $dacpacPath $connectionString
  continueOnError: true
  displayName: check if .dacpac has been deployed

- script: |
    sqlpackage /Action:Publisk /SourceFile:"$(dacpacPath)" /TargetConnectionString:"$(connectionString)"
    echo "##vso[task.setvariable variable=markAsDeployed]Yes"
  condtions: failed()
  displayName: Publish .dacpac if metadata check failed

- script: |
    dacdeployskip mark "$(dacpacPath)" "$(connectionString)"
  displayName: Set metadata
  condition: and(succeeded(), eq(variables.markAsDeployed, 'Yes')

```
