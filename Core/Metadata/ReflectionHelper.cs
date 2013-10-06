using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Adglopez.ServiceDocumenter.Core.Metadata
{
    public static class ReflectionHelper
    {
        public static readonly Type[] List;

        static ReflectionHelper()
        {
            var types = new[]
                          {
                              typeof (Enum),
                              typeof (String),
                              typeof (Char),
                              typeof (Boolean),
                              typeof (Byte),
                              typeof (Int16),
                              typeof (Int32),
                              typeof (Int64),
                              typeof (Single),
                              typeof (Double),
                              typeof (Decimal),
                              typeof (SByte),
                              typeof (UInt16),
                              typeof (UInt32),
                              typeof (UInt64),
                              typeof (DateTime),
                              typeof (DateTimeOffset),
                              typeof (TimeSpan),
                              typeof(Guid)
                          };
            var nullTypes = from t in types
                            where t.IsValueType
                            select typeof(Nullable<>).MakeGenericType(t);

            List = types.Concat(nullTypes).ToArray();
        }

        public static bool IsSimpleType(Type type)
        {
            if (List.Any(x => x.IsAssignableFrom(type)))
                return true;

            var nut = Nullable.GetUnderlyingType(type);
            return nut != null && nut.IsEnum;
        }

        public static bool IsCollection(Type property)
        {
            return property.GetInterface(typeof(IEnumerable<>).FullName) != null;
        }

        public static bool IsCollection(PropertyInfo property)
        {
            return property.PropertyType.GetInterface(typeof(IEnumerable<>).FullName) != null;
        }

        public static bool IsCollection(ParameterInfo property)
        {
            return property.ParameterType.GetInterface(typeof(IEnumerable<>).FullName) != null;
        }

        public static bool IsServiceOperation(MethodInfo op)
        {
            var name = op.Name;

            var ignoredMethods = new[]
                                 {
                                     "get_ChannelFactory",
                                     "get_ClientCredentials",
                                     "get_State",
                                     "get_InnerChannel",
                                     "get_Endpoint",
                                     "Open",
                                     "Abort",
                                     "Close",
                                     "DisplayInitializationUI",
                                     "ToString",
                                     "Equals",
                                     "GetHashCode",
                                     "GetType"
                                 };
            return !ignoredMethods.Contains(name);
        }

        public static string GetFriendlyTypeName(Type type)
        {
            string friendlyName;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments()[0];
                friendlyName = type.Name + "?";
            }
            else
            {
                friendlyName = type.Name;    
            }
            
            return friendlyName;
        }
    }
}
