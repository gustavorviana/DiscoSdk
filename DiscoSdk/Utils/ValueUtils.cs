namespace DiscoSdk.Utils
{
    internal static class ValueUtils
    {
        public static bool ValueEquals(object? left, object? right)
        {
            if (left == null && right == null)
                return true;

            if (left == null || right == null)
                return false;

            // Compare numeric values properly
            if (left is IConvertible && right is IConvertible)
            {
                try
                {
                    var leftDouble = Convert.ToDouble(left);
                    var rightDouble = Convert.ToDouble(right);
                    return Math.Abs(leftDouble - rightDouble) < double.Epsilon;
                }
                catch
                {
                    // Fall through to reference equality
                }
            }

            return Equals(left, right);
        }

        public static bool UnsafeBoolComparer(bool? left, bool? right)
        {
            if (left == null && right == null)
                return true;

            if (left == null || right == null)
                return true;

            return left == right;
        }
    }
}
