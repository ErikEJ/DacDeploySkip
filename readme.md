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

### Sample usage in Azure DevOps pipeline

```yml
trigger:
- main

pool:
  name: selfhosted

variables:
  buildConfiguration: 'Release'
  connectionString: 'Data Source=(localdb)\mssqllocaldb;Initial Catalog=TestBed;Integrated Security=true;Encrypt=false'
  dacpacPath: '$(Build.SourcesDirectory)\Database\bin\Release\net8.0\Database.dacpac'

steps:

  - script: dotnet tool install -g Microsoft.SqlPackage
    displayName: Install sqlpackage CLI

  - script: dotnet tool install -g ErikEJ.DacFX.DacDeploySkip
    displayName: Install DacDeploySkip CLI

  - script: dotnet build --configuration $(buildConfiguration)
    displayName: 'dotnet build $(buildConfiguration)'

  - powershell: |
      dacdeployskip check "$(dacpacPath)" "$(connectionString)"
      if (!$?)
      {
         sqlpackage /Action:Publish /SourceFile:"$(dacpacPath)" /TargetConnectionString:"$(connectionString)" /p:DropExtendedPropertiesNotInSource=False
         dacdeployskip mark "$(dacpacPath)" "$(connectionString)"
      }
    displayName: deploy dacpac if needed only

```
