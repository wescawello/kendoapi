using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using DynamicExpression = System.Linq.Dynamic.DynamicExpression;

namespace KendoGriｄJsonApi.Extensions
{
    public static class Extensions
    {
        public class DataItem
        {
            public string Fieldname { get; set; }
            public string Prefix { get; set; }
            public object Value { get; set; }
        }

        public class GroupResult
        {
            public object Key { get; set; }
            public int Count { get; set; }
            public IEnumerable Items { get; set; }
            public IEnumerable<GroupResult> SubGroups { get; set; }
            public override string ToString() { return string.Format("{0} ({1})", Key, Count); }
        }
        public static IEnumerable<GroupResult> GroupByMany<TElement>(
        this IEnumerable<TElement> elements, params string[] groupSelectors)
        {
            var selectors = new List<Func<TElement, object>>(groupSelectors.Length);
            foreach (var selector in groupSelectors)
            {
                LambdaExpression l =
                    DynamicExpression.ParseLambda(typeof(TElement), typeof(object), selector);
                selectors.Add((Func<TElement, object>)l.Compile());
            }
            return elements.GroupByMany(selectors.ToArray());
        }

        public static IEnumerable<GroupResult> GroupByMany<TElement>(
            this IEnumerable<TElement> elements, params Func<TElement, object>[] groupSelectors)
        {
            if (groupSelectors.Length > 0)
            {
                var selector = groupSelectors.First();
                var nextSelectors = groupSelectors.Skip(1).ToArray(); //reduce the list recursively until zero
                return
                    elements.GroupBy(selector).Select(
                        g => new GroupResult
                        {
                            Key = g.Key,
                            Count = g.Count(),
                            Items = g,
                            SubGroups = g.GroupByMany(nextSelectors)
                        });
            }
            else
                return null;
        }

        public static IQueryable<KendoGroup> GroupByManyko<TElement>(
      this IQueryable<TElement> elements, KendoGridRequest request, params string[] groupSelectors)
        {
            var selectors = new List<Func<TElement, object>>(groupSelectors.Length);
            selectors.AddRange(groupSelectors.Select(selector => DynamicExpression.ParseLambda(typeof (TElement), typeof (object), selector)).Select(l => (Func<TElement, object>) l.Compile()));
            return elements.GroupByManyko(request,groupSelectors, selectors.ToArray());
        }

//        public static IQueryable<KendoGroup> GroupBykdMany<TElement>(this IQueryable<TElement> elements, KendoGridRequest request, params string[] groupSelectors)
//        {
//            if (groupSelectors.Length > 0)
//            {
//                var gFirst= groupSelectors.First();
//                var subgs = groupSelectors.Skip(1).ToArray();

//var hh = elements.GroupBy(gFirst, "*", null);
//                    hh.Select(g =>
//                {

//                    var k = new KendoGroup()
//                    {
//                        field = selectorstr,
//                        value = g.Key,
//                        hasSubgroups = (nextSelectors.Length > 0)
//                    };
//                    if (k.hasSubgroups)
//                    {
//                        k.aggregates = new object();
//                        var gp = g.GroupBykdMany(request, subgs);
//                        k.items = gp.OrderBy(request.GetSortingString() ?? gp.ElementType.FirstSortableProperty());
//                    }
//                    return k;

//                });

//            }
//            else
//            {
//                return null;
//            }


        
//        }


