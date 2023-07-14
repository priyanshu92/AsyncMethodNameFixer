; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 1.2

### New Rules
Rule ID | Category | Severity | Notes
--------|----------|----------|-------
AMNF0001 | Naming  | Warning  | Methods that have an awaitable return type should be postfixed 'Async'
AMNF0002 | Naming  | Warning  | Methods that do not have an awaitable return type should not be postfixed 'Async'