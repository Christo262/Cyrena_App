## Overview

`Cyrena.Persistence.Core` is the persistence abstraction layer for the Cyréna framework. It defines:

- The generic repository contract `IStore<T>` for entity persistence
- The persistence builder interface `ICyrenaPersistenceBuilder` for DI registration
- The specification pattern types (`ISpecification<T>`, `Specification<T>`, and the `And/Or/Not/Identity/All/EntityId` concrete specs)
- The ordering types (`IOrderBy<T>`, `OrderBy<T>`, `SortDirection`)
- The paging types (`IPaging`, `Paging`)
- Extension methods on `CyrenaBuilder` for adding scoped/singleton stores
- Extension methods on `IStore<T>` for lambda-predicate overloads

Concrete implementations of `IStore<T>` (e.g. `Cyrena.Persistence.File` with `FileStore<T>`) provide the actual storage backend. `Cyrena.Persistence.Core` itself does **not** contain any I/O — it is the contract layer that every persistence implementation must satisfy.

**Version:** 0.6.0
**Target Framework:** .NET 10.0
**Namespaces:** `Cyrena.Persistence`, `Cyrena.Persistence.Contracts`, `Cyrena.Persistence.Options`, `Cyrena.Persistence.Specifications`, `Cyrena.Extensions`

---

## Contracts

### `IStore<T>` (`Cyrena.Persistence.Contracts`)

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

All operations are async and support `CancellationToken`. `SaveAsync` is an upsert; `AddAsync` / `AddManyAsync` insert; `UpdateAsync` updates. The three query methods accept a specification, plus optional ordering and paging.

---

## Options

### `ICyrenaPersistenceBuilder` (`Cyrena.Persistence.Options`)

```csharp
public interface ICyrenaPersistenceBuilder
{
    void AddScopedStore<TEntity>(string collectionName) where TEntity : class, IEntity;
    void AddSingletonStore<TEntity>(string collectionName) where TEntity : class, IEntity;
}
```

Helper interface for registering `IStore<TEntity>` services. Persistence implementation packages register an implementation of this in `CyrenaBuilder.FeatureOptions`. The `AddScopedStore` / `AddSingletonStore` extension methods on `CyrenaBuilder` resolve this builder and forward calls.

---

## Specifications (`Cyrena.Persistence` and `Cyrena.Persistence.Specifications`)

The specification pattern lets you compose composable query predicates.

### `ISpecification<T>` (`Cyrena.Persistence`)
```csharp
public interface ISpecification<T>
{
    bool IsSatisfiedBy(T entity);
    Expression<Func<T, bool>> ToExpression();
}
```

### `Specification<T>` (`Cyrena.Persistence`) — abstract base
```csharp
public abstract class Specification<T> : ISpecification<T>
{
    public static readonly ISpecification<T> Identity = new IdentitySpecification<T>();
    public static readonly ISpecification<T> None = Identity.Not();

    public virtual bool IsSatisfiedBy(T entity)
    {
        _predicate ??= ToExpression().Compile();
        return _predicate(entity);
    }

    public abstract Expression<Func<T, bool>> ToExpression();
}
```

The base class caches the compiled predicate per instance. `Identity` always matches; `None` is its negation.

### `AndSpecification<T>` (`Cyrena.Persistence`) — `struct`
```csharp
public struct AndSpecification<T> : ISpecification<T>
{
    public AndSpecification(ISpecification<T> left, ISpecification<T> right);
    public bool IsSatisfiedBy(T entity) => Left.IsSatisfiedBy(entity) && Right.IsSatisfiedBy(entity);
    public Expression<Func<T, bool>> ToExpression() => Left.ToExpression().And(Right.ToExpression());
    public ISpecification<T> Left { get; }
    public ISpecification<T> Right { get; }
}
```

### `OrSpecification<T>` (`Cyrena.Persistence`) — `struct`
```csharp
public struct OrSpecification<T> : ISpecification<T>
{
    public OrSpecification(ISpecification<T> left, ISpecification<T> right);
    public bool IsSatisfiedBy(T entity) => _left.IsSatisfiedBy(entity) || _right.IsSatisfiedBy(entity);
    public Expression<Func<T, bool>> ToExpression() => _left.ToExpression().Or(_right.ToExpression());
}
```