        public static IQueryable<KendoGroup> GroupByManyko<TElement>(
            this IQueryable<TElement> elements, KendoGridRequest request, string[]  groupSelectors, params Func<TElement, object>[] selectors)
        {
            if (selectors.Length > 0)
            {
                var selector = selectors.First();
                var selectorstr = groupSelectors.First();

                groupSelectors = groupSelectors.Skip(1).ToArray();

                var nextSelectors = selectors.Skip(1).ToArray(); //reduce the list recursively until zero
                return
                    elements.GroupBy(selector).Select(
                        g =>
                        {
                            var k = new KendoGroup
                            {
                                field = selectorstr,
                                value = g.Key,
                                hasSubgroups = (nextSelectors.Length >0)
                            };
                            if (k.hasSubgroups)
                            {
                                k.aggregates=new object();
                                var gp = g.AsQueryable().GroupByManyko(request, groupSelectors, nextSelectors);
                                k.items =gp.OrderBy(request.GetSortingString() ?? gp.ElementType.FirstSortableProperty());
                            }
                            else
                            {
                                k.items = g.OrderBy(request.GetSortingString() ?? g.AsQueryable().ElementType.FirstSortableProperty());
                                var d = new Dictionary<string, object>();
                                var vd = new Dictionary<string, object>();
                                request.GroupObjects.First().AggregateObjects.ToList().ForEach(q =>
                                {
                                    switch (q.Aggregate)
                                    {
                                        case "count":
                                            vd.Add(q.Aggregate, g.Count());
                                            break;
                                        case "sum":
                                            vd.Add(q.Aggregate, ((IEnumerable<int>)g.Select(q.Field)).Sum());
                                            break;
                                        case "max":
                                            vd.Add(q.Aggregate, g.OrderBy(q.Field).First().GetPropertyValue(q.Field));
                                            break;
                                        case "min":
                                            vd.Add(q.Aggregate, g.OrderBy(q.Field).Last().GetPropertyValue(q.Field));
                                            break;
                                        default:
                                            break;
                                    }
                                });
                               if (vd.Count > 0) {
                                    d.Add(request.GroupObjects.First().AggregateObjects.First().Field, vd);
                               }
                               k.aggregates = d;
                              
                            }
                            return k;
                        }               
                 ).AsQueryable();
            }
            else
                return null;
        }


        public static object GetPropertyValue(this object self, string propertyName)
        {
            return self.GetPropertyValue<object>(propertyName);
        }

        public static T GetPropertyValue<T>(this object self, string propertyName)
        {
            var type = self.GetType();

            var propInfo = type.GetProperty(propertyName.Split('.').Last()); // In case the propertyName contains a . like Company.Name, take last part.
            try
            {
                return (T)propInfo.GetValue(self, null);
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Combines the property into a list
        /// new(\"First\" as field__First, \"Last\" as field__Last) ==> Dictionary[string, string]
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static IEnumerable<DataItem> GetDataItems(this DynamicClass self, string propertyName)
        {
            var propertyType = self.GetType();
            var propertyInfo = propertyType.GetProperty(propertyName);

            if (propertyInfo == null)
            {
                return new List<DataItem>();
            }

            var property = propertyInfo.GetValue(self, null);
            var props = property.GetType().GetProperties().Where(p => p.Name.Contains("__"));

            return props
                // Split on __ to get the prefix and the field
                .Select(prop => new { PropertyInfo = prop, Data = prop.Name.Split(new[] { "__" }, StringSplitOptions.None) })

                // Return the Fieldname, Prefix and the the value ('First' , 'field' , 'First')
                .Select(x => new DataItem { Fieldname = x.Data.Last(), Prefix = x.Data.First(), Value = x.PropertyInfo.GetValue(property, null) })
            ;
        }

        /// <summary>
        /// Gets the aggregate properties and stores them into a Dictionary object.
        /// Property is defined as : {aggregate}__{field name}  Example : count__Firstname
        /// </summary>
        /// <param name="self">The self.</param>
        /// <returns>Dictionary</returns>
        public static object GetAggregatesAsDictionary(this DynamicClass self)
        {
            var dataItems = self.GetDataItems("Aggregates");

            // Group by the field and return an anonymous dictionary
            return dataItems
                .GroupBy(groupBy => groupBy.Fieldname)
                .ToDictionary(x => x.Key, y => y.ToDictionary(k => k.Prefix, v => v.Value))
            ;
        }


        public static IDictionary<string, object> ToDictionary(this object a)
        {
            var type = a.GetType();
            var props = type.GetProperties();
            return props.ToDictionary(x => x.Name, y => y.GetValue(a, null));
        }




        public static KendoGrid<TModel> Out<TModel>(this KendoGridRequest request, IQueryable<TModel> query)
        {
            return new KendoGrid<TModel>(request, query);
        }

        public static string GetSortingString(this KendoGridRequest request)
        {
            if (request.SortObjects == null) return null;
            var expression = string.Join(",", request.SortObjects.Select(s => s.Field + " " + s.Direction));
            return expression.Length > 1 ? expression : null;
        }

        public static KendoGrid<TModel> Out<TModel>(this KendoGridRequest request, IEnumerable<TModel> list)
        {
            return new KendoGrid<TModel>(request, list.AsQueryable());
        }

    }
}