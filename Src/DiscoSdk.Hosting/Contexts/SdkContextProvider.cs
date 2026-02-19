using DiscoSdk.Contexts;

namespace DiscoSdk.Hosting.Contexts;

internal class SdkContextProvider : ISdkContextProvider
{
    private IContext? _context;

    public void SetContext(IContext context) => _context = context;

    public IContext GetContext() => _context!;
}
