using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MultiMap.Helpers
{
    /// <summary>
    /// Internal static class that provides guard methods for validating method arguments, such as checking for null values. These methods throw appropriate exceptions when validation fails, helping to ensure that public APIs are used correctly and to prevent common programming errors.
    /// </summary>
    internal static class Guard
    {
        /// <summary>
        /// Validates that a reference type argument is not null.
        /// </summary>
        /// <typeparam name="T">The reference type to validate.</typeparam>
        /// <param name="value">The argument to validate.</param>
        /// <param name="paramName">The name of the parameter being validated.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void NotNull<T>(
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    [NotNull]
#endif
    T? value, string paramName)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(value, paramName);
#else
            if (value is null)
                throw new ArgumentNullException(paramName);
#endif
        }

        /// <summary>
        /// Validates that a value is not null.
        /// </summary>
        /// <typeparam name="T">The type of the value to validate.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="paramName">The name of the parameter being validated.</param>
        /// <param name="message">The error message to use if the value is null.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        internal static void NotNull<T>(T? value, string paramName, string message)
        {
            if (value is null)
                throw new ArgumentNullException(paramName, message);
        }
    }
}
