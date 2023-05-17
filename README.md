# iCuePlumber
Monitors iCUE for memory leaks and automatically restarts the service.

I'll probably rewrite this for .NET Core when I get the chance.


# Installation

```
iCUEPlumber.exe install
```

### Optional Arguments


```
iCUEPlumber.exe install /r:<polling_rate_in_ms> /m:<memory_threshold_in_kb>
```