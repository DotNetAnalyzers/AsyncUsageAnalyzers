## UseAsyncSuffix

<table>
<tr>
  <td>TypeName</td>
  <td>UseAsyncSuffix</td>
</tr>
<tr>
  <td>CheckId</td>
  <td>UseAsyncSuffix</td>
</tr>
<tr>
  <td>Category</td>
  <td>Naming Rules</td>
</tr>
</table>

## Cause

A Task-returning method is named without the suffix `Async`.

## Rule description

This diagnostic identifies methods which are asynchronous according to the Task-based Asynchronous Pattern (TAP) by
their signature, and reports a warning if the method name does not include the suffix `Async`.

## How to fix violations

To fix a violation of this rule, rename the method to include the `Async` suffix.

## How to suppress violations

```csharp
#pragma warning disable UseAsyncSuffix // Use Async suffix
public async Task<int> GetValue()
#pragma warning restore UseAsyncSuffix // Use Async suffix
{
    return 0;
}
```
