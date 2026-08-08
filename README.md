# Integrity C# Client (`integrity-sharp`)

[![CI](https://github.com/bleedingdeacons/integrity-sharp/actions/workflows/ci.yml/badge.svg)](https://github.com/bleedingdeacons/integrity-sharp/actions/workflows/ci.yml)
[![Coverage Status](https://coveralls.io/repos/github/bleedingdeacons/integrity-sharp/badge.svg?branch=main)](https://coveralls.io/github/bleedingdeacons/integrity-sharp?branch=main)

A C# client library for the [Integrity](https://github.com/bleedingdeacons/integrity)
WordPress REST API — secure, authenticated access to Unity intergroup Groups,
Meetings and member data.

This repository was extracted from `integrity/client/sharp/` so the client can
be versioned, built and packaged independently of the WordPress plugin.

## Projects

| Project | Target | Purpose |
| --- | --- | --- |
| `TheBleedingDeacons.Unity.Client` | net9.0 | The client (`UnityRestSharp`). Packaged as **`Integrity.Client`**. |
| `TheBleedingDeacons.Unity.Models` | net9.0 | Request/response models (Group, Meeting, Member, GDPR, …). Ships bundled inside `Integrity.Client`. |
| `TheBleedingDeacons.Unity.Tests`  | net9.0 | xUnit v3 unit tests (Microsoft.Testing.Platform). |
| `example/Integrity-cli`           | net10.0 | Runnable example console app. |

## Installing

Releases are published to **GitHub Packages** as `Integrity.Client`. The
`TheBleedingDeacons.Unity.Models` assembly ships bundled inside that package, so
it is the only reference you need.

GitHub Packages requires authentication even for public repositories. Create a
[classic personal access token](https://github.com/settings/tokens) with the
`read:packages` scope, then add a `nuget.config` next to your solution:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="bleedingdeacons" value="https://nuget.pkg.github.com/bleedingdeacons/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <bleedingdeacons>
      <add key="Username" value="YOUR_GITHUB_USERNAME" />
      <add key="ClearTextPassword" value="%GITHUB_PACKAGES_TOKEN%" />
    </bleedingdeacons>
  </packageSourceCredentials>
</configuration>
```

Keep the token in the `GITHUB_PACKAGES_TOKEN` environment variable rather than in
the file, and don't commit a `nuget.config` containing a literal token. Then:

```bash
dotnet add package Integrity.Client
```

Alternatively, every release also attaches the `.nupkg` to its
[GitHub Release](https://github.com/bleedingdeacons/integrity-sharp/releases).
Download it into a folder and add that folder as a package source — no token
required.

## Releasing

`ci.yml` runs on every push and PR to `main`. `release.yml` runs on a version tag:
it repeats the full CI gate (build, `dotnet format`, analyzers, tests) and
publishes only if all of it passes.

```powershell
./scripts/release.ps1 1.10.4
```

That bumps `<Version>` in the client csproj, commits it as `release: v1.10.4`,
tags, and pushes both — the tag push is what starts the workflow. It refuses to
run unless the working tree is clean, `HEAD` is `main` and in sync with `origin`,
and the tag doesn't already exist, since a published version can never be reused.
Add `-WhatIf` to see what it would do without touching anything.

The csproj therefore carries the *last released* version between releases; the
script moves it as part of the release commit. Doing it by hand is equivalent:

```bash
git tag v1.10.4 && git push origin v1.10.4
```

The tag drives the package version (`v1.10.4` → `1.10.4`), overriding `<Version>`
in the csproj, so a tag and a published package can never disagree. Tags with a
prerelease suffix (`v1.11.0-rc.1`) are marked as prereleases. Nothing publishes
from a plain push to `main`.

Publishing requires a repository secret named **`PACKAGES_TOKEN`** — a classic
PAT with the `repo` and `write:packages` scopes. The built-in `GITHUB_TOKEN`
cannot be used: the `bleedingdeacons` organization disables write permissions for
workflow tokens, which overrides the `permissions:` block in the workflow, so
both the package push and the release creation would fail with `403`. The
workflow checks the secret is present before running the build gate.

## Usage

```csharp
using TheBleedingDeacons.Unity.Client;

using var client = new UnityRestSharp("https://your-site.example/", "int_your_api_key");

var health = await client.CheckHealthAsync();
Console.WriteLine($"Unity available: {health?.UnityAvailable}");

var groups = await client.GetGroupsAsync(expandMeetings: true);
if (groups.Success && groups.Data != null)
{
    foreach (var group in groups.Data)
        Console.WriteLine($"{group.Title} — {group.Meetings.Count} meetings");
}
```

The constructor also accepts an optional `HttpClient` and
`ILogger<UnityRestSharp>` for custom transport and logging.

## Build & test

```bash
# Build the library
dotnet build TheBleedingDeacons.Unity.Client.sln -c Release

# Run the tests
dotnet test TheBleedingDeacons.Unity.Tests/TheBleedingDeacons.Unity.Tests.csproj

# Build & run the example CLI
dotnet run --project example/Integrity-cli/Integrity-cli.csproj
```

## License

MIT © The Bleeding Deacons
