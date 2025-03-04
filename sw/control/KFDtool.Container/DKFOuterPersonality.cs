using System.Xml.Serialization;

namespace KFDtool.Container
{
    [XmlRoot("personality")]
    public class DKFOuterPersonality
    {
        [XmlAttribute("pdlversion")]
        public string PDLVersion { get; set; }

        [XmlAttribute("defaultversion")]
        public string DefaultVersion { get; set; }

        [XmlAttribute("valueversion")]
        public string ValueVersion { get; set; }

        [XmlAttribute("devicetype")]
        public string DeviceType { get; set; }

        [XmlAttribute("targetversionmajor")]
        public string TargetVersionMajor { get; set; }

        [XmlAttribute("targetversionminor")]
        public string TargetVersionMinor { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("percategory")]
        public string PerCategory { get; set; }

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

        public string ParamData { get; set; }
    }
}
