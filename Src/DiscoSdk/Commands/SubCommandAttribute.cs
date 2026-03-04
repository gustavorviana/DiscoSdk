namespace DiscoSdk.Commands;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class SubCommandAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }

    public SubCommandAttribute(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Subcommand name cannot be null, empty, or whitespace.", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Subcommand description cannot be null, empty, or whitespace.", nameof(description));

        Name = name;
        Description = description;
    }
}
