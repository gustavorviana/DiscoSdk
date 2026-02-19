namespace DiscoSdk.Commands;

/// <summary>
/// Indicates that the parameter value should be resolved from the dependency injection container.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class FromServicesAttribute : Attribute;
