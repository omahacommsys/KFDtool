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
    [XmlRoot("param")]
    public class DKFOuterParam
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("id")]
        public string ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("type")]
        public string Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("value")]
        public string Value { get; set; }

        /*
        ** Methods
        */

        /// <summary>
        /// 
        /// </summary>
        public DKFOuterParam()
        {
            /* stub */
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public DKFOuterParam(string id, string type, string value)
        {
            this.ID = id;
            this.Type = type;
            this.Value = value;
        }
    }
}
