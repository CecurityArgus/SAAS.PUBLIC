using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PUBLIC.SERVICE.LIB.Models
{
    /// <summary>
    ///
    /// </summary>
    public class Solution
    {
        /// <summary>
        ///
        /// </summary>
        [JsonProperty("Name")]
        public string Name { get; set; }

        /// <summary>
        ///
        /// </summary>
        [JsonProperty("Version")]
        public long Version { get; set; }

        /// <summary>
        ///
        /// </summary>
        [JsonProperty("Params")]
        public string Params { get; set; }
    }
}
