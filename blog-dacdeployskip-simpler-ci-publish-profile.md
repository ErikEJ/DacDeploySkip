---
layout: post
title: "DacDeploySkip update: simpler CI and publish profile support"
date: 2026-09-12 10:45:00 +0000
tags:
  - sql server
  - dacpac
  - devops
  - github actions
  - dotnet
excerpt: "DacDeploySkip now has simpler CI usage and publish profile-aware checksums, helping you skip unnecessary dacpac deployments more reliably."
---

If you deploy `.dacpac` files frequently, you know how much time is wasted when nothing has actually changed.

That is exactly what **DacDeploySkip** solves: it checks whether the same `.dacpac` (with the same effective deployment settings) has already been deployed to a target database, and lets you skip unnecessary publishes.

## What is new

### Simpler CI usage

Using DacDeploySkip in CI is now simpler, especially in GitHub Actions.

You can use the action directly to:

1. Run `check` before publishing.
2. Publish with `SqlPackage` only if needed.
3. Run `mark` after a successful publish.

This keeps your workflow small and avoids custom scripting.

### Publish profile support

A key improvement is support for including publish profile settings in checksum calculation.

With `-profile` (or `publish-profile` in the action), DacDeploySkip includes deployment options and SQLCMD variable values from the `.publish.xml` file in the checksum. This makes skip decisions more accurate when deployment behavior is controlled by a publish profile.

`TargetConnectionString` and `TargetDatabaseName` are intentionally excluded.

## Why this matters

Two deployments of the same `.dacpac` are not always equivalent when deployment options differ.

By including publish profile options in the checksum, DacDeploySkip can now detect these cases and avoid false "already deployed" decisions.

## Note about SqlPackage

Sadly, **FastComparison** has been (temporarily?) removed from `SqlPackage`.

Until that returns, reducing unnecessary deployments becomes even more important for keeping CI/CD pipelines fast.

## Example

```bash
dacdeployskip check "<path to .dacpac>" "SQL Server connection string" -profile "<path to .publish.xml>"
# if exit code is 1 -> run sqlpackage publish
dacdeployskip mark "<path to .dacpac>" "SQL Server connection string" -profile "<path to .publish.xml>"
```

## Get started

```bash
dotnet tool install -g ErikEJ.DacFX.DacDeploySkip
```

Project: [github.com/ErikEJ/DacDeploySkip](https://github.com/ErikEJ/DacDeploySkip)
