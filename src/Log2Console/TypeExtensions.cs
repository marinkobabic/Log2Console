using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log2Console
{
    public static class TypeExtensions
    { 
        public static string GetTypeDescription(this Type type)
        {
            var attr = (DisplayNameAttribute)Attribute.GetCustomAttribute(type, typeof(DisplayNameAttribute), true);
            return attr != null ? attr.DisplayName : type.ToString();
        }
    }
}
