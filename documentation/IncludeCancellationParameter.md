## IncludeCancellationParameter

<table>
<tr>
  <td>TypeName</td>
  <td>IncludeCancellationParameterAnalyzer</td>
</tr>
<tr>
  <td>CheckId</td>
  <td>IncludeCancellationParameter</td>
</tr>
<tr>
  <td>Category</td>
  <td>Usage Rules</td>
</tr>
</table>

## Cause

An asynchronous method does not include a `CancellationToken` parameter.

## Rule description

This diagnostic identifies asynchronous methods which do not include a `CancellationToken` parameter in their signature.

## How to fix violations

To fix a violation of this rule, add a `CancellationToken` parameter to the signature of the method.

## How to suppress violations

```csharp
[SuppressMessage("AsyncUsage.CSharp.Usage", "IncludeCancellationParameter", Justification = "Reviewed.")]
```

```csharp
#pragma warning disable IncludeCancellationParameter // Include CancellationToken parameter
public Task<int> ThisMethodCannotBeCancelled()
#pragma warning restore IncludeCancellationParameter // Include CancellationToken parameter
{
    return Task.FromResult(0);
}
```
