using System.Xml.Serialization;

namespace KFDtool.Container
{
    [XmlRoot("param")]
    public class DKFInnerParam
    {
        [XmlAttribute("ref")]
        public string Ref { get; set; }

        [XmlAttribute("id")]
        public string ID { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }

        public DKFInnerParam()
        {
            /* stub */
        }

        public DKFInnerParam(int _ref, string id, string type, string value)
        {
            this.Ref = _ref.ToString("D12");
            this.ID = id;
            this.Type = type;
            this.Value = value;
        }
    }
}
