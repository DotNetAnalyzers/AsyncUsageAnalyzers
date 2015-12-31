## AvoidAsyncVoid

<table>
<tr>
  <td>TypeName</td>
  <td>AvoidAsyncVoid</td>
</tr>
<tr>
  <td>CheckId</td>
  <td>AvoidAsyncVoid</td>
</tr>
<tr>
  <td>Category</td>
  <td>Reliability Rules</td>
</tr>
</table>

## Cause

An asynchronous method has the return type `void`.

## Rule description

This diagnostic identifies code using `void`-returning `async` methods, and reports a warning.

## How to fix violations

To fix a violation of this rule, change the signature of the method to return a `Task` instead of `void`.

## How to suppress violations

```csharp
#pragma warning disable AvoidAsyncVoid // Avoid async void
private async void HandleSomeEvent(object sender, EventArgs e)
#pragma warning restore AvoidAsyncVoid // Avoid async void
{
}
```
