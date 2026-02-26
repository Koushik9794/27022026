# Wolverine Migration Guide

## Overview

This guide provides step-by-step instructions for migrating services from **MediatR** to **WolverineFx** based on successful migrations of `admin-service` and `catalog-service`.

---

## Prerequisites

- .NET 8.0 or .NET 10.0
- PowerShell (for batch operations)
- Understanding of CQRS pattern

---

## Migration Steps

### Step 1: Update Package References

Replace MediatR packages with WolverineFx in your `.csproj` file:

```xml
<!-- Remove -->
<PackageReference Include="MediatR" Version="11.1.0" />
<PackageReference Include="MediatR.Extensions.Microsoft.DependencyInjection" Version="11.1.0" />

<!-- Add -->
<PackageReference Include="WolverineFx" Version="5.9.2" />
```

> [!IMPORTANT]
> The package name is `WolverineFx`, not `Wolverine`!

---

### Step 2: Fix Unicode Escape Sequences

Run this PowerShell command in your service's `src` directory:

```powershell
Get-ChildItem -Path . -Recurse -Include *.cs | ForEach-Object { 
    (Get-Content $_.FullName -Raw) -replace '\\u003c', '<' -replace '\\u003e', '>' | 
    Set-Content $_.FullName -NoNewline 
}
```

This fixes any `\\u003c` (<) and `\\u003e` (>) characters that may have been introduced.

---

### Step 3: Remove MediatR Interfaces

Run this PowerShell command in your service root directory:

```powershell
Get-ChildItem -Path .\src\application -Recurse -Include *.cs | ForEach-Object { 
    $content = Get-Content $_.FullName -Raw
    $content = $content -replace ' : IRequest<[^>]+>', ''
    $content = $content -replace ' : IRequestHandler<[^>]+>', ''
    $content = $content -replace 'using MediatR;', ''
    Set-Content $_.FullName -Value $content -NoNewline 
}
```

This removes:
- `: IRequest<T>` from commands/queries
- `: IRequestHandler<TRequest, TResponse>` from handlers
- `using MediatR;` statements

---

### Step 4: Update Program.cs

Replace MediatR registration with Wolverine:

**Before:**
```csharp
using MediatR;

builder.Services.AddMediatR(typeof(Program));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

**After:**
```csharp
using Wolverine;

builder.Host.UseWolverine(opts =>
{
    // Auto-discover handlers in the application assembly
    opts.Discovery.IncludeAssembly(typeof(YourHandlerClass).Assembly);
    
    // Configure policies
    opts.Policies.AutoApplyTransactions();
});
```

---

### Step 5: Update Controllers

Replace `IMediator` with `IMessageBus`:

**Before:**
```csharp
using MediatR;

private readonly IMediator _mediator;

public MyController(IMediator mediator)
{
    _mediator = mediator;
}

var result = await _mediator.Send(command);
```

**After:**
```csharp
using Wolverine;

private readonly IMessageBus _messageBus;

public MyController(IMessageBus messageBus)
{
    _messageBus = messageBus;
}

// For commands/queries with return values
var result = await _messageBus.InvokeAsync<TResponse>(command);

// For commands without return values (void)
await _messageBus.InvokeAsync(command);
```

**Batch update controllers:**
```powershell
Get-ChildItem -Path .\src\api -Recurse -Include *.cs | ForEach-Object { 
    $content = Get-Content $_.FullName -Raw
    $content = $content -replace 'using MediatR;', 'using Wolverine;'
    $content = $content -replace 'IMediator', 'IMessageBus'
    $content = $content -replace '\.Send\(', '.InvokeAsync('
    $content = $content -replace '\.Send<', '.InvokeAsync<'
    Set-Content $_.FullName -Value $content -NoNewline 
}
```

> [!WARNING]
> After batch update, manually add type parameters to `InvokeAsync` calls where needed!

---

### Step 6: Fix InvokeAsync Type Parameters

Wolverine's `InvokeAsync` requires explicit type parameters for return values:

```csharp
// ✅ Correct
var user = await _messageBus.InvokeAsync<UserDto>(new GetUserByIdQuery(id));
var id = await _messageBus.InvokeAsync<Guid>(new CreateUserCommand(...));
var success = await _messageBus.InvokeAsync<bool>(new UpdateUserCommand(...));

// ❌ Incorrect - will cause CS0815 error
var user = await _messageBus.InvokeAsync(new GetUserByIdQuery(id));
```

---

## Common Issues & Solutions

### Issue 1: "Cannot find package Wolverine"
**Solution**: Use `WolverineFx` instead of `Wolverine`

### Issue 2: "CS0815: Cannot assign void to an implicitly-typed variable"
**Solution**: Add type parameter to `InvokeAsync`:
```csharp
// Change this:
var result = await _messageBus.InvokeAsync(command);

// To this:
var result = await _messageBus.InvokeAsync<TResponse>(command);
```

### Issue 3: "CS1022: Type or namespace definition expected"
**Solution**: Check for stray `>` characters left by regex replacement. Manually inspect and fix query/command files.

### Issue 4: Unicode escape sequences (`\\u003c`, `\\u003e`)
**Solution**: Run the PowerShell script from Step 2

---

## Verification Checklist

- [ ] All `.csproj` files updated with `WolverineFx` package
- [ ] No `using MediatR;` statements remain
- [ ] No `: IRequest<T>` or `: IRequestHandler<T, R>` interfaces remain
- [ ] Program.cs uses `UseWolverine()` instead of `AddMediatR()`
- [ ] Controllers use `IMessageBus` instead of `IMediator`
- [ ] All `InvokeAsync` calls have explicit type parameters where needed
- [ ] Build succeeds with 0 errors
- [ ] All tests pass

---

## Migration Time Estimates

| Service Size | Estimated Time |
|--------------|----------------|
| Small (5-10 handlers) | 2-3 hours |
| Medium (10-20 handlers) | 4-6 hours |
| Large (20+ handlers) | 1-2 days |

**Note**: Time includes testing and troubleshooting

---

## Quick Reference

| MediatR | Wolverine |
|---------|-----------|
| `IMediator` | `IMessageBus` |
| `.Send(command)` | `.InvokeAsync<T>(command)` |
| `IRequest<T>` | *(no interface needed)* |
| `IRequestHandler<T, R>` | *(no interface needed)* |
| `INotification` | *(no interface needed)* |
| `IPipelineBehavior<T, R>` | Middleware pattern |
| `AddMediatR()` | `UseWolverine()` |
