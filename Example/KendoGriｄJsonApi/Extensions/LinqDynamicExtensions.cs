using System.Collections.Generic;
using System.Linq;

namespace KendoGriｄJsonApi.Extensions
{
    public static class DynamicQueryableExtensions
    {
        public static IEnumerable<TEntity> Select<TEntity>(this IEnumerable<object> source, string propertyName)
        {
            return source.Select(x => GetPropertyValue<TEntity>(x, propertyName));
        }

        private static T GetPropertyValue<T>(object self, string propName)
        {
            var type = self.GetType();
            var propInfo = type.GetProperty(propName);


            try
            {
                return propInfo != null ? (T)propInfo.GetValue(self, null) : default(T);
            }
            catch
            {
                return default(T);
            }
        }
    }
}