using System.Reflection;

namespace DiscoSdk.Hosting.Commands;

internal class CommandReflection
{
    public static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
}
