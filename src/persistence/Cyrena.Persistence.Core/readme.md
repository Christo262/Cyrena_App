## Overview

`Cyrena.Persistence.Core` provides the persistence abstraction layer for the Cyréna framework. It defines the generic repository contract `IStore<T>`, the persistence builder interface `ICyrenaPersistenceBuilder`, and extension methods for convenient querying. Concrete implementations (e.g., `Cyrena.Persistence.File`) provide the actual storage backend.

**Target Framework:** .NET 10.0
**Namespaces:** `Cyrena.Persistence.Contracts`, `Cyrena.Persistence.Options`, `Cyrena.Extensions`

---

## Contracts

### `IStore<T>`
Generic repository interface for entity persistence. Supports CRUD operations, querying with specifications, ordering, and paging.

```csharp
public interface IStore<T> : IDisposable
    where T : class, IEntity
{
    IQueryable<T> QueryableData { get; }
    Task SaveAsync(T entity, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task AddManyAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task<int> DeleteManyAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindManyAsync(ISpecification<T> specification, IOrderBy<T>? orderBy = default, IPaging? paging = default, CancellationToken cancellationToken = default);
    Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
    Task<T?> FindAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
}
```

**Key behaviors:**
- `SaveAsync`: Upsert operation. Auto-generates `Id` if null.
- `AddAsync`: Insert operation. Auto-generates `Id` if null.
- `QueryableData`: Direct LINQ queryable access to the underlying collection.
- All operations are async and support cancellation.

### `ICyrenaPersistenceBuilder`
Helper interface for registering entity stores during DI configuration.

```csharp
public interface ICyrenaPersistenceBuilder
{
    void AddScopedStore<TEntity>(string collectionName) where TEntity : class, IEntity;
    void AddSingletonStore<TEntity>(string collectionName) where TEntity : class, IEntity;
}
```

- `AddScopedStore`: Registers `IStore<TEntity>` as a scoped service (one per chat/kernel).
- `AddSingletonStore`: Registers `IStore<TEntity>` as a singleton service (application-wide).
- `collectionName`: Logical collection/table name for the entity type.

---

## Extension Methods

### `CyrenaBuilderExtensions` (in `Cyrena.Extensions`)

```csharp
public static class CyrenaBuilderExtensions
{
    public static CyrenaBuilder AddScopedStore<TEntity>(this CyrenaBuilder builder, string collectionName) 
        where TEntity : class, IEntity;
    
    public static CyrenaBuilder AddSingletonStore<TEntity>(this CyrenaBuilder builder, string collectionName) 
        where TEntity : class, IEntity;
}
```

These methods delegate to the `ICyrenaPersistenceBuilder` registered in `CyrenaBuilder.FeatureOptions`. The persistence implementation package (e.g., `Cyrena.Persistence.File`) must be added first.

### `StoreExtensions` (in `Cyrena.Extensions`)

Convenience overloads using `Expression<Func<T, bool>>` predicates instead of `ISpecification<T>`.

```csharp
public static class StoreExtensions
{
    public static Task<T?> FindAsync<T>(this IStore<T> store, Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        where T : class, IEntity;
    
    public static Task<IEnumerable<T>> FindManyAsync<T>(this IStore<T> store, Expression<Func<T, bool>> predicate, 
        OrderBy<T>? orderBy = null, Paging? paging = null, CancellationToken ct = default)
        where T : class, IEntity;
    
    public static Task<int> CountAsync<T>(this IStore<T> store, Expression<Func<T, bool>> predicate, CancellationToken ct = default) 
        where T : class, IEntity;
    
    public static Task<int> DeleteManyAsync<T>(this IStore<T> store, Expression<Func<T, bool>> predicate, CancellationToken ct = default) 
        where T : class, IEntity;
}
```

These extensions wrap the predicate in an internal `AnySpecification<T>` that implements `ISpecification<T>`.

---

## Related Types (Defined in Implementation Packages)

The following types are referenced by `IStore<T>` and `StoreExtensions` but defined in concrete persistence implementation packages:

- `ISpecification<T>` - Query specification interface
- `Specification<T>` - Base specification implementation
- `IOrderBy<T>` - Ordering specification interface
- `OrderBy<T>` - Ordering implementation
- `IPaging` - Paging specification interface
- `Paging` - Paging implementation

When using `Cyrena.Persistence.Core`, you must also reference a concrete implementation package that provides these types.

---

## Usage for Extension Developers

Reference `Cyrena.Persistence.Core` to:
1. Define custom entities that implement `IEntity`
2. Use `IStore<TEntity>` for data access in services
3. Register stores via `CyrenaBuilder.AddScopedStore<TEntity>()` or `AddSingletonStore<TEntity>()`
4. Query data using LINQ expressions via `StoreExtensions`

**Example:**
```csharp
// Define entity
public class MyData : Entity { public string Value { get; set; } }

// Register store in extension
builder.AddScopedStore<MyData>("my-data");

// Use in service
public class MyService(IStore<MyData> store) { ... }
```