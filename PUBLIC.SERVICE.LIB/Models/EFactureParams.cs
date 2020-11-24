using System;
using System.Collections.Generic;
using System.Text;

namespace PUBLIC.SERVICE.LIB.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class EFactureParams
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
        public List<EFactureInputFormat> AllowedInputFormats { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string EntiteFacturable { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string NombreEmployes { get; set; }
    }

    /// <summary>
    /// Allowed input formats for EFacture
    /// </summary>
    public enum EFactureInputFormat
    {
        /// <summary>
        /// PDF files
        /// </summary>
        PDF = 200001,
        /// <summary>
        /// PDF files with accompanying CSV files
        /// </summary>
        PDFCSV = 200002
    }
}
