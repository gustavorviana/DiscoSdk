namespace DiscoSdk.Commands;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class SubCommandGroupAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }

    public SubCommandGroupAttribute(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Subcommand group name cannot be null, empty, or whitespace.", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Subcommand group description cannot be null, empty, or whitespace.", nameof(description));

        Name = name;
        Description = description;
    }
}
