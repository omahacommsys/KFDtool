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
    public class DKFInnerPersonality
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("definition")]
        public string Definition { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("device")]
        public string Device { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("family")]
        public string Family { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ValueData { get; set; }
    }
}
