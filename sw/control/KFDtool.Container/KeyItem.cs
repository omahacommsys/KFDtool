using System.ComponentModel;

namespace KFDtool.Container
{
    public class KeyItem : INotifyPropertyChanged
    {
        public int Id { get; set; }

        private string _name;

        public string Name
        {
            get { return _name; }

            set
            {
                if (_name != value)
                {
                    _name = value;

                    NotifyPropertyChanged("Name");
                }
            }
        }

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

        public bool ActiveKeyset { get; set; }

        public int KeysetId { get; set; }

        public int Sln { get; set; }

        public bool KeyTypeAuto { get; set; }

        public bool KeyTypeTek { get; set; }

        public bool KeyTypeKek { get; set; }

        public int KeyId { get; set; }

        private int _algorithmId { get; set; }
        public int AlgorithmId 
        { 
            get
            {
                return _algorithmId;
            }
            set
            {
                if (_algorithmId != value)
                {
                    _algorithmId = value;
                }
                NotifyPropertyChanged("AlgoImage");
                NotifyPropertyChanged("AlgorithmId");
            }
        }

        public string Key { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }
}
