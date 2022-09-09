using System.Collections.Generic;
using System.Linq;

namespace GuitarConfiguratorSharp.NetCore.Utils
{
    public static class Extension
    {
        public static IEnumerable<TO> FilterCast<T,TO>(this IEnumerable<T> o) where T : class where TO : class
            => o.Select(x => x as TO).Where(x => x != null)!;
    }
}