### `NotSpecification<T>` (`Cyrena.Persistence`) — `struct`
```csharp
public struct NotSpecification<T> : ISpecification<T>
{
    public NotSpecification(ISpecification<T> specification);
    public bool IsSatisfiedBy(T entity) => !_specification.IsSatisfiedBy(entity);
    public Expression<Func<T, bool>> ToExpression()
        => Expression.Lambda<Func<T, bool>>(Expression.Not(expression.Body), expression.Parameters.Single());
}
```

### `IdentitySpecification<T>` (`Cyrena.Persistence`) — `internal struct`
```csharp
internal struct IdentitySpecification<T> : ISpecification<T>
{
    public bool IsSatisfiedBy(T entity) => true;
    public Expression<Func<T, bool>> ToExpression() => x => true;
}
```

Used as the seed value for `And` aggregations and as the `Identity` constant on `Specification<T>`.

### `AllSpecification<T>` (`Cyrena.Persistence.Specifications`)
```csharp
public class AllSpecification<T> : Specification<T>
    where T : class, IEntity
{
    public override Expression<Func<T, bool>> ToExpression() => x => true;
}
```

Always matches. Convenience for "find all" calls.

### `EntityIdSpecification<T>` (`Cyrena.Persistence.Specifications`)
```csharp
public class EntityIdSpecification<T> : Specification<T>
    where T : class, IEntity
{
    public EntityIdSpecification(string id);
    public override Expression<Func<T, bool>> ToExpression() => x => x.Id == _id;
}
```

Matches entities whose `IEntity.Id` equals the supplied id.

### `SpecificationExtensions` (`Cyrena.Persistence`)
```csharp
public static class SpecificationExtensions
{
    public static ISpecification<T> And<T>(this ISpecification<T> @this, ISpecification<T> specification);
    public static ISpecification<T> Or<T>(this ISpecification<T> @this, ISpecification<T> specification);
    public static ISpecification<T> Not<T>(this ISpecification<T> @this);
}
```

`And` short-circuits on `Identity`. `Or` short-circuits if either side is `Identity` — returns `Identity` itself.

### `SpecificationCollectionsExtensions` (`Cyrena.Persistence`)
```csharp
public static class SpecificationCollectionsExtensions
{
    public static ISpecification<T> ToAndSpecification<T>(this IEnumerable<ISpecification<T>> specifications);
    public static ISpecification<TChecked> ToAndSpecification<TSource, TChecked>(this IEnumerable<TSource> source, Func<TSource, ISpecification<TChecked>> specificationFactory);
    public static ISpecification<T> ToOrSpecification<T>(this IEnumerable<ISpecification<T>> specifications);
    public static ISpecification<TChecked> ToOrSpecification<TSource, TChecked>(this IEnumerable<TSource> source, Func<TSource, ISpecification<TChecked>> specificationFactory);
}
```

Aggregates a collection of specifications with `And` (seeded by `Identity`) or `Or` (seeded by `None`).

---

## Ordering (`Cyrena.Persistence`)

### `SortDirection`
```csharp
public enum SortDirection
{
    Ascending,
    Descending,
}
```

### `IOrderBy<T>` and `OrderBy<T>`
```csharp
public interface IOrderBy<T>
{
    Expression<Func<T, object>> OrderByExpression { get; }
    SortDirection SortDirection { get; }
}

public class OrderBy<T> : IOrderBy<T>
{
    public OrderBy(Expression<Func<T, object>> orderByExpression, SortDirection sortDirection);
    public Expression<Func<T, object>> OrderByExpression { get; }
    public SortDirection SortDirection { get; }
}
```

### `OrderBySpecification`
```csharp
public static class OrderBySpecification
{
    public static OrderBy<T> OrderBy<T>(Expression<Func<T, object>> expression, SortDirection sortDirection = SortDirection.Ascending);
    public static OrderBy<T> OrderByDescending<T>(Expression<Func<T, object>> expression, SortDirection sortDirection = SortDirection.Descending);
}
```

