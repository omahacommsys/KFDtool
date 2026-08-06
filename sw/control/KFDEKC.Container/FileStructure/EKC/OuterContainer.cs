// SPDX-License-Identifier: MIT
/*
* KFDtool Container (EKC) Editor
* MIT Open Source. Use is subject to license terms.
* DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
*
*/

using System.Xml.Serialization;

namespace KFDEKC.Container.FileStructure.EKC
{
    /// <summary>
    /// 
    /// </summary>
    public class OuterContainer
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("version")]
        public string Version { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public KeyDerivation KeyDerivation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string EncryptedDataPlaceholder { get; set; }
    }
}
