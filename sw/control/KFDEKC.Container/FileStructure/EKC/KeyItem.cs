// SPDX-License-Identifier: MIT
/*
* KFDtool Container (EKC) Editor
* MIT Open Source. Use is subject to license terms.
* DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
*
*/

using System.ComponentModel;

namespace KFDEKC.Container.FileStructure.EKC
{
    /// <summary>
    /// 
    /// </summary>
    public class KeyItem : INotifyPropertyChanged
    {
        private string name;
        private int algorithmId;

        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string AlgoImage
        {
            get
            {
                switch (this.AlgorithmId)
                {
                    // DES-OFB & DES-XL
                    case 0x81:
                    case 0x9F:
                        return "/Images/Algos/DES.png";
                    // AES
                    case 0x84:
                    case 0x85:
                        return "/Images/Algos/AES.png";
                    // ARC4
                    case 0xAA:
                        return "/Images/Algos/ARC4.png";
                    // Return TBD for all non-handled algos
                    default:
                        return "/Images/Algos/OTHER.png";
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ActiveKeyset { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int KeysetId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Sln { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool KeyTypeAuto { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool KeyTypeTek { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool KeyTypeKek { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int KeyId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int AlgorithmId 
        { 
            get { return algorithmId; }
            set
            {
                if (algorithmId != value)
                    algorithmId = value;
                NotifyPropertyChanged("AlgoImage");
                NotifyPropertyChanged("AlgorithmId");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Key { get; set; }

        /*
        ** Events
        */

        /// <summary>
        /// 
        /// </summary>

        public event PropertyChangedEventHandler PropertyChanged;

        /*
        ** Methods
        */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propName"></param>
        public void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name.ToString();
        }
    }
}
