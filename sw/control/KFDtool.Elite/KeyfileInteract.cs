using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace KFDtool.Elite
{
    public class KeyfileInteract
    {
        public static bool Ensure7zExists()
        {
            return (File.Exists(@"C:\Program Files\7-Zip\7z.exe"));
        }
        public static void GenerateKeyfile(string xmlPath, string outputPath, string password)
        {
            if (!Ensure7zExists())
            {
                throw new Exception("7-zip does not exist.");
            }
            string strCmdText = $"/C 7z a {outputPath} -p\"{password}\" {xmlPath}";
            //Console.WriteLine(strCmdText);
            System.Diagnostics.Process.Start("CMD.exe", strCmdText);
        }
        public static string SerializeKeys(List<KeyEntry> keys)
        {
            XmlDocument doc = new XmlDocument();

            //xml declaration
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            // file container
            XmlElement kmf_file = doc.CreateElement(string.Empty, "ikmf-app-file", string.Empty);
            doc.AppendChild(kmf_file);

            XmlElement keyMsg = doc.CreateElement(string.Empty, "ikmf-app-msg", string.Empty);

            XmlNode keyHeaderNode = keyMsg.OwnerDocument.ImportNode(buildHeader(false), true);
            keyMsg.AppendChild(keyHeaderNode);

            /*
             * BEGIN KEYS
             */
            foreach (KeyEntry key in keys)
            {
                XmlElement content = doc.CreateElement(string.Empty, "ikmf-content", string.Empty);
                XmlElement dissemination = doc.CreateElement(string.Empty, "Key-Dissemination", string.Empty);

                // key name
                XmlElement key_name = doc.CreateElement(string.Empty, "key-name", string.Empty);
                XmlElement key_name_source = doc.CreateElement(string.Empty, "key-name-source", string.Empty);
                XmlText key_name_sourceText = doc.CreateTextNode("KeyFileGen");
                key_name_source.AppendChild(key_name_sourceText);
                key_name.AppendChild(key_name_source);

                XmlElement key_name_key = doc.CreateElement(string.Empty, "key-name-key", string.Empty);
                XmlText key_name_keyText = doc.CreateTextNode(key.KeyName);
                key_name_key.AppendChild(key_name_keyText);
                key_name.AppendChild(key_name_key);

                dissemination.AppendChild(key_name);

                // key class
                XmlElement key_class = doc.CreateElement(string.Empty, "key-class", string.Empty);
                XmlText key_classText = doc.CreateTextNode("TEK");
                key_class.AppendChild(key_classText);
                dissemination.AppendChild(key_class);

                // key ID
                XmlElement key_id = doc.CreateElement(string.Empty, "key-ID", string.Empty);
                XmlText key_idText = doc.CreateTextNode(key.keyId.ToString("x4"));
                key_id.AppendChild(key_idText);
                dissemination.AppendChild(key_id);

                // algo
                XmlElement algo = doc.CreateElement(string.Empty, "algorithm", string.Empty);
                XmlElement algo_specific = doc.CreateElement(string.Empty, "specific-algorithm", string.Empty);

                string algoText = "";

                switch (key.algId)
                {
                    case 0x81:
                        algoText = "DES";
                        break;
                    case 0x84:
                        algoText = "AES256";
                        break;
                    case 0xAA:
                        algoText = "ADP";
                        break;
                    default:
                        throw new Exception("Unknown algorithm ID");
                }

                XmlText algo_specificText = doc.CreateTextNode(algoText);
                algo_specific.AppendChild(algo_specificText);
                algo.AppendChild(algo_specific);

                XmlElement algo_algid = doc.CreateElement(string.Empty, "algorithm-id", string.Empty);
                XmlText algo_algidText = doc.CreateTextNode(key.algId.ToString("x2"));
                algo_algid.AppendChild(algo_algidText);
                algo.AppendChild(algo_algid);

                dissemination.AppendChild(algo);

                // key length
                XmlElement key_len = doc.CreateElement(string.Empty, "key-length", string.Empty);

                string keylenText = "";

                switch (key.algId)
                {
                    case 0x81:
                        keylenText = "8";
                        break;
                    case 0x84:
                        keylenText = "32";
                        break;
                    case 0xAA:
                        keylenText = "5";
                        break;
                    default:
                        throw new Exception("Unknown algorithm ID");
                }

                XmlText key_lenText = doc.CreateTextNode(keylenText);
                key_len.AppendChild(key_lenText);
                dissemination.AppendChild(key_len);

                // key data
                XmlElement keyData = doc.CreateElement(string.Empty, "key", string.Empty);
                XmlText keyDataText = doc.CreateTextNode(key.keyData);
                keyData.AppendChild(keyDataText);
                dissemination.AppendChild(keyData);

                // ckr
                XmlElement ckr = doc.CreateElement(string.Empty, "ckr-ID", string.Empty);
                XmlText ckrText = doc.CreateTextNode(key.ckrId.ToString());
                ckr.AppendChild(ckrText);
                dissemination.AppendChild(ckr);

                // keyset ID
                XmlElement kset = doc.CreateElement(string.Empty, "keyset-ID", string.Empty);
                XmlText ksetText = doc.CreateTextNode(key.ksetId.ToString());
                kset.AppendChild(ksetText);
                dissemination.AppendChild(kset);

                content.AppendChild(dissemination);
                keyMsg.AppendChild(content);

            }

            kmf_file.AppendChild(keyMsg);

            /*
             * END KEYS
             */

            XmlElement ksetMsg = doc.CreateElement(string.Empty, "ikmf-app-msg", string.Empty);

            XmlNode ksetHeaderNode = ksetMsg.OwnerDocument.ImportNode(buildHeader(true), true);
            ksetMsg.AppendChild(ksetHeaderNode);

            XmlElement ksetContent = doc.CreateElement(string.Empty, "ikmf-content", string.Empty);

            XmlElement ksetDissemination = doc.CreateElement(string.Empty, "Keyset-Dissemination", string.Empty);

            XmlElement ksetName = doc.CreateElement(string.Empty, "keyset-name", string.Empty);

            XmlElement ksetNameSource = doc.CreateElement(string.Empty, "keyset-name-source", string.Empty);
            XmlText ksetNameSourceText = doc.CreateTextNode("KeyFileGen");
            ksetNameSource.AppendChild(ksetNameSourceText);
            ksetName.AppendChild(ksetNameSource);

            XmlElement ksetNameKeyset = doc.CreateElement(string.Empty, "keyset-name-keyset", string.Empty);
            XmlText ksetNameKeysetText = doc.CreateTextNode("SET001");
            ksetNameKeyset.AppendChild(ksetNameKeysetText);
            ksetName.AppendChild(ksetNameKeyset);

            ksetDissemination.AppendChild(ksetName);

            // hardcode to kset ID 1 for now
            XmlElement ksetId = doc.CreateElement(string.Empty, "keyset-ID", string.Empty);
            XmlText ksetIdText = doc.CreateTextNode("1");
            ksetId.AppendChild(ksetIdText);
            ksetDissemination.AppendChild(ksetId);

            XmlElement ksetisActive = doc.CreateElement(string.Empty, "is-keyset-active", string.Empty);
            XmlText ksetisActiveText = doc.CreateTextNode("true");
            ksetisActive.AppendChild(ksetisActiveText);
            ksetDissemination.AppendChild(ksetisActive);

            ksetContent.AppendChild(ksetDissemination);
            ksetMsg.AppendChild(ksetContent);

            kmf_file.AppendChild(ksetMsg);


            return doc.OuterXml;
        }

        private static XmlElement buildHeader(bool keyset = false)
        {
            var doc = new XmlDocument();
            XmlElement root = doc.DocumentElement;
            XmlElement header = doc.CreateElement(string.Empty, "ikmf-header", string.Empty);

            // MFID
            XmlElement mfid = doc.CreateElement(string.Empty, "mf-id", string.Empty);
            XmlText mfidText = doc.CreateTextNode("90");
            mfid.AppendChild(mfidText);
            header.AppendChild(mfid);

            // protocl version
            XmlElement ver = doc.CreateElement(string.Empty, "protocol-version", string.Empty);
            XmlText verText = doc.CreateTextNode("1");
            ver.AppendChild(verText);
            header.AppendChild(ver);

            // source RSI
            XmlElement srsi = doc.CreateElement(string.Empty, "source-RSI", string.Empty);
            XmlText srsiText = doc.CreateTextNode("98967F");
            srsi.AppendChild(srsiText);
            header.AppendChild(srsi);

            // destination RSI
            XmlElement drsi = doc.CreateElement(string.Empty, "destination-RSI", string.Empty);
            XmlText drsiText = doc.CreateTextNode("FFFFFF");
            drsi.AppendChild(drsiText);
            header.AppendChild(drsi);

            // message ID
            XmlElement msgId = doc.CreateElement(string.Empty, "msg-ID", string.Empty);
            string msgIdInnerText = "";

            if (keyset)
            {
                msgIdInnerText = "KeysetDis";
            }
            else
            {
                msgIdInnerText = "KeyDis";
            }

            XmlText msgIdText = doc.CreateTextNode(msgIdInnerText);
            msgId.AppendChild(msgIdText);
            header.AppendChild(msgId);

            // ctag
            XmlElement ctag = doc.CreateElement(string.Empty, "ctag", string.Empty);

            XmlElement ctag_source = doc.CreateElement(string.Empty, "ctag-source", string.Empty);
            XmlText ctag_sourceText = doc.CreateTextNode("KeyFileGen");
            ctag_source.AppendChild(ctag_sourceText);
            ctag.AppendChild(ctag_source);

            XmlElement ctag_tag = doc.CreateElement(string.Empty, "ctag-tag", string.Empty);
            XmlText ctag_tagText = doc.CreateTextNode("0001");
            ctag_tag.AppendChild(ctag_tagText);
            ctag.AppendChild(ctag_tag);

            header.AppendChild(ctag);

            // time
            XmlElement time = doc.CreateElement(string.Empty, "time", string.Empty);
            XmlText timeText = doc.CreateTextNode("2012-01-13T14:20:49");  //TODO: fix this and make it current time??
            time.AppendChild(timeText);
            header.AppendChild(time);

            return header;
        }
    }
}
