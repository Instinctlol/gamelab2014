using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestureLib
{
    internal static class ObjectExtensions
    {
        internal static bool ImplementsInterface(this object obj, Type interfaceType)
        {
            Type[] interfaces = obj.GetType().GetInterfaces();
            return (Array.IndexOf(interfaces, interfaceType) != -1);
        }
    }
}
