using System;
using System.Collections.Generic;
using System.Linq;

namespace GuitarConfiguratorSharp.Utils
{
    public static class Extension
    {
        public static IEnumerable<O> FilterCast<T,O>(this IEnumerable<T> o) where T : class where O : class
            => o.Select(x => x as O).Where(x => x != null)!;
    }
}