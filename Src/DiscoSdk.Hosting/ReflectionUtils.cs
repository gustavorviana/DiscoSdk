using System.Reflection;

namespace DiscoSdk.Hosting;

internal class ReflectionUtils
{
    public static MethodInfo? FindInterfaceMethod(
        Type targetType,
        Type interfaceType,
        string methodName)
    {
        if (!interfaceType.IsInterface)
            throw new ArgumentException("Type must be an interface.", nameof(interfaceType));

        if (!interfaceType.IsAssignableFrom(targetType))
            return null;

        var interfaceMethods = interfaceType
            .GetMethods()
            .Where(m => m.Name == methodName)
            .ToArray();

        if (interfaceMethods.Length == 0)
            return null;

        var map = targetType.GetInterfaceMap(interfaceType);

        for (int i = 0; i < map.InterfaceMethods.Length; i++)
        {
            if (interfaceMethods.Contains(map.InterfaceMethods[i]))
                return map.TargetMethods[i];
        }

        return null;
    }
}
