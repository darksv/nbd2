using System.Collections.Generic;

namespace NBD2.Util
{
    class EnumerableExt
    {
        public static IEnumerable<T> FromSingle<T>(T item)
        {
            yield return item;
        }
    }
}
