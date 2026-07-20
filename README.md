# Integrity C# Client (`integrity-sharp`)

A C# client library for the [Integrity](https://github.com/bleedingdeacons/integrity)
WordPress REST API — secure, authenticated access to Unity intergroup Groups,
Meetings and member data.

This repository was extracted from `integrity/client/sharp/` so the client can
be versioned, built and packaged independently of the WordPress plugin.

## Projects

| Project | Target | Purpose |
| --- | --- | --- |
| `TheBleedingDeacons.Unity.Client` | net9.0 | The client (`UnityRestSharp`). Packaged as **`Integrity.Client`**. |
| `TheBleedingDeacons.Unity.Models` | net9.0 | Request/response models (Group, Meeting, Member, GDPR, …). |
| `TheBleedingDeacons.Unity.Tests`  | net9.0 | MSTest unit tests. |
| `example/Integrity-cli`           | net10.0 | Runnable example console app. |

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
