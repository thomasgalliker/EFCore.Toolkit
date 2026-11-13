# EFCore.Toolkit
[![Version](https://img.shields.io/nuget/v/EFCore.Toolkit.svg)](https://www.nuget.org/packages/EFCore.Toolkit)  [![Downloads](https://img.shields.io/nuget/dt/EFCore.Toolkit.svg)](https://www.nuget.org/packages/EFCore.Toolkit)

<img src="https://raw.githubusercontent.com/thomasgalliker/EFCore.Toolkit/develop/logo.png" width="100" height="100" alt="EFCore.Toolkit" align="right">
EFCore.Toolkit is a library which provides implementations for EntityFramework best practices, patterns, utilities and extensions.
- Patterns such as UnitOfWork, Repository
- Helper classes, e.g. DbContextBase, ContextTestBase
- Generic way to seed data using IDataSeed and DataSeedBase
- DbConnection abstraction IDbConnection to inject ConnectionString into EntityFramework context
- EDMX tools to generate *.edmx file from EntityFramework context (when doing EF code-first)

### Download and Install EFCore.Toolkit
This library is available on NuGet: https://www.nuget.org/packages/EFCore.Toolkit/
Use the following command to install EFCore.Toolkit using NuGet package manager console:

    PM> Install-Package EFCore.Toolkit
	
For certain assemblies you may only want to use abstractions used in EFCore.Toolkit. In this case, you can install EFCore.Toolkit.Abstractions which is available as a seperate Nuget package and is absolutely free of any external dependencies.

	PM> Install-Package EFCore.Toolkit.Abstractions

### API Usage
#### Repository pattern and GenericRepository
As the name implies, ```GenericRepository<T>``` implements the repository pattern for the generic type argument T. The repository pattern is basically a set of CRUD commands (create, read, update, delete) for the given entity type T. Depending on the application that consumes the data layer, CRUD operations are totally sufficient. However, some entity types may require more sophisticated CRUD logic. In this situation, you may want to override the virtual methods of GenericRepository.

#### UnitOfWork pattern
The UnitOfWork (UOW) pattern is by definition a way to commit a set of defined work steps or -if one step cannot be performed for whatever reason- to rollback all steps. The UOW implementation in this library is capable of committing to multiple EntityFramework DbContexts in one go. To do so, it makes use of System.Transaction, which involves MSDTC. The working mode and configuration of MSDTC is beyond this documentation. Further reading: http://martinfowler.com/eaaCatalog/unitOfWork.html

#### Auditing
Tracking of data changes is an important feature of enterprise applications. EFCore.Toolkit provides simple auditing functionality trough its specialized DbContext ```AuditDbContextBase<TContext>```. There are mainly following features implemented:

##### Track creation and update date of particular entities
Apply ```ICreatedDate``` and/or ```IUpdatedDate``` to automatically track the CreatedDate resp. the UpdatedDate on SaveChanges.

##### Write history of entity changes to a seperate audit log
Define an audit entity which implements ```IAuditEntity``` resp. inherits from the predefined ```AuditEntity``` base class. Then, define a mapping between your entity and the audit entity (e.g. Employee + EmployeeAudit). This mapping can basically be achieved by either providing an [App.config](https://github.com/thomasgalliker/EFCore.Toolkit/blob/master/%20EFCore.Toolkit.Tests/App.config) or by programmatically calling [```RegisterAuditType```](https://github.com/thomasgalliker/EFCore.Toolkit/blob/master/%20EFCore.Toolkit.Tests/Auditing/AuditDbContextBaseTests.cs). Changes are automatically audited to the configured audit entity table on SaveChanges.

#### Generic Data Seed with IDataSeed
Providing databases with predefined data is an essential feature. IDataSeed is the interface which abstracts the data seed of one particular entity type. Use the abstract base class ```DataSeedBase<T>``` to have the least possible effort to provide a data seed. ```DataSeedBase<T>``` allows you to define an AddOrUpdateExpression which is evaluated in order to check whether a certain entity of type T is already in the database or if it needs to be added. 

#### Query Filters
Use `ApplyQueryFilter<TInterface>` to apply a global query filter to every entity type which implements a specific interface. This is useful for cross-cutting filters such as soft delete, tenant isolation, or user-specific data access.

```C#
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.ApplyQueryFilter<IDeletable>(entity => !entity.IsDeleted);
    modelBuilder.ApplyQueryFilter<ICreatedBy>(entity => entity.UserId == this.currentUserId);
}
```

The filter expression is automatically rewritten for each matching entity CLR type. When targeting EF Core 10 or later, filters are stored as named query filters using the interface full name as the filter key. Applying another filter for the same interface combines it with the existing filter using `AndAlso`; applying filters for different interfaces stores them separately and EF Core applies them together.

### EFCore.Toolkit and IoC
EFCore.Toolkit is ready to be used with an IoC framework. You may intend to create a data access module which contains your EF context, the repositories, the entity type configurations, etc. On top of that, you want to promote the CRUD-style repositories to whoever want to consume your data access layer. So, simply create a seperate data access abstraction assembly which contains an interface definition for your repositories. Have a look at the ToolkitSample provided in this project. This sample project wires up modularity using `Microsoft.Extensions.DependencyInjection`. Have a look at the `AddDataAccess` extension method to get an impression of how to set-up the dependencies.

```C#
// Register all data seeds:
services.AddSingleton<IDataSeed, DepartmentDataSeed>();

// Register an IDatabaseInitializer which is injected into EmployeeContext
services.AddSingleton<IDatabaseInitializer, EmployeeContextDatabaseInitializer>();

// Finally, register the context and all the repositories as transient
services.AddTransient<EmployeeContext>(sp =>
    new EmployeeContext(
        EmployeeContextDbContextOptions.Create<EmployeeContext>(),
        sp.GetRequiredService<IDatabaseInitializer>()));
services.AddTransient<IEmployeeContext>(sp => sp.GetRequiredService<EmployeeContext>());
services.AddTransient<IEmployeeRepository, EmployeeRepository>();
```

Depending on your application, you may need to change the lifetime of your EF context from transient to scoped. It is recommended to give the EF context (and therefore all its descendants, e.g. the repositories and the units of work) a minimal lifetime scope only. You should avoid to have a singleton instance of the context!

### Contribution
If you have any further ideas or specific needs, do not hesitate to submit a [new issue](https://github.com/thomasgalliker/EFCore.Toolkit/issues).

### License
This project is Copyright &copy; 2026 [Thomas Galliker](https://ch.linkedin.com/in/thomasgalliker). Free for non-commercial use. For commercial use please contact the author.
