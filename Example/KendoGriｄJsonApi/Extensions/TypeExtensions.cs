﻿using System;
using System.ComponentModel;
using System.Linq;

namespace KendoGriｄJsonApi.Extensions
{
    public static class TypeExtensions
    {
        internal static readonly Type[] PredefinedTypes =
        {
            typeof(object),
            typeof(bool),
            typeof(char),
            typeof(string),
            typeof(sbyte),
            typeof(byte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(Guid),
            typeof(Math),
            typeof(Convert)
        };

        public static bool IsPredefinedType(this Type type)
        {
            return PredefinedTypes.Any(left => left == type);
        }

        public static string FirstSortableProperty(this Type type)
        {
            var propertyInfo = type.GetProperties().FirstOrDefault(property => property.PropertyType.IsPredefinedType());
            
            if (propertyInfo == null)
            {
                throw new NotSupportedException("CannotFindPropertyToSortBy");
            }

            return propertyInfo.Name;
        }

        public static object ChangeType(object value, Type conversionType)
        {
            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (value == null)
                {
                    return null;
                }

                var nullableConverter = new NullableConverter(conversionType);
                conversionType = nullableConverter.UnderlyingType;
            }

            return Convert.ChangeType(value, conversionType);
        }
    }
}