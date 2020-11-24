using Newtonsoft.Json;

namespace PUBLIC.SERVICE.LIB.Models
{
    /// <summary>
    ///
    /// </summary>
    public partial class Option
    {
        /// <summary>
        ///
        /// </summary>
        [JsonProperty("Name")]
        public string Name { get; set; }

        /// <summary>
        ///
        /// </summary>
        [JsonProperty("Value")]
        public long Value { get; set; }
    }
}
