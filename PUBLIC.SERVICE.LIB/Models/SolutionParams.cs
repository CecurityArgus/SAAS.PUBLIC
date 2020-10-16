using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PUBLIC.SERVICE.LIB.Models
{
    /// <summary>
    ///
    /// </summary>
    public class SolutionParams
    {
        /// <summary>
        ///
        /// </summary>
        [JsonProperty("Solution")]
        public Solution Solution { get; set; }

        /// <summary>
        ///
        /// </summary>
        [JsonProperty("Connectors")]
        public List<Connector> Connectors { get; set; }

        /// <summary>
        ///
        /// </summary>
        [JsonProperty("Options")]
        public List<Option> Options { get; set; }

        /// <summary>
        ///
        /// </summary>
        [JsonProperty("Modules")]
        public List<Solution> Modules { get; set; }
    }
}