---

## Paging (`Cyrena.Persistence`)

### `IPaging` and `Paging`
```csharp
public interface IPaging
{
    int Skip { get; }
    int Take { get; }
}

public class Paging : IPaging
{
    public Paging(int skip, int take);

    public int Skip { get; }
    public int Take { get; }

    public static Paging Page(int page, int pageSize) => new(page * pageSize, pageSize);
    public static Paging? Create(int? skip, int? take) => skip != null && take != null ? new Paging(skip.Value, take.Value) : default;
}
```

`Page(page, pageSize)` is `page * pageSize` skip with `pageSize` take. `Create` returns `null` if either input is `null`.

---

## Extension Methods (`Cyrena.Extensions`)

### `CyrenaBuilderExtensions` (Persistence.Core)
```csharp
public static class CyrenaBuilderExtensions
{
    public static CyrenaBuilder AddScopedStore<TEntity>(this CyrenaBuilder builder, string collectionName)
        where TEntity : class, IEntity;

    public static CyrenaBuilder AddSingletonStore<TEntity>(this CyrenaBuilder builder, string collectionName)
        where TEntity : class, IEntity;
}
```

Both delegate to `builder.GetFeatureOption<ICyrenaPersistenceBuilder>()` then call `AddScopedStore` / `AddSingletonStore` on that builder.

### `StoreExtensions` (Persistence.Core)
```csharp
public static class StoreExtensions
{
    public static Task<T?> FindAsync<T>(this IStore<T> store, Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        where T : class, IEntity;

    public static Task<IEnumerable<T>> FindManyAsync<T>(this IStore<T> store, Expression<Func<T, bool>> predicate, OrderBy<T>? orderBy = null, Paging? paging = null, CancellationToken ct = default)
        where T : class, IEntity;

    public static Task<int> CountAsync<T>(this IStore<T> store, Expression<Func<T, bool>> predicate, CancellationToken ct = default) where T : class, IEntity;

    public static Task<int> DeleteManyAsync<T>(this IStore<T> store, Expression<Func<T, bool>> predicate, CancellationToken ct = default) where T : class, IEntity;
}
```

The lambda is wrapped in an `internal` `AnySpecification<T> : Specification<T>` defined in the same file. The store must already be registered (e.g. via `AddScopedStore<T>` or `AddSingletonStore<T>`).

---

## Usage for Extension Developers

Reference `Cyrena.Persistence.Core` to:
1. Define custom entities that implement `IEntity` (from `Cyrena.Core`)
2. Use `IStore<TEntity>` for data access in services
3. Register stores via `CyrenaBuilder.AddScopedStore<TEntity>()` or `AddSingletonStore<TEntity>()`
4. Query data using `ISpecification<T>` or `Expression<Func<T, bool>>` via `StoreExtensions`
5. Combine specifications with `And`/`Or`/`Not` and `ToAndSpecification` / `ToOrSpecification`
6. Order with `OrderBySpecification.OrderBy(...)` and page with `Paging.Page(...)`

**Example:**
```csharp
// Define entity
public class MyData : Entity { public string Value { get; set; } }

// Register store in extension
builder.AddScopedStore<MyData>("my-data");

// Use in service
public class MyService(IStore<MyData> store)
{
    public Task<MyData?> Find(string id) =>
        store.FindAsync(x => x.Id == id);
}
```

A concrete persistence backend (e.g. `Cyrena.Persistence.File`) must be referenced and added to the `CyrenaBuilder` for these stores to resolve.

---

## Project Dependencies
- `Cyrena.Core` (for `IEntity`, `Entity`, `CyrenaBuilder`, `GetFeatureOption`)

---

## Package Information
- **PackageId**: `Cyrena.Persistence.Core`
- **Version**: `0.6.0`
- **Title**: Cyréna Persistence Core
- **ProjectUrl**: https://cyrena.dev
- **Repository**: https://github.com/Christo262/Cyrena_App
- **PostPack Target**: Copies NuGet package to `../../../sdk`