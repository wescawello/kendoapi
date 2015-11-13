using System.Collections.Generic;
using Newtonsoft.Json;

namespace KendoGriｄJsonApi.Containers
{
    public class FilterObject
    {
        public string Field { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
        [JsonProperty(PropertyName = "filters")]
        public IList<FilterObject> Filters { get; set; }
        public string Logic { get; set; }

        public bool IsConjugate
        {
            get { return (Filters != null &&  (Filters.Count == 2)); }
        }

        public string LogicToken
        {
            get
            {
                switch (Logic)
                {
                    case "and":
                        return "&&";
                    case "or":
                        return "||";
                    default:
                        return null;
                }
            }
        }
    }
}
