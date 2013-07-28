using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RJ.RuntimePocoGenerator.Extensions
{
    public static class StringExtensions
    {
        public static string UppercaseFirst(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            char[] c = text.ToCharArray();
            c[0] = char.ToUpper(c[0]);
            return new string(c);
        }
    }
}
