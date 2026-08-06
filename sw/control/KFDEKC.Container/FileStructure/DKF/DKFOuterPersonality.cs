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
    [XmlRoot("personality")]
    public class DKFOuterPersonality
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("pdlversion")]
        public string PDLVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("defaultversion")]
        public string DefaultVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("valueversion")]
        public string ValueVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("devicetype")]
        public string DeviceType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("targetversionmajor")]
        public string TargetVersionMajor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("targetversionminor")]
        public string TargetVersionMinor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("type")]
        public string Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("percategory")]
        public string PerCategory { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ParamData { get; set; }

        /*
        ** Methods
        */

        /// <summary>
        /// 
        /// </summary>
        public DKFOuterPersonality()
        {
            PDLVersion = "2";
            DefaultVersion = "1";
            ValueVersion = "1";
            DeviceType = "KeyManager";
            TargetVersionMajor = "0";
            TargetVersionMinor = "0";
            Type = "User";
            PerCategory = "Encrypted";
        }
    }
}
