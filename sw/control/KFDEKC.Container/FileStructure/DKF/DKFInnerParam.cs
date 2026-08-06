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
    public class DKFInnerParam
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("ref")]
        public string Ref { get; set; }

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
        public DKFInnerParam()
        {
            /* stub */
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_ref"></param>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public DKFInnerParam(int _ref, string id, string type, string value)
        {
            this.Ref = _ref.ToString("D12");
            this.ID = id;
            this.Type = type;
            this.Value = value;
        }
    }
}
