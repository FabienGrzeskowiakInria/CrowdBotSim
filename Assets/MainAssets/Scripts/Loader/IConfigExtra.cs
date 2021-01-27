using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System;

namespace CrowdMP.Core
{

    /// <summary>
    /// Interface to create new addOn config line in the config.xml
    /// </summary>
    public abstract class ConfigExtra
    {
        [XmlAttribute]
        public bool isUsed;
    }
}