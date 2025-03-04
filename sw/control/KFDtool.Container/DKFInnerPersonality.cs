using System.Xml.Serialization;

namespace KFDtool.Container
{
    [XmlRoot("personality")]
    public class DKFInnerPersonality
    {
        [XmlAttribute("definition")]
        public string Definition { get; set; }

        [XmlAttribute("device")]
        public string Device { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("family")]
        public string Family { get; set; }

        public string ValueData { get; set; }
    }
}
