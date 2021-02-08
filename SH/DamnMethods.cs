using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.SH
{
    public static class DamnMethods
    {
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
        public static void Shuffle<T>(T[] a, int L, int R) // Shuffle a [ L .. R - 1 ]
        {
            var rnd = new Random();
            int n = R - L;
            while (n > 1)
            {
                int k = L + rnd.Next(n--);
                Swap(ref a[L + n], ref a[k]);
            }
        }
        public static void Shuffle<T>(T[] a)
        {
            Shuffle(a, 0, a.Length);
        }
    }
}
