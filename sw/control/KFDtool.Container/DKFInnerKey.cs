using System.Xml.Serialization;

namespace KFDtool.Container
{
    [XmlRoot("key")]
    public class DKFInnerKey
    {
        [XmlAttribute("alg")]
        public string Algorithm { get; set; }

        [XmlAttribute("plat")]
        public string Platform { get; set; }

        [XmlAttribute("id")]
        public string ID { get; set; }

        [XmlAttribute("ksid")]
        public string KSID { get; set; }

        [XmlAttribute("ksname")]
        public string KSName { get; set; }

        [XmlAttribute("sln")]
        public string SLN { get; set; }

        [XmlAttribute("ekid")]
        public string EKID { get; set; }

        [XmlAttribute("ekalg")]
        public string EKAlgorithm { get; set; }

        [XmlAttribute("keytype")]
        public string KeyType { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }

        [XmlAttribute("interop")]
        public string Interop { get; set; }

        public DKFInnerKey()
        {
            /* stub */
        }

        public DKFInnerKey(string alg, string id, string sln, string value)
        {
            this.Algorithm = alg;
            this.ID = id;
            this.SLN = sln;
            this.Value = value;

            // default remaining
            this.Platform = "P25";
            this.KSID = "1";
            this.KSName = "Set001";
            this.EKID = "0";
            this.EKAlgorithm = "Unencrypted";
            this.KeyType = "Tek";
            this.Interop = "False";
        }
    }
}
