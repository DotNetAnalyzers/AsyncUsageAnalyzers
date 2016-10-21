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
Thread.Sleep(0) causes the thread to relinquishes the remainder of its time slice to any thread of equal priority that is ready to run.
Thread.Sleep(...) with non-zero argument suspends the thread. 
Suspended thread cannot be used to execute other code which is undesirable since threads are quite expensive to create and take significant amount of memory.
Switching between can decrease program's performance.
Thread.Sleep should not be used to run an action periodically because it is imprecise (since it depends on OS's thread scheduler) and inefficient.
Thread.Sleep(...) on UI thread pauses message pumping which makes the app unresponsive.
There are cases when using Thread.Sleep() method is valid.

## How to fix violations

Thrad.Sleep with non-zero argument in async code can be replaced with "await System.Threading.Tasks.Task.Delay(...)"; Thread.Sleep(0) in async code can be replaced with "await System.Threading.Tasks.Yield()".
If Thread.Sleep is used to run actions periodically, consider using timer or appropriate observable instead.
If you are sure that using Thread.Sleep() is valid, suppress violations as described below.
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
