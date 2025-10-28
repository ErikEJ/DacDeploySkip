# Dac Deploy Skip

Tool to determine if a deployment of a specific .dacpac file is required based on metadata present in the target database.

This can reduce your .dacpac deployment times significantly in scenarios you deploy the same .dacpac multiple times, e.g. in CI/CD pipelines.

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

You can use the optional `-namekey` parameter to use the name of the .dacpac file instead of the full path as key.

```bash
dacdeployskip mark "<path to .dacpac>" "SQL Server connection string" -namekey
```

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
      dotnet tool install -g ErikEJ.DacFX.DacDeploySkip
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
