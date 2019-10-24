// enable unit tests for internal methods
using System.Runtime.CompilerServices;

#if DEBUG
[assembly: InternalsVisibleTo("Inpainting.UnitTests")]
#endif
