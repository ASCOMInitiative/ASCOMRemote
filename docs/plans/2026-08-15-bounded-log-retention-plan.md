# Bounded Log Retention Implementation Plan

## Objective

Prevent ASCOM Remote server and access logs from growing without a disk bound
while preserving complete diagnostics and compatibility for other logger
callers.

## Implementation Steps

- Add a focused .NET test project and tests that describe size rollover,
  bounded retention, and deletion scope.
- Run the focused tests and capture the expected pre-fix failure.
- Add optional size and retention properties to `TraceLoggerPlus`.
- Rotate automatically named files when the active stream reaches its limit.
- Apply best-effort retention after creating an automatic log file.
- Configure both Remote Server logger creation paths with 50 MiB and 10-file
  defaults.
- Run focused tests, the complete test suite, and a solution build.
- Review the diff for deletion scope, compatibility, and unrelated changes.
- Commit the change on `fix/bounded-log-retention`, push to a fork, and open an
  upstream pull request. Cite the maintainer's retention comment in issue 55
  for context without claiming that this change closes the OpenTelemetry issue.

## Verification Criteria

- Focused rollover and retention tests pass.
- Other log types and unrelated files survive cleanup.
- Existing callers remain unlimited unless they set the new properties.
- The solution builds successfully with the required .NET 8 SDK.
- The pull request clearly states the measured disk-growth failure mode and the
  bounded behavior introduced by the fix.
