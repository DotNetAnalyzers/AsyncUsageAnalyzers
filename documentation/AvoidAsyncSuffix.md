## AvoidAsyncSuffix

<table>
<tr>
  <td>TypeName</td>
  <td>AvoidAsyncSuffix</td>
</tr>
<tr>
  <td>CheckId</td>
  <td>AvoidAsyncSuffix</td>
</tr>
<tr>
  <td>Category</td>
  <td>Naming Rules</td>
</tr>
</table>

## Cause

A non-Task-returning method is named with the suffix `Async`.

## Rule description

This diagnostic identifies methods which are not asynchronous according to the Task-based Asynchronous Pattern (TAP) by
their signature, and reports a warning if the method name includes the suffix `Async`.

## How to fix violations

To fix a violation of this rule, remove the `Async` suffix from the method name or change the signature to return a
`Task`.

## How to suppress violations

```csharp
[SuppressMessage("AsyncUsage.CSharp.Naming", "AvoidAsyncSuffix", Justification = "Reviewed.")]
```

```csharp
#pragma warning disable AvoidAsyncSuffix // Avoid Async suffix
public int ThisIsNotAsync()
#pragma warning restore AvoidAsyncSuffix // Avoid Async suffix
{
    return 0;
}
```
