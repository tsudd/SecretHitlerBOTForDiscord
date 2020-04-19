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
        public static void Shuffle<T>(T[] a, int L, int R)
        {
            var rnd = new Random();
            int n = R - L + 1;
            while (n > 1)
            {
                int k = L + rnd.Next(n--);
                T tmp = a[L + n];
                a[L + n] = a[k];
                a[k] = tmp;
            }
        }
        public static void Shuffle<T>(T[] a)
        {
            Shuffle(a, 0, a.Length - 1);
        }
    }
}
