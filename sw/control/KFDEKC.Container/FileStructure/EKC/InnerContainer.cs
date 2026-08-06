// SPDX-License-Identifier: MIT
/*
* KFDtool Container (EKC) Editor
* MIT Open Source. Use is subject to license terms.
* DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
*
*/

using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace KFDEKC.Container.FileStructure.EKC
{
    /// <summary>
    /// 
    /// </summary>
    public class InnerContainer
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("version")]
        public string Version { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<KeyItem> Keys { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int NextKeyNumber { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<GroupItem> Groups { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int NextGroupNumber { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<RSIKeyItem> UKEKs { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int NextUKEKNumber { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<RSIKeyItem> LLAs { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int NextLLANumber { get; set; }
    }
}
