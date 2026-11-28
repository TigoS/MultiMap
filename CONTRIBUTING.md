# Contributing to MultiMap

Thank you for contributing to MultiMap. Contributions should improve correctness, performance, compatibility, testing, documentation, or developer experience without introducing unintended API changes.

## Ways to contribute

- Report bugs and propose enhancements through GitHub Issues.
- Improve implementation or public API documentation.
- Add focused unit, property-based, benchmark, or regression tests.
- Update examples, guides, release notes, and Review documents when behavior changes.
- Report security or accessibility concerns using the private channels described below.

## Development setup

MultiMap is a .NET library targeting .NET 11, 10, 9, and 8.

Prerequisites:

- .NET SDK 11.0 or a later SDK that can target all supported frameworks
- Git
- Visual Studio 2026 or another compatible editor (optional)

Restore and build the repository from the solution directory:

```shell
dotnet restore
dotnet build --configuration Release
```

Build and test the relevant projects as needed:

```shell
dotnet build MultiMap/MultiMap.csproj --configuration Release
dotnet test MultiMap.Tests/MultiMap.Tests.csproj --configuration Release
```

The CI workflow also restores, builds, tests, and packs the library, demo, tests, and benchmark projects across the supported test targets.

## Making changes

1. Search existing issues and pull requests before opening a new one.
2. Create a focused branch for the change.
3. Follow the existing C# style, naming, namespace, XML documentation, and null-handling conventions.
4. Preserve backward compatibility unless the change intentionally requires an API or behavioral update.
5. Add or update tests for bug fixes and new behavior.
6. Update directly affected documentation and release notes when public behavior or package metadata changes.
7. Run relevant builds and tests before opening a pull request.

For public API changes, include:

- XML documentation for public members
- Interface and implementation updates
- README or dedicated documentation updates
- Unit tests covering success, failure, and edge-case behavior
- Release-note entry when appropriate

For performance changes, include measured BenchmarkDotNet evidence or a regression benchmark when practical. Avoid treating one-time timing data as a guarantee.

## Testing

Run the full unit-test project:

```shell
dotnet test MultiMap.Tests/MultiMap.Tests.csproj --configuration Release
```

The test project runs unit tests on all configured target frameworks. Run the smallest relevant test selection during development, then run the full project before submitting a pull request.

For a Coverlet run:

```shell
dotnet test MultiMap.Tests/MultiMap.Tests.csproj --collect:"XPlat Code Coverage"
```

If a coverage run changes the repository's documented metrics, update `Docs/Testing.md`, relevant badges, `README.md`, and `Docs/ReleaseNotes.md` consistently.

Property tests use FsCheck/NUnit. Benchmarks use BenchmarkDotNet and may be found under `BenchmarkSuite/`.

## Pull requests

Before submitting:

- Ensure builds and tests pass.
- Keep each pull request focused on one logical change.
- Include a clear description of the problem and solution.
- List validation commands and test results.
- Link related issues, such as `Fixes #123`.
- Include screenshots or recordings only when they help explain a UI, documentation, or demo change.
- Preserve unrelated work and do not revert existing changes you did not make.

Maintainers may request smaller changes, additional tests, documentation updates, or benchmark evidence before merging.

## Documentation

Documentation is maintained in:

- `README.md` for the package overview and quick-start information
- `Docs/` for detailed usage, testing, benchmarks, migration, implementation, and release information
- `Review/` for repository review and implementation planning documents

Keep examples accurate for the current package version and supported target frameworks.

## Reporting issues

Use GitHub Issues for reproducible bugs and enhancement proposals. Include:

- Expected behavior
- Actual behavior
- Minimal reproduction steps or code
- Target framework and operating system
- Package or repository version
- Relevant exception messages or stack traces

Do not use public issues for vulnerabilities or sensitive reports.

For security reports, follow [`SECURITY.md`](SECURITY.md).

For accessibility barriers, follow [`ACCESSIBILITY.md`](ACCESSIBILITY.md).

## License

By contributing, you agree that your contributions are licensed under the repository's MIT License: [`LICENSE.md`](LICENSE.md).
