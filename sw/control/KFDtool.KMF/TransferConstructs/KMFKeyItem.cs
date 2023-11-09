using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KFDtool.KMF.TransferConstructs
{
    public class KMFKeyItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        private int _keysetId;
        private int _sln;
        private int _algorithmId;
        private int _keyId;
        public string Key { get; set; }
        public bool UseActiveKeyset { get; set; }
        public bool IsKek { get; set; }
        public int KeysetId
        {
            get
            {
                return _keysetId;
            }
            set
            {
                if (value < 0x00 || value > 0xFF)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _keysetId = value;
            }
        }

        public int Sln
        {
            get
            {
                return _sln;
            }
            set
            {
                if (value < 0x0000 || value > 0xFFFF)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _sln = value;
            }
        }



        public int KeyId
        {
            get
            {
                return _keyId;
            }
            set
            {
                if (value < 0x0000 || value > 0xFFFF)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _keyId = value;
            }
        }

        public int AlgorithmId
        {
            get
            {
                return _algorithmId;
            }
            set
            {
                if (value < 0x00 || value > 0xFF)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _algorithmId = value;
            }
        }

        public KMFKeyItem()
        {
            //
        }
    }
}
