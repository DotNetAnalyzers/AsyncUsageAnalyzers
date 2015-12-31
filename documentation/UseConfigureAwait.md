## UseConfigureAwait

<table>
<tr>
  <td>TypeName</td>
  <td>UseConfigureAwait</td>
</tr>
<tr>
  <td>CheckId</td>
  <td>UseConfigureAwait</td>
</tr>
<tr>
  <td>Category</td>
  <td>Usage Rules</td>
</tr>
</table>

## Cause

A `Task` whose continuation behavior has not been configured is awaited.

## Rule description

The continuation behavior for a `Task` should be configured by calling `ConfigureAwait` prior to awaiting the task. This
diagnostic is reported if an `await` expression is used on a `Task` that has not been configured.

## How to fix violations

To fix a violation of this rule, use `ConfigureAwait(false)` or `ConfigureAwait(true)` as appropriate for the context of
the asynchronous operation.

## How to suppress violations

```csharp
#pragma warning disable UseConfigureAwait // Use ConfigureAwait
await Task.Delay(1000);
#pragma warning restore UseConfigureAwait // Use ConfigureAwait
```
