// SPDX-License-Identifier: MIT
/*
* KFDtool Container (EKC) Editor
* MIT Open Source. Use is subject to license terms.
* DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
*
*/

namespace KFDEKC.Container.FileStructure.EKC
{
    /// <summary>
    /// 
    /// </summary>
    public class KeyDerivation
    {
        /// <summary>
        /// 
        /// </summary>
        public string DerivationAlgorithm { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string HashAlgorithm { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public byte[] Salt { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int IterationCount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int KeyLength { get; set; }
    }
}
