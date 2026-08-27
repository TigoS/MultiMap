#if NETSTANDARD2_0
// Polyfill: NotNullAttribute is part of the BCL from .NET Core 3.0 / .NET Standard 2.1 onward.
// For netstandard2.0 we declare an internal copy so that [NotNull] can be used in source
// without conditional-compilation guards throughout the library.
namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class NotNullAttribute : Attribute { }
}
#endif
