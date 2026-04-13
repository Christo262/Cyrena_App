# Cyrena.Persistence.Core SDK

**Version:** 0.3.0  
**Target Framework:** .NET 10.0  
**Core Dependency:** `Cyrena.Core`

Cyrena.Persistence.Core is a thin persistence abstraction library that provides the `IStore<T>` repository pattern interface for long-term data storage within the Cyrena AI application. This library does not contain implementations—it defines contracts that persistence providers (e.g., MongoDB, SQLite, CosmosDB) must implement.

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    Cyrena Application                       │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐                                        │
│  │ Cyrena.Core     │  ← Provides IEntity, CyrenaBuilder     │
│  └────────┬────────┘                                        │
│           │                                                 │
│  ┌────────▼──────────────────────────────────────────────┐  │
│  │           Cyrena.Persistence.Core                     │  │
│  │                                                      │   │
│  │  Contracts:     IStore<T>                             │  │
│  │  Options:       ICyrenaPersistenceBuilder             │  │
│  │  Extensions:    StoreExtensions                      │   │
│  │                  CyrenaBuilderExtensions              │  │
│  └────────────────────────┬───────────────────────────────┘   │
│                           │                                   │
│  ┌────────────────────────▼───────────────────────────────┐ │
│  │     Persistence Provider (e.g., MongoDB, SQLite)       │ │
│  │     → Implements IStore<T> for each entity type         │ │
│  └──────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────┘
```

**Purpose:** This library enables application plugins and features to define custom entities for long-term storage without coupling to a specific persistence technology.

---

## Contracts

### IStore<T>

Main repository pattern interface for CRUD operations on entities. Implement this interface to provide persistence for any type that implements `IEntity`.

```csharp
public interface IStore<T> : IDisposable
        where T : class, IEntity
{
    // Access to raw queryable data
    IQueryable<T> QueryableData { get; }
    
    // Save entity (upsert - insert or update)
    Task SaveAsync(T entity, CancellationToken cancellationToken = default);
    
    // Insert new entity
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    
    // Insert multiple entities
    Task AddManyAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    
    // Update existing entity
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    
    // Delete single entity
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    
    // Delete entities matching specification
    Task<int> DeleteManyAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
    
    // Query entities with specification, ordering, and pagination
    Task<IEnumerable<T>> FindManyAsync(
        ISpecification<T> specification,
        IOrderBy<T>? orderBy = default,
        IPaging? paging = default,
        CancellationToken cancellationToken = default);
    
    // Count entities matching specification
    Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
    
    // Find single entity matching specification
    Task<T?> FindAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
}
```

**Key Design Points:**
- Generic `T` must implement `IEntity` (provides `Id` property)
- `ISpecification<T>` pattern for query composition (from LinqKit.Core)
- `IOrderBy<T>` and `IPaging` for sorting and pagination
- Implements `IDisposable` for resource cleanup

---

## Options

### ICyrenaPersistenceBuilder

Helper interface for registering entity stores during dependency injection configuration.

```csharp
public interface ICyrenaPersistenceBuilder
{
    // Register store as scoped (new instance per request)
    void AddScopedStore<TEntity>(string collectionName) where TEntity : class, IEntity;
    
    // Register store as singleton (shared instance)
    void AddSingletonStore<TEntity>(string collectionName) where TEntity : class, IEntity;
}
```

**Usage:** Used internally by extension methods to wire up stores. The `collectionName` parameter allows persistence providers to map entities to storage collections/tables.

---

## Extension Methods

### StoreExtensions

Helper methods providing LINQ expression-based queries as an alternative to specification-based queries.

```csharp
public static class StoreExtensions
{
    // Find single entity using LINQ expression (converted to AnySpecification)
    public static Task<T?> FindAsync<T>(
        this IStore<T> store,
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default) where T : class, IEntity;
    
    // Find many entities using LINQ expression
    public static Task<IEnumerable<T>> FindManyAsync<T>(
        this IStore<T> store,
        Expression<Func<T, bool>> predicate,
        OrderBy<T>? orderBy = null,
        Paging? paging = null,
        CancellationToken ct = default) where T : class, IEntity;
    
    // Count using LINQ expression
    public static Task<int> CountAsync<T>(
        this IStore<T> store,
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default) where T : class, IEntity;
    
    // Delete many using LINQ expression
    public static Task<int> DeleteManyAsync<T>(
        this IStore<T> store,
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default) where T : class, IEntity;
}
```

**Internal Implementation:** Uses `AnySpecification<T>` wrapper to convert `Expression<Func<T, bool>>` to `ISpecification<T>`.

### CyrenaBuilderExtensions

Extension methods for registering stores via the `CyrenaBuilder`.

```csharp
public static class CyrenaBuilderExtensions
{
    // Add scoped store to application
    public static CyrenaBuilder AddScopedStore<TEntity>(
        this CyrenaBuilder builder,
        string collectionName) where TEntity : class, IEntity;
    
    // Add singleton store to application
    public static CyrenaBuilder AddSingletonStore<TEntity>(
        this CyrenaBuilder builder,
        string collectionName) where TEntity : class, IEntity;
}
```

---

## Data Model Dependencies

This library depends on models defined in **Cyrena.Core**:

### IEntity
Base interface requiring an `Id` property.

```csharp
public interface IEntity
{
    string Id { get; }
}
```

### Specification<T>
Abstract base for query specifications (from LinqKit.Core).

### IOrderBy<T>, IPaging
Supporting interfaces for sorting and pagination queries.

---

## Usage Example

### Registering a Custom Entity Store

```csharp
// In your plugin's startup configuration
public class MyPluginStartup : IStartupTask
{
    public int Order => 0;
    
    public Task ExecuteAsync(IServiceProvider serviceProvider, CancellationToken ct)
    {
        var builder = serviceProvider.GetRequiredService<CyrenaBuilder>();
        
        // Register your entity store
        builder.AddScopedStore<MyCustomEntity>("my-entities");
        
        return Task.CompletedTask;
    }
}
```

### Using the Store in a Service

```csharp
public class MyService
{
    private readonly IStore<MyCustomEntity> _store;
    
    public MyService(IStore<MyCustomEntity> store)
    {
        _store = store;
    }
    
    public async Task<MyCustomEntity?> GetByIdAsync(string id, CancellationToken ct)
    {
        // Using specification pattern
        return await _store.FindAsync(
            new SimpleSpecification<MyCustomEntity>(e => e.Id == id),
            ct);
    }
    
    public async Task<IEnumerable<MyCustomEntity>> GetActiveAsync(int page, int pageSize, CancellationToken ct)
    {
        // Using LINQ expression extension
        return await _store.FindManyAsync(
            e => e.IsActive && !e.IsDeleted,
            orderBy: new OrderBy<MyCustomEntity>(e => e.CreatedAt, descending: true),
            paging: new Paging(page, pageSize),
            ct);
    }
    
    public async Task SaveAsync(MyCustomEntity entity, CancellationToken ct)
    {
        await _store.SaveAsync(entity, ct);
    }
}
```

---

## Package Dependencies

- **Cyrena.Core** — Core library providing `IEntity`, `CyrenaBuilder`, and base models
- **LinqKit.Core** — LINQ expression extensions for specification pattern

---

## Notes

- This library is an abstraction layer. Actual persistence implementations are provided by separate packages (e.g., Cyrena.Persistence.Mongo).
- The `ISpecification<T>`, `IOrderBy<T>`, and `IPaging` interfaces are re-exported from dependencies for convenience.
- All stores should be registered during application startup via `CyrenaBuilderExtensions`.
