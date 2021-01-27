using System;
using System.Xml.Serialization;

namespace CrowdMP.Core
{

    /// <summary>
    /// Serializer to handle xml serialization of abstract classes
    /// Code from https://stackoverflow.com/questions/20084/xml-serialization-and-inherited-types
    /// </summary>
    public class CustomXmlSerializer<AbstractType> : IXmlSerializable
    {
        // Override the Implicit Conversions Since the XmlSerializer
        // Casts to/from the required types implicitly.
        public static implicit operator AbstractType(CustomXmlSerializer<AbstractType> o)
        {
            return o.Data;
        }

        public static implicit operator CustomXmlSerializer<AbstractType>(AbstractType o)
        {
            return o == null ? null : new CustomXmlSerializer<AbstractType>(o);
        }

        private AbstractType _data;
        /// <summary>
        /// [Concrete] Data to be stored/is stored as XML.
        /// </summary>
        public AbstractType Data
        {
            get { return _data; }
            set { _data = value; }
        }

        /// <summary>
        /// **DO NOT USE** This is only added to enable XML Serialization.
        /// </summary>
        /// <remarks>DO NOT USE THIS CONSTRUCTOR</remarks>
        public CustomXmlSerializer()
        {
            // Default Ctor (Required for Xml Serialization - DO NOT USE)
        }

        /// <summary>
        /// Initialises the Serializer to work with the given data.
        /// </summary>
        /// <param name="data">Concrete Object of the AbstractType Specified.</param>
        public CustomXmlSerializer(AbstractType data)
        {
            _data = data;
        }

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null; // this is fine as schema is unknown.
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            // Cast the Data back from the Abstract Type.
            reader.ReadStartElement();
            string typeAttrib = reader.LocalName;

            // Ensure the Type was Specified
            if (typeAttrib == null)
                throw new ArgumentNullException("Unable to Read Xml Data for Abstract Type '" + typeof(AbstractType).Name +
                    "' because no 'type' attribute was specified in the XML.");

            Type type = Type.GetType(typeAttrib);

            // Check the Type is Found.
            if (type == null)
            {
                type = Type.GetType("CrowdMP.Core." + typeAttrib);
            }
            if (type == null)
                throw new InvalidCastException("Unable to Read Xml Data for Abstract Type '" + typeof(AbstractType).Name +
                    "' because the type specified in the XML was not found.");

            // Check the Type is a Subclass of the AbstractType.
            if (!typeof(AbstractType).IsAssignableFrom(type))
                throw new InvalidCastException("Unable to Read Xml Data for Abstract Type '" + typeof(AbstractType).Name +
                    "' because the Type specified in the XML differs ('" + type.Name + "').");

            // Read the Data, Deserializing based on the (now known) concrete type.
            //reader.ReadStartElement();
            this.Data = (AbstractType)new
                XmlSerializer(type).Deserialize(reader);
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            // Write the Type Name to the XML Element as an Attrib and Serialize
            Type type = _data.GetType();

            // BugFix: Assembly must be FQN since Types can/are external to current.
            //writer.WriteAttributeString("type", type.FullName);
            new XmlSerializer(type).Serialize(writer, _data);
        }

        #endregion
    }

}