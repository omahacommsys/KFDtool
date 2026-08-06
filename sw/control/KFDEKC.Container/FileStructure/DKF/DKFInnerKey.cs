// SPDX-License-Identifier: MIT
/*
* KFDtool Container (EKC) Editor
* MIT Open Source. Use is subject to license terms.
* DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
*
*/

using System.Xml.Serialization;

namespace KFDEKC.Container.FileStructure.DKF
{
    /// <summary>
    /// 
    /// </summary>
    [XmlRoot("key")]
    public class DKFInnerKey
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("alg")]
        public string Algorithm { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("plat")]
        public string Platform { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("id")]
        public string ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("ksid")]
        public string KSID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("ksname")]
        public string KSName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("sln")]
        public string SLN { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("ekid")]
        public string EKID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("ekalg")]
        public string EKAlgorithm { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("keytype")]
        public string KeyType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("value")]
        public string Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("interop")]
        public string Interop { get; set; }

        /*
        ** Methods
        */

        /// <summary>
        /// 
        /// </summary>
        public DKFInnerKey()
        {
            /* stub */
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alg"></param>
        /// <param name="id"></param>
        /// <param name="sln"></param>
        /// <param name="value"></param>
        public DKFInnerKey(string alg, string id, string sln, string value)
        {
            this.Algorithm = alg;
            this.ID = id;
            this.SLN = sln;
            this.Value = value;

            // default remaining
            this.Platform = "P25";
            this.KSID = "1";
            this.KSName = "Set001";
            this.EKID = "0";
            this.EKAlgorithm = "Unencrypted";
            this.KeyType = "Tek";
            this.Interop = "False";
        }
    }
}
