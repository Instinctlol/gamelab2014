using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestureLib
{
    public static class Utility
    {
        internal static string GenerateNextAvailableName<T>(IEnumerable<T> list, string prefix) where T : INamed
        {
            int i = 0;
            bool nameGenerated = false;

            do
            {
                nameGenerated = list.All(t => t.Name != prefix + i.ToString());

                if(!nameGenerated)
                    i++;
            } while (!nameGenerated);

            return prefix + i.ToString();
        }
    }
}
