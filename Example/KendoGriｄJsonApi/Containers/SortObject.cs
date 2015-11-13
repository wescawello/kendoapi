using Newtonsoft.Json;

namespace KendoGriｄJsonApi.Containers
{
    public class SortObject
    {
        //public SortObject(string field, string direction)
        //{
        //    Field = field;
        //    Direction = direction;
        //}

        [JsonProperty(PropertyName = "field")]
        public string Field { get; set; }
        [JsonProperty(PropertyName = "dir")]
        public string Direction { get; set; }
    }
}
