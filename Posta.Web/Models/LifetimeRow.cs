namespace Posta.Web.Models;
public sealed class TransientProbe { public Guid Id { get; } = Guid.NewGuid(); }
public sealed class ScopedProbe { public Guid Id { get; } = Guid.NewGuid(); }
public sealed class SingletonProbe { public Guid Id { get; } = Guid.NewGuid(); }
public record LifetimeRow(string Name, string Description, Guid First, Guid Second);
