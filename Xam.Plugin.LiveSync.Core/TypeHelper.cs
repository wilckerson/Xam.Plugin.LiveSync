using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Xam.Plugin.LiveSync
{
    public static class TypeHelper
    {
        public static FieldInfo GetField(Type type, string fieldName)
        {
            var fieldInfo = type.GetRuntimeField(fieldName);
            if(fieldInfo == null)
            {
                var baseType = type.GetTypeInfo().BaseType;
                if(baseType == null){ return null; }

                var baseField = GetField(baseType, fieldName);
                return baseField;
            }
            else
            {
                return fieldInfo;
            }
        }
    }
}
