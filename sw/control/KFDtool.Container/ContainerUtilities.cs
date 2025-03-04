using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace KFDtool.Container
{
    public class ContainerUtilities
    {
        public static byte[] GenerateSalt(int saltLength)
        {
            byte[] salt = new byte[saltLength];

            using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(salt);
            }

            return salt;
        }

        public static byte[] Pbkdf2DeriveKeyFromPassword(string password, byte[] salt, int iterationCount, string hashAlgorithm, int keyLength)
        {
            Rfc2898DeriveBytes pbkdf2;

            if (hashAlgorithm == "SHA512")
            {
                pbkdf2 = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(password), salt, iterationCount, HashAlgorithmName.SHA512);
            }
            else
            {
                throw new Exception(string.Format("invalid hash algorithm: {0}", hashAlgorithm));
            }

            return pbkdf2.GetBytes(keyLength);
        }

        public static InnerContainer CreateInnerContainer()
        {
            InnerContainer innerContainer = new InnerContainer();
            innerContainer.Version = "1.0";
            innerContainer.Keys = new ObservableCollection<KeyItem>();
            innerContainer.NextKeyNumber = 1;
            innerContainer.Groups = new ObservableCollection<GroupItem>();
            innerContainer.NextGroupNumber = 1;
            return innerContainer;
        }

        public static XmlDocument SerializeInnerContainer(InnerContainer innerContainer)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.InsertBefore(xmlDeclaration, doc.DocumentElement);
            XPathNavigator nav = doc.CreateNavigator();
            using (XmlWriter w = nav.AppendChild())
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty); // remove xsd and xsi attributes
                XmlSerializer s = new XmlSerializer(typeof(InnerContainer));
                s.Serialize(w, innerContainer, ns);
            }
            return doc;
        }

        public static (OuterContainer Container, byte[] Key) CreateOuterContainer(string password)
        {
            string outerVersion = "1.0";

            int saltLength = 32; // critical security parameter

            int iterationCount = 100000; // critical security parameter

            string hashAlgorithm = "SHA512"; // critical security parameter

            int keyLength = 32; // critical security parameter

            byte[] salt = GenerateSalt(saltLength);

            byte[] key = Pbkdf2DeriveKeyFromPassword(password, salt, iterationCount, hashAlgorithm, keyLength);

            OuterContainer outerContainer = new OuterContainer();

            outerContainer.Version = outerVersion;
            outerContainer.KeyDerivation = new KeyDerivation();
            outerContainer.KeyDerivation.DerivationAlgorithm = "PBKDF2";
            outerContainer.KeyDerivation.HashAlgorithm = hashAlgorithm;
            outerContainer.KeyDerivation.Salt = salt;
            outerContainer.KeyDerivation.IterationCount = iterationCount;
            outerContainer.KeyDerivation.KeyLength = keyLength;
            outerContainer.EncryptedDataPlaceholder = "placeholder";

            return (outerContainer, key);
        }

        public static XmlDocument SerializeOuterContainer(OuterContainer outerContainer)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.InsertBefore(xmlDeclaration, doc.DocumentElement);
            XPathNavigator nav = doc.CreateNavigator();
            using (XmlWriter w = nav.AppendChild())
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty); // remove xsd and xsi attributes
                XmlSerializer s = new XmlSerializer(typeof(OuterContainer));
                s.Serialize(w, outerContainer, ns);
            }
            return doc;
        }

        public static byte[] EncryptOuterContainer(OuterContainer outerContainer, InnerContainer innerContainer, byte[] key)
        {
            XmlDocument outerContainerXml = SerializeOuterContainer(outerContainer);

            XmlDocument innerContainerXml = SerializeInnerContainer(innerContainer);

            XmlElement encryptedDataPlaceholder = outerContainerXml.GetElementsByTagName("EncryptedDataPlaceholder")[0] as XmlElement;

            XmlElement plaintextInnerContainer = innerContainerXml.GetElementsByTagName("InnerContainer")[0] as XmlElement;

            EncryptedData encryptedData = new EncryptedData();

            encryptedData.Type = EncryptedXml.XmlEncElementUrl;
            encryptedData.EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncAES256Url);

            EncryptedXml encryptedXml = new EncryptedXml();

            using (AesCryptoServiceProvider aesCsp = new AesCryptoServiceProvider())
            {
                aesCsp.KeySize = 256; // critical security parameter
                aesCsp.Key = key; // critical security parameter
                aesCsp.Mode = CipherMode.CBC; // critical security parameter
                aesCsp.GenerateIV(); // critical security parameter

                encryptedData.CipherData.CipherValue = encryptedXml.EncryptData(plaintextInnerContainer, aesCsp, false);
            }

            EncryptedXml.ReplaceElement(encryptedDataPlaceholder, encryptedData, false);

            byte[] outerContainerBytes = Encoding.UTF8.GetBytes(outerContainerXml.OuterXml);

            byte[] fileBytes = Shared.Utility.Compress(outerContainerBytes);

            return fileBytes;
        }

        private static XmlDocument SerializeDKFOuterPersonality(DKFOuterPersonality pers)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.InsertBefore(xmlDeclaration, doc.DocumentElement);
            XPathNavigator nav = doc.CreateNavigator();
            using (XmlWriter w = nav.AppendChild())
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty); // remove xsd and xsi attributes
                XmlSerializer s = new XmlSerializer(typeof(DKFOuterPersonality));
                s.Serialize(w, pers, ns);
            }
            return doc;
        }

        private static XmlDocument SerializeDKFOuterParamData(DKFOuterParamData paramData)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.InsertBefore(xmlDeclaration, doc.DocumentElement);
            XPathNavigator nav = doc.CreateNavigator();
            using (XmlWriter w = nav.AppendChild())
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty); // remove xsd and xsi attributes
                XmlSerializer s = new XmlSerializer(typeof(DKFOuterParamData));
                s.Serialize(w, paramData, ns);
            }
            return doc;
        }

        private static void SerializeDKFOuterParam(XmlDocument doc, DKFOuterParam param)
        {
            XmlDocument eleDoc = new XmlDocument();
            XPathNavigator nav = eleDoc.CreateNavigator();
            using (XmlWriter w = nav.AppendChild())
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty); // remove xsd and xsi attributes
                XmlSerializer s = new XmlSerializer(typeof(DKFOuterParam));
                s.Serialize(w, param, ns);
            }

            XmlElement valueData = doc.GetElementsByTagName("paramdata")[0] as XmlElement;

            XmlNode n = doc.ImportNode(eleDoc.DocumentElement, true);
            valueData.AppendChild(n);
        }

        private static XmlDocument SerializeDKFInnerPersonality(DKFInnerPersonality pers)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.InsertBefore(xmlDeclaration, doc.DocumentElement);
            XPathNavigator nav = doc.CreateNavigator();
            using (XmlWriter w = nav.AppendChild())
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty); // remove xsd and xsi attributes
                XmlSerializer s = new XmlSerializer(typeof(DKFInnerPersonality));
                s.Serialize(w, pers, ns);
            }
            return doc;
        }

        private static XmlDocument SerializeDKFInnerValueData(DKFInnerValueData valueData)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.InsertBefore(xmlDeclaration, doc.DocumentElement);
            XPathNavigator nav = doc.CreateNavigator();
            using (XmlWriter w = nav.AppendChild())
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty); // remove xsd and xsi attributes
                XmlSerializer s = new XmlSerializer(typeof(DKFInnerValueData));
                s.Serialize(w, valueData, ns);
            }
            return doc;
        }

        private static void SerializeDKFInnerParam(XmlDocument doc, DKFInnerParam param)
        {
            XmlDocument eleDoc = new XmlDocument();
            XPathNavigator nav = eleDoc.CreateNavigator();
            using (XmlWriter w = nav.AppendChild())
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty); // remove xsd and xsi attributes
                XmlSerializer s = new XmlSerializer(typeof(DKFInnerParam));
                s.Serialize(w, param, ns);
            }

            XmlElement valueData = doc.GetElementsByTagName("valuedata")[0] as XmlElement;

            XmlNode n = doc.ImportNode(eleDoc.DocumentElement, true);
            valueData.AppendChild(n);
        }

        private static void SerializeDKFInnerKey(XmlDocument doc, DKFInnerKey key)
        {
            XmlDocument eleDoc = new XmlDocument();
            XPathNavigator nav = eleDoc.CreateNavigator();
            using (XmlWriter w = nav.AppendChild())
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty); // remove xsd and xsi attributes
                XmlSerializer s = new XmlSerializer(typeof(DKFInnerKey));
                s.Serialize(w, key, ns);
            }

            XmlElement valueData = doc.GetElementsByTagName("valuedata")[0] as XmlElement;

            XmlNode n = doc.ImportNode(eleDoc.DocumentElement, true);
            valueData.AppendChild(n);
        }

        public static byte[] EncryptOuterContainerDKF(OuterContainer outerContainer, InnerContainer innerContainer, string password)
        {
            // generate inner DKF container
            string innerIVBase64 = string.Empty;
            string innerSaltBase64 = string.Empty;
            string innerBase64 = string.Empty;
            {
                DKFInnerValueData valueData = new DKFInnerValueData();

                // add DKF params
                XmlDocument valueDataXml = SerializeDKFInnerValueData(valueData);
                DKFInnerParam persEnableFips = new DKFInnerParam(0, "enablefips", "boolean", "false");
                SerializeDKFInnerParam(valueDataXml, persEnableFips);
                DKFInnerParam persPIN = new DKFInnerParam(1, "pin", "string", "");
                SerializeDKFInnerParam(valueDataXml, persPIN);
                DKFInnerParam persKEK = new DKFInnerParam(2, "kek", "string", "");
                SerializeDKFInnerParam(valueDataXml, persKEK);
                DKFInnerParam persPrevKEK = new DKFInnerParam(3, "prevkek", "string", "");
                SerializeDKFInnerParam(valueDataXml, persPrevKEK);
                DKFInnerParam persDESMAC = new DKFInnerParam(4, "desmac", "string", "");
                SerializeDKFInnerParam(valueDataXml, persDESMAC);
                DKFInnerParam persPrevDESMAC = new DKFInnerParam(5, "prevdesmac", "string", "");
                SerializeDKFInnerParam(valueDataXml, persPrevDESMAC);
                DKFInnerParam persVersion = new DKFInnerParam(6, "version", "integer", "5");
                SerializeDKFInnerParam(valueDataXml, persVersion);
                DKFInnerParam persKEKEncryptionAlg = new DKFInnerParam(7, "kekencryptionalg", "string", "");
                SerializeDKFInnerParam(valueDataXml, persKEKEncryptionAlg);
                DKFInnerParam persDESMACEncryptionAlg = new DKFInnerParam(8, "desmacencryptionalg", "string", "");
                SerializeDKFInnerParam(valueDataXml, persDESMACEncryptionAlg);
                DKFInnerParam persTEKEncryptionAlg = new DKFInnerParam(9, "tekencryptionalg", "string", "Unencrypted");
                SerializeDKFInnerParam(valueDataXml, persTEKEncryptionAlg);
                DKFInnerParam persAESFIPSKEK = new DKFInnerParam(10, "aesfipskek", "string", "");
                SerializeDKFInnerParam(valueDataXml, persAESFIPSKEK);
                DKFInnerParam persPrevAESFIPSKEK = new DKFInnerParam(11, "prevaesfipskek", "string", "");
                SerializeDKFInnerParam(valueDataXml, persPrevAESFIPSKEK);
                DKFInnerParam persFIPSMACKey = new DKFInnerParam(12, "fipsmackey", "string", "");
                SerializeDKFInnerParam(valueDataXml, persFIPSMACKey);
                DKFInnerParam persPrevFIPSMACKey = new DKFInnerParam(13, "prevfipsmackey", "string", "");
                SerializeDKFInnerParam(valueDataXml, persPrevFIPSMACKey);
                DKFInnerParam persAESFIPSKEKEncryptionAlg = new DKFInnerParam(14, "aesfipskekencryptionalg", "string", "");
                SerializeDKFInnerParam(valueDataXml, persAESFIPSKEKEncryptionAlg);
                DKFInnerParam persFIPSMACKeyEncryptionAlg = new DKFInnerParam(15, "fipsmackeyencryptionalg", "string", "");
                SerializeDKFInnerParam(valueDataXml, persFIPSMACKeyEncryptionAlg);
                DKFInnerParam persPINIsValidFlag = new DKFInnerParam(16, "pinisvalidflag", "boolean", "false");
                SerializeDKFInnerParam(valueDataXml, persPINIsValidFlag);
                DKFInnerParam persIsMaster = new DKFInnerParam(17, "ismaster", "boolean", "true");
                SerializeDKFInnerParam(valueDataXml, persIsMaster);

                // add keys
                foreach (KeyItem containerKey in innerContainer.Keys)
                {
                    string algString = "Aes";
                    switch (containerKey.AlgorithmId)
                    {
                        case 0x81:
                            algString = "Des";
                            break;
                        case 0x84:
                            algString = "Aes";
                            break;
                        default:
                            continue; // skip key
                    }

                    if (containerKey.KeyTypeKek)
                        continue;

                    DKFInnerKey dkfKey = new DKFInnerKey(algString, containerKey.KeyId.ToString(), containerKey.Sln.ToString(), containerKey.Key);
                    dkfKey.KSID = containerKey.KeysetId.ToString();
                    dkfKey.KSName = $"Set{containerKey.KeysetId.ToString("D3")}";
                    SerializeDKFInnerKey(valueDataXml, dkfKey);
                }

                // create structures for DKF
                DKFInnerPersonality personality = new DKFInnerPersonality()
                {
                    Definition = "1",
                    Device = "1",
                    Name = "KFDtool EKC to DKF",
                    Family = "KeyManager"
                };

                // serialize
                XmlDocument personalityXml = SerializeDKFInnerPersonality(personality);

                XmlElement personalityElement = personalityXml.GetElementsByTagName("personality")[0] as XmlElement;

                XmlNode n = personalityXml.ImportNode(valueDataXml.DocumentElement, true);
                personalityElement.AppendChild(n);

                string xml = personalityXml.OuterXml;
                byte[] xmlBytes = Encoding.UTF8.GetBytes(xml);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress);
                    gZipStream.Write(xmlBytes, 0, xmlBytes.Length);
                    gZipStream.Close();
                    xmlBytes = memoryStream.ToArray();
                }

                using (AesCryptoServiceProvider aesCsp = new AesCryptoServiceProvider())
                {
                    aesCsp.KeySize = 256; // critical security parameter
                    aesCsp.Mode = CipherMode.CBC; // critical security parameter

                    // generate SALT and IV
                    RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider();
                    byte[] salt = new byte[32];
                    rNGCryptoServiceProvider.GetBytes(salt);
                    innerSaltBase64 = Convert.ToBase64String(salt);
                    byte[] iv = new byte[16];
                    rNGCryptoServiceProvider.GetBytes(iv);
                    innerIVBase64 = Convert.ToBase64String(iv);

                    Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, 10000);
                    byte[] passwordKey = rfc2898DeriveBytes.GetBytes(32);

                    using (ICryptoTransform cryptoTransform = aesCsp.CreateEncryptor(passwordKey, iv))
                    {
                        MemoryStream xmlms = new MemoryStream(xmlBytes);

                        MemoryStream memoryStream = new MemoryStream();
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                        {
                            byte[] buffer = new byte[4096];
                            for (int i = xmlms.Read(buffer, 0, 4096); i > 0; i = xmlms.Read(buffer, 0, 4096))
                                cryptoStream.Write(buffer, 0, i);
                        }

                        xmlBytes = memoryStream.ToArray();
                    }
                }

                innerBase64 = Convert.ToBase64String(xmlBytes);
            }

            // generate outer DKR container
            byte[] outBytes = null;
            {
                DKFOuterParamData paramData = new DKFOuterParamData();

                // add DKF params
                XmlDocument paramDataXml = SerializeDKFOuterParamData(paramData);
                DKFOuterParam persEncryptMode = new DKFOuterParam("encryptMode", "string", "Aes256Cbc");
                SerializeDKFOuterParam(paramDataXml, persEncryptMode);
                DKFOuterParam persRFC2898 = new DKFOuterParam("rfc2898", "boolean", "True");
                SerializeDKFOuterParam(paramDataXml, persRFC2898);
                DKFOuterParam persIV = new DKFOuterParam("iv", "string", innerIVBase64);
                SerializeDKFOuterParam(paramDataXml, persIV);
                DKFOuterParam persSalt = new DKFOuterParam("salt", "string", innerSaltBase64);
                SerializeDKFOuterParam(paramDataXml, persSalt);
                DKFOuterParam persIterations = new DKFOuterParam("iterations", "int32", "10000");
                SerializeDKFOuterParam(paramDataXml, persIterations);
                DKFOuterParam perContent = new DKFOuterParam("content", "string", innerBase64);
                SerializeDKFOuterParam(paramDataXml, perContent);

                // create structures for DKF
                DKFOuterPersonality personality = new DKFOuterPersonality();

                // serialize
                XmlDocument personalityXml = SerializeDKFOuterPersonality(personality);

                XmlElement personalityElement = personalityXml.GetElementsByTagName("personality")[0] as XmlElement;

                XmlNode n = personalityXml.ImportNode(paramDataXml.DocumentElement, true);
                personalityElement.AppendChild(n);

                outBytes = Encoding.UTF8.GetBytes(personalityXml.OuterXml);
            }

            return outBytes;
        }

        public static (OuterContainer ContainerOuter, InnerContainer ContainerInner, byte[] Key) DecryptOuterContainer(byte[] fileBytes, string password)
        {
            byte[] outerContainerBytes;

            try
            {
                outerContainerBytes = Shared.Utility.Decompress(fileBytes);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("failed to decompress (corrupted or not a valid container file): {0}", ex.Message));
            }

            XmlDocument outerContainerXml = new XmlDocument();

            outerContainerXml.LoadXml(Encoding.UTF8.GetString(outerContainerBytes));

            string version;

            try
            {
                version = outerContainerXml.SelectSingleNode("/OuterContainer/@version").Value;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("unable to read /OuterContainer/@version: {0}", ex.Message));
            }

            Version parsedVersion = new Version(version);

            if (parsedVersion.Major != 1)
            {
                throw new Exception(string.Format("outer container version too new (this container was written on a newer software version) - expected 1, got {0}", parsedVersion.Major));
            }

            OuterContainer outerContainer = new OuterContainer();

            string outerVersion = "1.0";

            outerContainer.Version = outerVersion;
            outerContainer.KeyDerivation = new KeyDerivation();

            byte[] key;

            string derivationAlgorithm;

            try
            {
                derivationAlgorithm = outerContainerXml.SelectSingleNode("/OuterContainer/KeyDerivation/DerivationAlgorithm").InnerText;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("invalid KeyDerivation DerivationAlgorithm: {0}", ex.Message));
            }

            outerContainer.KeyDerivation.DerivationAlgorithm = derivationAlgorithm;

            if (derivationAlgorithm == "PBKDF2")
            {
                string hashAlgorithm;

                try
                {
                    hashAlgorithm = outerContainerXml.SelectSingleNode("/OuterContainer/KeyDerivation/HashAlgorithm").InnerText;
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("invalid KeyDerivation HashAlgorithm: {0}", ex.Message));
                }

                outerContainer.KeyDerivation.HashAlgorithm = hashAlgorithm;

                byte[] salt;

                try
                {
                    salt = Convert.FromBase64String(outerContainerXml.SelectSingleNode("/OuterContainer/KeyDerivation/Salt").InnerText);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("invalid KeyDerivation Salt: {0}", ex.Message));
                }

                outerContainer.KeyDerivation.Salt = salt;

                int iterationCount;

                try
                {
                    iterationCount = Convert.ToInt32(outerContainerXml.SelectSingleNode("/OuterContainer/KeyDerivation/IterationCount").InnerText);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("invalid KeyDerivation IterationCount: {0}", ex.Message));
                }

                outerContainer.KeyDerivation.IterationCount = iterationCount;

                int keyLength;

                try
                {
                    keyLength = Convert.ToInt32(outerContainerXml.SelectSingleNode("/OuterContainer/KeyDerivation/KeyLength").InnerText);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("invalid KeyDerivation KeyLength: {0}", ex.Message));
                }

                outerContainer.KeyDerivation.KeyLength = keyLength;

                key = Pbkdf2DeriveKeyFromPassword(password, salt, iterationCount, hashAlgorithm, keyLength);
            }
            else
            {
                throw new Exception(string.Format("unsupported KeyDerivation DerivationAlgorithm: {0}", derivationAlgorithm));
            }

            outerContainer.EncryptedDataPlaceholder = "placeholder";

            XmlElement encryptedDataElement = outerContainerXml.GetElementsByTagName("EncryptedData")[0] as XmlElement;

            if (encryptedDataElement == null)
            {
                throw new Exception("unable to find EncryptedData");
            }

            EncryptedData encryptedData = new EncryptedData();

            encryptedData.LoadXml(encryptedDataElement);

            EncryptedXml encryptedXml = new EncryptedXml();

            byte[] innerContainerBytes;

            using (AesCryptoServiceProvider aesCsp = new AesCryptoServiceProvider())
            {
                aesCsp.KeySize = 256; // critical security parameter
                aesCsp.Key = key; // critical security parameter
                aesCsp.Mode = CipherMode.CBC; // critical security parameter
                aesCsp.GenerateIV(); // critical security parameter

                try
                {
                    innerContainerBytes = encryptedXml.DecryptData(encryptedData, aesCsp);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("unable to decrypt InnerContainer (check password): {0}", ex.Message));
                }
            }

            XmlDocument innerContainerXml = new XmlDocument();

            try
            {
                innerContainerXml.LoadXml(Encoding.UTF8.GetString(innerContainerBytes));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("unable to parse InnerContainer (check password): {0}", ex.Message));
            }

            string innerReadVersion;

            try
            {
                innerReadVersion = innerContainerXml.SelectSingleNode("/InnerContainer/@version").Value;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("unable to read /InnerContainer/@version: {0}", ex.Message));
            }

            Version innerReadVersionParsed = new Version(innerReadVersion);

            if (innerReadVersionParsed.Major != 1)
            {
                throw new Exception(string.Format("inner container version too new (this container was written on a newer software version) - expected 1, got {0}", innerReadVersionParsed.Major));
            }

            InnerContainer innerContainer;

            using (XmlReader xmlReader = new XmlNodeReader(innerContainerXml))
            {
                try
                {
                    innerContainer = (InnerContainer)new XmlSerializer(typeof(InnerContainer)).Deserialize(xmlReader);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("unable to deserialize InnerContainer: {0}", ex.Message));
                }
            }

            return (outerContainer, innerContainer, key);
        }
    }
}
