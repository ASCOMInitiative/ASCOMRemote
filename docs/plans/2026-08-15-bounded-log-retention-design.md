# Bounded Remote Server Log Retention

## Problem

ASCOM Remote writes detailed server and access logs for every Alpaca request.
The loggers roll at a configured time, but they do not limit individual file
size or remove old files. A continuously polled server can therefore create
multi-gigabyte daily files and consume all available disk space.

## Goals

- Preserve complete server and access diagnostics.
- Limit the size of each automatically named Remote Server log file.
- Limit the number of retained files independently for each Remote Server log
  type.
- Delete only files generated for the same logger type.
- Keep logging available if retention cleanup cannot delete an old file.
- Preserve existing behavior for callers that do not opt into limits.

## Non-goals

- Rate-limit Alpaca clients or change request handling.
- Sample or suppress individual log messages.
- Delete arbitrary ASCOM logs, configuration, or user files.
- Add new setup-dialog controls in this change.

## Design

`TraceLoggerPlus` receives two optional properties:

- `MaximumLogFileSizeBytes`: a positive value rolls an automatically named
  file before the next message when the current file has reached the limit.
- `MaximumRetainedLogFiles`: a positive value retains at most that many files
  for the same logger type beneath the configured log root.

Both properties default to zero, which disables the new behavior for existing
generic callers. Remote Server configures both its server trace and access
loggers with product defaults of 50 MiB per file and 10 retained files per log
type. This bounds normal Remote Server logging to approximately 1 GiB in total,
apart from a single message that may cross a size boundary.

Retention matches the exact automatic name prefix
`ASCOM.{logger-type}.HHmm.ssfff{suffix}.txt`, rejects lookalike names, and skips
reparse-point directories and files. It excludes the active file, orders
remaining candidates from newest to oldest, and removes only overflow files
across all generated daily directories. Files for other logger types and
unrelated files remain untouched.
Deletion is best effort: an unavailable or protected old file must not stop
request processing or current logging.

A replacement stream is opened before the current stream is closed. If file
creation fails temporarily, the current stream remains valid and a later log
message can retry rollover.

## Test Strategy

- Verify that repeated writes create more than one automatically named file
  after the configured size is reached.
- Verify that retention keeps the configured number of files for the active
  logger type.
- Verify that another logger type and an unrelated text file are preserved.
- Verify that generic callers remain unlimited unless they opt into limits.
- Verify retention across more than one generated daily directory.
- Verify that a temporary replacement-file creation failure can recover on the
  next log message.
- Build the complete solution after the focused tests pass.

## Compatibility and Risk

The default values on `TraceLoggerPlus` preserve its historical unbounded
behavior. Only Remote Server opts into the product safeguards. Rotation is
limited to automatically named files because reopening a caller-supplied fixed
name would overwrite it. Cleanup is deliberately scoped by logger type and
never removes the active file.
