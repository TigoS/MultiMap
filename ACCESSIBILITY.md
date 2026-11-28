# Accessibility

Accessibility matters because MultiMap should be understandable and usable by as many developers and documentation readers as possible. This document describes the project's accessibility commitments, contributor expectations, and reporting process.

It applies primarily to repository documentation, examples, demonstrations, and the GitHub project experience. It does not claim conformance for third-party platforms or generated NuGet package hosting interfaces.

## Priorities

The project prioritizes:

- Clear, descriptive headings and a usable navigation structure.
- Plain-language documentation that explains tasks without requiring prior repository-specific terminology.
- Descriptive link text and meaningful code examples.
- Readable tables, lists, and code blocks with equivalent text for important information.
- Keyboard and screen-reader usability in project documentation and demo applications.
- Accessibility regressions caught through focused review before release documentation is published.

No accessibility conformance level is claimed for the repository or package as a whole. The priorities above are aspirational engineering goals rather than a completed standards evaluation.

## Contributor expectations

Contributors should:

- Prefer plain language and descriptive headings.
- Use meaningful link text instead of "click here" or bare URLs.
- Provide text alternatives for diagrams or describe essential diagram content nearby.
- Preserve heading hierarchy and avoid using tables only to control visual layout.
- Check code samples and documentation examples for readability when rendered.
- Keep keyboard workflows usable in demo applications and documentation tooling.
- For documentation or demo changes, include the rendering or browser environment tested and attach an optional screenshot or recording only when useful.

Automated accessibility tools are not currently used by a dedicated CI accessibility job. Contributors should identify the manual checks performed and any limitations in the pull-request description.

## Reporting accessibility issues

Report barriers through a GitHub Issue:

- <https://github.com/TigoS/MultiMap/issues/new>

Useful context includes:

- The task you were trying to complete
- The affected URL or repository file
- The observed behavior
- The expected behavior
- Operating system and version
- Browser or screen reader, where applicable
- Assistive technology or input method
- A workaround you found

Screenshots or recordings are optional. Disability disclosure is never required.

Please use the issue title prefix `[Accessibility]` so maintainers can filter and triage the report.

### Severity

- **Critical:** Prevents a person from completing a core task, with no practical workaround.
- **High:** Creates a major barrier for an important task or affects a broad group of users.
- **Medium:** Makes a task difficult or inefficient but still permits a workaround.
- **Low:** A usability or consistency issue that does not prevent task completion.

Reporters do not need to assign a severity; maintainers may confirm or adjust it during triage.

### How we respond

The maintainer aims to acknowledge accessibility reports within 5 business days. Reports are triaged as time permits, assigned a severity, and linked to a fix or documentation change when appropriate.

Status updates are provided in the GitHub Issue as investigation or implementation progresses. For a barrier with no immediate fix, the maintainer will document a workaround when one exists and explain any known limitation.

Fixes are verified by the maintainer or a contributor who can reproduce the affected experience. Reporters may be asked to verify a fix, but may decline without affecting triage.

## Ownership and maintenance

Accessibility is currently maintained by the repository maintainer, TigoS. Responsibilities include triaging reports, reviewing user-facing changes, documenting known barriers, and maintaining this statement.

Review this statement when project structure, documentation platforms, demo applications, or support expectations materially change.

## Supported environments

The library targets:

- .NET 11
- .NET 10
- .NET 9
- .NET 8

Repository documentation is tested or reviewed through GitHub's Markdown rendering and standard text editors. No formal browser, operating-system, screen-reader, or input-method support matrix has been evaluated.

Documentation and demo contribution should target common modern browsers and standard keyboard and screen-reader workflows. These environments are best-effort rather than a conformance guarantee.

## Known limitations

No formal accessibility evaluation has been completed for the repository, documentation, or demo applications. Known results and barriers should be documented here or linked to tracked GitHub Issues after evaluation or user report.

The project may contain:

- Long Markdown tables that can be difficult to navigate with some assistive technologies
- Large code samples that require careful keyboard or screen-reader navigation
- Visual benchmark and coverage layouts whose underlying values may not have a dedicated text summary in every historical document

Text equivalents are preferred for coverage, test, and benchmark information. Where a table is not reasonably navigable, maintainers should add a short prose summary or list.

## Feedback and improvements

Suggestions for improving this statement or the project's accessibility practices can be submitted as GitHub Issues or pull requests.

Active barriers should use the reporting process above. Sensitive or security-related concerns must use [`SECURITY.md`](SECURITY.md) instead of an accessibility issue.
