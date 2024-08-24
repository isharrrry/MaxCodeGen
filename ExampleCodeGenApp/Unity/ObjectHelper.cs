using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common
{
    public static class ObjectHelper
    {
        #region Type
        public static List<Type> GetSubClassTypesByType(this Assembly assembly, Type parentType)
        {
            return assembly.GetTypes()
                .Where(type => parentType.IsAssignableFrom(type) && !type.IsInterface)
                .ToList();
        }
        #endregion
    }
}
