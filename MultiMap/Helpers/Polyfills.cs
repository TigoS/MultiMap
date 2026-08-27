// All conditional-compilation polyfills for the MultiMap library are centralised here.
// Targets: net8.0, net9.0, net10.0 — .NET Standard 2.0 was dropped in v3.0.0.
//
// Remaining version-gated feature:
//   HashSet<T>.AsReadOnly()  — available from .NET 10 onward.
//   On earlier targets we return an array snapshot instead.

namespace MultiMap.Helpers
{
    internal static class Polyfills
    {
        /// <summary>
        /// Returns a read-only view of <paramref name="source"/> on .NET 10+,
        /// or a defensive array copy on earlier targets.
        /// </summary>
        internal static IEnumerable<T> AsReadOnlyOrSnapshot<T>(HashSet<T> source)
        {
#if NET10_0_OR_GREATER
            return source.AsReadOnly();
#else
            return source.ToArray();
#endif
        }
    }
}
