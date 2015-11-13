using System.Collections.Generic;
using KendoGriｄJsonApi.Containers;
using Newtonsoft.Json;

namespace KendoGriｄJsonApi
{
    public class KendoGridRequest
    {
        public int? Take { get; set; }
        public int? Skip { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        [JsonProperty(PropertyName = "logic")]
        public string Logic { get; set; }
        [JsonProperty(PropertyName = "filter")]

        public FilterObjectWrapper FilterObjectWrapper { get; set; }

        [JsonProperty(PropertyName = "sort")]
        public IList<SortObject> SortObjects { get; set; }
        [JsonProperty(PropertyName = "group", NullValueHandling = NullValueHandling.Ignore)]
        public IList<GroupObject>  GroupObjects { get; set; }
    }
}
