using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CrowdMP.Core
{

    /// <summary>
    /// Tools to serialize/deserialize data to/from XML files
    /// </summary>
    public static class LoaderXML
    {
        // -----------------------------------------
        // DATA SERIALIZATION FOR XML SAVING/LOADING
        #region dataSerialization
        public static void CreateXML<ObjType>(string fileName, object pObject)
        {
            StreamWriter writer;
            FileInfo t = new FileInfo(fileName);
            if (!t.Exists)
            {
                writer = t.CreateText();
            }
            else
            {
                t.Delete();
                writer = t.CreateText();
            }
            writer.Write(SerializeObject<ObjType>(pObject));
            writer.Close();
        }

        public static object LoadXML<ObjType>(string fileName)
        {
            StreamReader r = File.OpenText(fileName);
            string _info = r.ReadToEnd();
            r.Close();

            if (_info.ToString() != "")
                return DeserializeObject<ObjType>(_info);
            else
                return null;
        }

        static string SerializeObject<ObjType>(object pObject)
        {
            string XmlizedString = null;
            MemoryStream memoryStream = new MemoryStream();
            XmlSerializer xs = new XmlSerializer(typeof(ObjType));
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);

            xmlTextWriter.Formatting = Formatting.Indented;
            xs.Serialize(xmlTextWriter, pObject);
            memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
            XmlizedString = UTF8ByteArrayToString(memoryStream.ToArray());
            return XmlizedString;
        }

        static object DeserializeObject<ObjType>(string pXmlizedString)
        {
            XmlSerializer xs = new XmlSerializer(typeof(ObjType));
            MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(pXmlizedString));
            return xs.Deserialize(memoryStream);
        }

        static string UTF8ByteArrayToString(byte[] characters)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            string constructedString = encoding.GetString(characters);
            return (constructedString);
        }

        static byte[] StringToUTF8ByteArray(string pXmlString)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] byteArray = encoding.GetBytes(pXmlString);
            return byteArray;
        }
        // DATA SERIALIZATION FOR XML SAVING/LOADING
        // -----------------------------------------
        #endregion
    }
}