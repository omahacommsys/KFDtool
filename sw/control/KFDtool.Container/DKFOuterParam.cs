using System.Xml.Serialization;

namespace KFDtool.Container
{
    [XmlRoot("param")]
    public class DKFOuterParam
    {
        [XmlAttribute("id")]
        public string ID { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }

        public DKFOuterParam()
        {
            /* stub */
        }

        public DKFOuterParam(string id, string type, string value)
        {
            this.ID = id;
            this.Type = type;
            this.Value = value;
        }
    }
}
