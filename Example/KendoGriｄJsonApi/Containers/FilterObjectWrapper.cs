using System.Collections.Generic;
using Newtonsoft.Json;

namespace KendoGriｄJsonApi.Containers
{
    public class FilterObjectWrapper
    {
        public FilterObjectWrapper(string logic, IList<FilterObject> filterObjects)
        {
            Logic = logic;
            FilterObjects = filterObjects;
        }

        public string Logic { get; set; }

        [JsonProperty(PropertyName = "filters")]
        public IList<FilterObject> FilterObjects { get; set; }
        public string LogicToken
        {
            get
            {
                switch(Logic)
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
