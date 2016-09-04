## DontUseThreadSleep

<table>
<tr>
  <td>TypeName</td>
  <td>DontUseThreadSleep</td>
</tr>
<tr>
  <td>CheckId</td>
  <td>DontUseThreadSleep</td>
</tr>
<tr>
  <td>Category</td>
  <td>Usage Rules</td>
</tr>
</table>

## Cause

System.Threading.Thread.Sleep() method is called in the code.

## Rule description

System.Threading.Thread.Sleep() method is called in the code. 
If this method is in code which can executed asynchronously, the code is not optimal - the thread that is sleeping cannot execute any other tasks.
Note that non-async method might be called from asynchronous code and in such circumstances the thread cannot execute other task.
In such case, consider refactoring your code.
There are cases when using Thread.Sleep() method is valid.

## How to fix violations

If identified method call is inside asynchronous method or function, consider using "await System.Threading.Tasks.Task.Delay(...)" as an alternative.
If you are sure that using Thread.Sleep() is approprate, suppress violations as described below.
You may use less strict rule (e.g. DontUseThreadSleepInAsyncCode) or opt-out of this rule completely.

## How to suppress violations

```csharp
[SuppressMessage("AsyncUsage.CSharp.Usage", "DontUseThreadSleep", Justification = "Reviewed.")]
```

```csharp
#pragma warning disable DontUseThreadSleep // Use Async suffix
Thread.Sleep(1000)
#pragma warning restore DontUseThreadSleep // Use Async suffix
```
