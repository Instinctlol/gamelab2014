using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectEntities
{
    class StringFormatter
    {
        /// <summary>
        /// Ersetzt Umlaute korrekt
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static String cleanUmlaute(String s)
        {
            byte[] b1 = Encoding.UTF8.GetBytes(s);
            return Encoding.UTF8.GetString(b1);
            //return (new StringBuilder(s))
            //    .Replace("ÃŸ", "ß")
            //    .Replace("Ãœ", "Ü").Replace("Ã„", "Ä").Replace("Ã–", "Ö")
            //    .Replace("Ã¼", "ü").Replace("Ã¤", "ä").Replace("Ã¶", "ö")
            //    .ToString().ToLower();
        }
    }
}
