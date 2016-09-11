using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressiveAnnotations.Infrastructure
{
    using System.Reflection;

    /// <summary>
    /// ReflectionExtensions
    /// </summary>
    public static class ReflectionExtensions
    {

        private static readonly Dictionary<Type, TypeCode> _typeCodeTable =
        new Dictionary<Type, TypeCode>
        {
            { typeof(Boolean), TypeCode.Boolean },
            { typeof(Char), TypeCode.Char },
            { typeof(Byte), TypeCode.Byte },
            { typeof(Int16), TypeCode.Int16 },
            { typeof(Int32), TypeCode.Int32 },
            { typeof(Int64), TypeCode.Int64 },
            { typeof(SByte), TypeCode.SByte },
            { typeof(UInt16), TypeCode.UInt16 },
            { typeof(UInt32), TypeCode.UInt32 },
            { typeof(UInt64), TypeCode.UInt64 },
            { typeof(Single), TypeCode.Single },
            { typeof(Double), TypeCode.Double },
            { typeof(DateTime), TypeCode.DateTime },
            { typeof(Decimal), TypeCode.Decimal },
            { typeof(String), TypeCode.String },
        };


        public static TypeCode GetTypeCode(this Type type)
        {
            if (type == null)
            {
                return TypeCode.Empty;
            }
            TypeCode result;
            if (!_typeCodeTable.TryGetValue(type, out result))
            {
                result = TypeCode.Object;
            }
            return result;
        }
        

        /// <summary>
        /// GetTypes
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetTypes(this Assembly assembly)
        {
            return assembly.DefinedTypes.Select(t => t.AsType());
        }

        /// <summary>
        /// GetEvent
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static EventInfo GetEvent(this Type type, string name)
        {
            return type.GetRuntimeEvent(name);
        }

        /// <summary>
        /// GetInterfaces
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetInterfaces(this Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces;
        }

        /// <summary>
        /// GetInterfaces
        /// </summary>
        /// <param name="type"></param>
        /// <param name="otherType"></param>
        /// <returns></returns>
        public static bool IsAssignableFrom(this Type type, Type otherType)
        {
            return type.GetTypeInfo().IsAssignableFrom(otherType.GetTypeInfo());
        }

        /// <summary>
        /// GetCustomAttributes
        /// </summary>
        /// <param name="type"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static Attribute[] GetCustomAttributes(this Type type, Type attributeType, bool inherit)
        {
            return type.GetTypeInfo().GetCustomAttributes(attributeType, inherit).ToArray();
        }

        /// <summary>
        /// GetConstructors
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<ConstructorInfo> GetConstructors(this Type type)
        {
            return type.GetTypeInfo().DeclaredConstructors.Where(c => c.IsPublic);
        }

        /// <summary>
        /// IsInstanceOfType
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsInstanceOfType(this Type type, object obj)
        {
            return type.IsAssignableFrom(obj.GetType());
        }

        /// <summary>
        /// GetAddMethod
        /// </summary>
        /// <param name="eventInfo"></param>
        /// <param name="nonPublic"></param>
        /// <returns></returns>
        public static MethodInfo GetAddMethod(this EventInfo eventInfo, bool nonPublic = false)
        {
            if (eventInfo.AddMethod == null || (!nonPublic && !eventInfo.AddMethod.IsPublic))
            {
                return null;
            }

            return eventInfo.AddMethod;
        }

        /// <summary>
        /// GetRemoveMethod
        /// </summary>
        /// <param name="eventInfo"></param>
        /// <param name="nonPublic"></param>
        /// <returns></returns>
        public static MethodInfo GetRemoveMethod(this EventInfo eventInfo, bool nonPublic = false)
        {
            if (eventInfo.RemoveMethod == null || (!nonPublic && !eventInfo.RemoveMethod.IsPublic))
            {
                return null;
            }

            return eventInfo.RemoveMethod;
        }

        /// <summary>
        /// GetGetMethod
        /// </summary>
        /// <param name="property"></param>
        /// <param name="nonPublic"></param>
        /// <returns></returns>
        public static MethodInfo GetGetMethod(this PropertyInfo property, bool nonPublic = false)
        {
            if (property.GetMethod == null || (!nonPublic && !property.GetMethod.IsPublic))
            {
                return null;
            }

            return property.GetMethod;
        }

        /// <summary>
        /// GetSetMethod
        /// </summary>
        /// <param name="property"></param>
        /// <param name="nonPublic"></param>
        /// <returns></returns>
        public static MethodInfo GetSetMethod(this PropertyInfo property, bool nonPublic = false)
        {
            if (property.SetMethod == null || (!nonPublic && !property.SetMethod.IsPublic))
            {
                return null;
            }

            return property.SetMethod;
        }

        /// <summary>
        /// GetProperties
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetProperties(this Type type)
        {
            return GetProperties(type, BindingFlags.Public); //BindingFlags.FlattenHierarchy |
        }

        /// <summary>
        /// GetProperties
        /// </summary>
        /// <param name="type"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetProperties(this Type type, BindingFlags flags)
        {
            var properties = type.GetTypeInfo().DeclaredProperties;
            if ((flags & BindingFlags.FlattenHierarchy) == BindingFlags.FlattenHierarchy)
            {
                properties = type.GetRuntimeProperties();
            }

            return from property in properties
                   let getMethod = property.GetMethod
                   where getMethod != null
                   where (flags & BindingFlags.Public) != BindingFlags.Public || getMethod.IsPublic
                   where (flags & BindingFlags.Instance) != BindingFlags.Instance || !getMethod.IsStatic
                   where (flags & BindingFlags.Static) != BindingFlags.Static || getMethod.IsStatic
                   select property;
        }



        /// <summary>
        /// GetProperty
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static PropertyInfo GetProperty(this Type type, string name, BindingFlags flags)
        {
            return GetProperties(type, flags).FirstOrDefault(p => p.Name == name);
        }

        /// <summary>
        /// GetProperty
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static PropertyInfo GetProperty(this Type type, string name)
        {
            return GetProperties(type, BindingFlags.Public | BindingFlags.FlattenHierarchy).FirstOrDefault(p => p.Name == name);
        }

        /// <summary>
        /// GetMethods
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetMethods(this Type type)
        {
            return GetMethods(type, BindingFlags.FlattenHierarchy | BindingFlags.Public);
        }

        /// <summary>
        /// GetMethods
        /// </summary>
        /// <param name="type"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetMethods(this Type type, BindingFlags flags)
        {
            var properties = type.GetTypeInfo().DeclaredMethods;
            if ((flags & BindingFlags.FlattenHierarchy) == BindingFlags.FlattenHierarchy)
            {
                properties = type.GetRuntimeMethods();
            }

            return properties
                .Where(m => (flags & BindingFlags.Public) != BindingFlags.Public || m.IsPublic)
                .Where(m => (flags & BindingFlags.Instance) != BindingFlags.Instance || !m.IsStatic)
                .Where(m => (flags & BindingFlags.Static) != BindingFlags.Static || m.IsStatic);
        }

        /// <summary>
        /// GetMethods
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static MethodInfo GetMethod(this Type type, string name, BindingFlags flags)
        {
            return GetMethods(type, flags).FirstOrDefault(m => m.Name == name);
        }

        /// <summary>
        /// GetMethod
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static MethodInfo GetMethod(this Type type, string name)
        {
            return GetMethods(type, BindingFlags.Public | BindingFlags.FlattenHierarchy)
                   .FirstOrDefault(m => m.Name == name);
        }

        /// <summary>
        /// GetConstructors
        /// </summary>
        /// <param name="type"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static IEnumerable<ConstructorInfo> GetConstructors(this Type type, BindingFlags flags)
        {
            return type.GetConstructors()
                .Where(m => (flags & BindingFlags.Public) != BindingFlags.Public || m.IsPublic)
                .Where(m => (flags & BindingFlags.Instance) != BindingFlags.Instance || !m.IsStatic)
                .Where(m => (flags & BindingFlags.Static) != BindingFlags.Static || m.IsStatic);
        }

        /// <summary>
        /// GetFields
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<FieldInfo> GetFields(this Type type)
        {
            return GetFields(type, BindingFlags.Public | BindingFlags.FlattenHierarchy);
        }

        /// <summary>
        /// GetFields
        /// </summary>
        /// <param name="type"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static IEnumerable<FieldInfo> GetFields(this Type type, BindingFlags flags)
        {
            var fields = type.GetTypeInfo().DeclaredFields;
            if ((flags & BindingFlags.FlattenHierarchy) == BindingFlags.FlattenHierarchy)
            {
                fields = type.GetRuntimeFields();
            }

            return fields
                .Where(f => (flags & BindingFlags.Public) != BindingFlags.Public || f.IsPublic)
                .Where(f => (flags & BindingFlags.Instance) != BindingFlags.Instance || !f.IsStatic)
                .Where(f => (flags & BindingFlags.Static) != BindingFlags.Static || f.IsStatic);
        }

        /// <summary>
        /// GetField
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static FieldInfo GetField(this Type type, string name, BindingFlags flags)
        {
            return GetFields(type, flags).FirstOrDefault(p => p.Name == name);
        }

        /// <summary>
        /// GetField
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FieldInfo GetField(this Type type, string name)
        {
            return GetFields(type, BindingFlags.Public | BindingFlags.FlattenHierarchy).FirstOrDefault(p => p.Name == name);
        }

        /// <summary>
        /// GetGenericArguments
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type[] GetGenericArguments(this Type type)
        {
            return type.GenericTypeArguments;
        }

        /// <summary>
        /// GetEntityFieldValue
        /// </summary>
        /// <param name="entityObj"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static object GetEntityFieldValue(this object entityObj, string propertyName)
        {
            var pro = entityObj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).First(x => x.Name == propertyName);
            return pro.GetValue(entityObj, null);

        }
        
    }

    public enum TypeCode
    {
        Empty = 0,
        Object = 1,
        DBNull = 2,
        Boolean = 3,
        Char = 4,
        SByte = 5,
        Byte = 6,
        Int16 = 7,
        UInt16 = 8,
        Int32 = 9,
        UInt32 = 10,
        Int64 = 11,
        UInt64 = 12,
        Single = 13,
        Double = 14,
        Decimal = 15,
        DateTime = 16,
        String = 18,
    }

    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum BindingFlags
    {
        //None = 0,
        //Instance = 1,
        //Public = 2,
        //Static = 4,
        //FlattenHierarchy = 8,
        //SetProperty = 8192

        /// <summary/>
        Default = 0,
        /// <summary/>
        IgnoreCase = 1,
        /// <summary/>
        DeclaredOnly = 2,
        /// <summary/>
        Instance = 4,
        /// <summary/>
        Static = 8,
        /// <summary/>
        Public = 16,
        /// <summary/>
        NonPublic = 32,
        /// <summary/>
        FlattenHierarchy = 64,
        /// <summary/>
        InvokeMethod = 256,
        /// <summary/>
        CreateInstance = 512,
        /// <summary/>
        GetField = 1024,
        /// <summary/>
        SetField = 2048,
        /// <summary/>
        GetProperty = 4096,
        /// <summary/>
        SetProperty = 8192,
        /// <summary/>
        PutDispProperty = 16384,
        /// <summary/>
        PutRefDispProperty = 32768,
        /// <summary/>
        ExactBinding = 65536,
        /// <summary/>
        SuppressChangeType = 131072,
        /// <summary/>
        OptionalParamBinding = 262144,
        /// <summary/>
        IgnoreReturn = 16777216,
    }
}
