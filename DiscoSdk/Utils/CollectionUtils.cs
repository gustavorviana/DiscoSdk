namespace DiscoSdk.Utils
{
    internal static class CollectionUtils
    {
        public static bool DictionaryEquals(Dictionary<string, string>? left, Dictionary<string, string>? right)
        {
            if (left == null && right == null)
                return true;
            if (left == null || right == null)
                return false;
            if (left.Count != right.Count)
                return false;

            foreach (var kvp in left)
            {
                if (!right.TryGetValue(kvp.Key, out var value) || !string.Equals(kvp.Value, value, StringComparison.Ordinal))
                    return false;
            }

            return true;
        }

        public static bool SequenceEquals<T>(ICollection<T>? left, ICollection<T>? right)
        {
            if (left == null && right == null)
                return true;
            if (left == null || right == null)
                return false;
            if (left.Count != right.Count)
                return false;

            return left.SequenceEqual(right);
        }

    }
}
