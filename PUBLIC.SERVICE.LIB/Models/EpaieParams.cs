using System;
using System.Collections.Generic;
using System.Text;

namespace PUBLIC.SERVICE.LIB.Models
{
    /// <summary>
    ///
    /// </summary>
    public class EPaieParams
    {
        /// <summary>
        ///
        /// </summary>
        public string Entreprise { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string SIRET { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string SIREN { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string Logiciel { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string CodeClient { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string CodeAgence { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string CodeClientInterne { get; set; }

        /// <summary>
        ///
        /// </summary>
        public List<EPaieInputFormat> AllowedInputFormats { get; set; }
    }

    /// <summary>
    /// Allowed epaie input formats
    /// </summary>
    public enum EPaieInputFormat
    {
        /// <summary>
        /// PDF files
        /// </summary>
        PDF = 100001,

        /// <summary>
        /// PDF files with accompanying CSV files
        /// </summary>
        PDFCSV = 100002,

        /// <summary>
        /// PDF/A3 files with embedded CSV file
        /// </summary>
        PDFA3CSV = 100003
    }
}
