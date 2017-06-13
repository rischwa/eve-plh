using Newtonsoft.Json;

namespace PLHLib
{
    public class BaseResult<T>
    {
        public string status
        {
            get { return "success"; }
        }

        public T data { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string message { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string stackTrace { get; set; }
    }
}