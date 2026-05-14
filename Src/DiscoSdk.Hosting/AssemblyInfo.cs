using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


[assembly: ComVisible(false)]
[assembly: Guid("7AB42F04-D8B5-49DC-8FB5-3F1A4AF385C1")]
[assembly: InternalsVisibleTo("DiscoSdk.Tests")]
[assembly: InternalsVisibleTo("DiscoSdk.Hosting.Tests")]
// Required for NSubstitute (Castle DynamicProxy) to mock internal types like IMemberChunkSink.
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]