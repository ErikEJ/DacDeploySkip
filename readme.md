# Dac Deploy Skip

Tool to determine if a deployment of a specific .dacpac file is required based on metadata present in the target database.

This can reduce your .dacpac deployment times significantly in scenarios you deploy the same .dacpac multiple times, e.g. in CI/CD pipelines.

## Getting started

The tool runs on any system with .NET 10 or later installed.

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

You can use the optional `-namekey` parameter to use the name of the .dacpac file instead of the full path as key.

```bash
dacdeployskip mark "<path to .dacpac>" "SQL Server connection string" -namekey
```

### Simpler usage in GitHub Actions

If you use GitHub Actions, you can call the repository action directly instead of installing the tool yourself. Here is a complete minimal workflow:

```yaml
name: Deploy dacpac

on:
  workflow_dispatch:

jobs:
  deploy:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v7

      - name: Check if dacpac deployment is needed
        id: dacdeployskip
        uses: ErikEJ/DacDeploySkip@v1
        with:
          command: check
          dacpac-path: ./Database/bin/Release/net8.0/Database.dacpac
          connection-string: ${{ secrets.SQL_CONNECTION_STRING }}

      - name: Install SqlPackage
        if: steps.dacdeployskip.outputs.deployed != 'true'
        run: dotnet tool install -g Microsoft.SqlPackage

      - name: Deploy dacpac
        if: steps.dacdeployskip.outputs.deployed != 'true'
        run: sqlpackage /Action:Publish /SourceFile:"./Database/bin/Release/net8.0/Database.dacpac" /TargetConnectionString:"${{ secrets.SQL_CONNECTION_STRING }}"

      - name: Mark dacpac as deployed
        if: steps.dacdeployskip.outputs.deployed != 'true'
        uses: ErikEJ/DacDeploySkip@v1
        with:
          command: mark
          dacpac-path: ./Database/bin/Release/net8.0/Database.dacpac
          connection-string: ${{ secrets.SQL_CONNECTION_STRING }}
```

You can optionally pass `tool-version` to pin the NuGet package version used by the action.

### Sample usage in Azure DevOps pipeline

```yaml
trigger:
- main

pool:
  name: selfhosted

variables:
  buildConfiguration: 'Release'
  connectionString: 'Data Source=(localdb)\mssqllocaldb;Initial Catalog=TestBed;Integrated Security=true;Encrypt=false'
  dacpacPath: '$(Build.SourcesDirectory)\Database\bin\Release\net8.0\Database.dacpac'

steps:

  - powershell: |
      dotnet tool install -g Microsoft.SqlPackage
      # Use the .NET 10 target when the agent already has the .NET 10 SDK installed.
      dotnet tool install -g ErikEJ.DacFX.DacDeploySkip --framework net10.0
      dotnet build $(buildConfiguration)
      dacdeployskip check "$(dacpacPath)" "$(connectionString)"
      if (!$?)
      {
         sqlpackage /Action:Publish /SourceFile:"$(dacpacPath)" /TargetConnectionString:"$(connectionString)"
         dacdeployskip mark "$(dacpacPath)" "$(connectionString)"
      }
    displayName: deploy dacpac if needed only

```

You can find a [complete example here](/sample).

You can also use the tool to set a condition in your pipeline based on whether a deployment is needed or not. This can be useful if you use a task like `SqlAzureDacpacDeployment` or `SqlDacpacDeploymentOnMachineGroup`.

```yaml
- powershell: |

    dacdeployskip check "$(dacpacPath)" "$(ConnectionString)"
    if (!$?)
    {
      Write-Host "##vso[task.setvariable variable=DeployDacPac;]$true"  
    }
    else
    {
      Write-Host "##vso[task.setvariable variable=DeployDacPac;]$false"  
    }
  displayName: check if dacpac deployment is needed
```

Then use the condition on subsequent tasks:

```yaml
 condition: and(succeeded(), eq(variables['DeployDacPac'], true))
```

### Using multiple deployments against the same database

If you deploy multiple different .dacpac files against the same database, use the additional parameter `/p:DropExtendedPropertiesNotInSource=False` to avoid dropping the metadata added by this tool.

If you use a publish profile, you can add the same parameter there.

```xml
<DropExtendedPropertiesNotInSource>False</DropExtendedPropertiesNotInSource>
```
