# NEST.Linq
ElasticSearch NEST Linq extension library which provides possibility to use LINQ over NEST elastic queries.

1) Search example using LINQ extension methods:

```csharp
client // Nest.IElasticClient instance
    .AsQueryable<User>()
    .Where(u => u.Name == "Vlad")
    .Skip(5)
    .Take(10)
    .Search();
```

This code will be converted to next query using NEST fluent syntax:

```csharp
client.Search<User>(s => s
    .From(5)
    .Size(10)
    .Query(q => q
        .Match(m => m
            .Field(f => f.Name)
            .Query("Vlad"))));
```


2) Search example using LINQ query syntax:

```csharp
var users = (
    from user in client.AsQueryable<User>()
    where user.Name.StartsWith("V")
    where user.Name == "Vlad"
    select user).Search();
```

3) Also, this LINQ syntax could be easily integrated with existing NEST queries, if there are some specific logic which could not be described in LINQ syntax.

```csharp
client
    .AsQueryable<User>()
    .Skip(5)
    .Take(10)
    .Search(s => s
        .Query(q => q
            .Match(m => m
                .Field(f => f.Name).Query("Vlad"))));
```

### Consider the difference between NEST and NEST.ElasticallyQuariable filter requests 
| NEST | NEST.ElasticallyQuariable |
| --- | --- |
| `.Bool(b => b.Should(l => l.Match(m => m.Field(f => f.Name).Query("Alice")), l => l.Match(m => m.Field(f => f.Name).Query("Bob"))))` | `.Where(u => u.Name == "Alice" \|\| u.Name == "Bob")` |
