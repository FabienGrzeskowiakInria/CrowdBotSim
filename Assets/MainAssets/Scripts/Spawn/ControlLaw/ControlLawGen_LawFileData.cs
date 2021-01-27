using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

//public class LawFileData : ControlLaw
namespace CrowdMP.Core
{
    public class ControlLawGen_LawFileData : ControlLawGen
    {
        [Header("Main Parameters")]
        [XmlAttribute]
        public int timeColumn;
        [XmlAttribute]
        public int xColumn;
        [XmlAttribute]
        public int yColumn;
        [XmlAttribute]
        public int zColumn;

        public string dataFile;



        public override CustomXmlSerializer<ControlLaw> createControlLaw()
        {
            LawFileData law = new LawFileData();
            law.timeColumn = timeColumn;
            law.xColumn = xColumn;
            law.yColumn = yColumn;
            law.zColumn = zColumn;
            law.dataFile = dataFile;

            return law;
        }

        public override ControlLawGen randDraw(GameObject agent, int id = 0)
        {
            ControlLawGen_LawFileData law = agent.AddComponent<ControlLawGen_LawFileData>();

            law.timeColumn = timeColumn;
            law.xColumn = xColumn;
            law.yColumn = yColumn;
            law.zColumn = zColumn;
            law.dataFile = dataFile.Replace("{USER}", id.ToString());

            return law;
        }
    }
}