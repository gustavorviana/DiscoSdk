using DiscoSdk.Models.Channels;
using System.Diagnostics.CodeAnalysis;

namespace DiscoSdk.Hosting.EqualityComparers
{
    internal class ChannelEqualityComparerById : IEqualityComparer<Channel>
    {
        public bool Equals(Channel? x, Channel? y)
            => x?.Id == y?.Id;

        public int GetHashCode([DisallowNull] Channel obj)
            => obj.Id.GetHashCode();
    }
}