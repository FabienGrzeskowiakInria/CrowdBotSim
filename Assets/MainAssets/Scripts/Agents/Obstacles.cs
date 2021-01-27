using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using CrowdMP.Core;

/// <summary>
/// Trial parameters concerning the obstacles (XML serializable)
/// </summary>
public class TrialObstacle
{
    public struct line
    {
        [XmlAttribute]
        public float x1;
        [XmlAttribute]
        public float y1;
        [XmlAttribute]
        public float x2;
        [XmlAttribute]
        public float y2;

        [XmlAttribute]
        public float thickness;

        public line(float _x1, float _y1, float _x2, float _y2, float _th)
        {
            x1 = _x1;
            y1 = _y1;
            x2 = _x2;
            y2 = _y2;
            thickness = _th;
        }
    }
    public struct point
    {
        [XmlAttribute]
        public float x1;
        [XmlAttribute]
        public float y1;

        [XmlAttribute]
        public float thickness;

        public point(float _x1, float _y1, float _th)
        {
            x1 = _x1;
            y1 = _y1;
            thickness = _th;
        }
    }

    [XmlArray("Lines")]
    [XmlArrayItem("Line")]
    public List<line> lines;

    [XmlArray("Points")]
    [XmlArrayItem("Point")]
    public List<point> points;

    public TrialObstacle()
    {
        lines = new List<line>();
        points = new List<point>();
    }

}