using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Azos.Collections
{
    /// <summary>
    /// Checks for reference equality. Use ReferenceEqualityComparer(T).Instance
    /// </summary>
    public sealed class ReferenceEqualityComparer<T> : EqualityComparer<T>
    {
        private static ReferenceEqualityComparer<T> s_Instance = new ReferenceEqualityComparer<T>();

        public static ReferenceEqualityComparer<T> Instance { get { return s_Instance;}}

        private ReferenceEqualityComparer() {}


        public override bool Equals(T x, T y)
        {
            return object.ReferenceEquals(x, y);
        }

        public override int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}

