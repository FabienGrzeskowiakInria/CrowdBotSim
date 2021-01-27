using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

public class LawFileData : ControlLaw
{
    [XmlAttribute]
    public int timeColumn;
    [XmlAttribute]
    public int xColumn;
    [XmlAttribute]
    public int yColumn;
    [XmlAttribute]
    public int zColumn;

    private bool bX;
    private bool bY;
    private bool bZ;

    public string dataFile;

    private List<Vector4> data;
    private Agent linkedAgent;

    public LawFileData()
    {
        timeColumn = 1;
        xColumn = 0;
        yColumn = 0;
        zColumn = 0;
        dataFile = "file.dat";
    }


    public void initialize(Agent a)
    {
        linkedAgent = a;

        bX = xColumn != 0;
        bY = yColumn != 0;
        bZ = zColumn != 0;

        if (File.Exists(dataFile) == false)
        {
            ToolsDebug.logFatalError("Error on LawFileData, File " + dataFile + " doesn't exist");
        }
        if (timeColumn == 0)
        {
            ToolsDebug.logFatalError("Error on LawFileData, no time column given");
        }
        data = new List<Vector4>();
        using (StreamReader sr = new StreamReader(dataFile))
        {
            string line = "";

            int indexTime = Math.Abs(timeColumn);
            int indexX = Math.Abs(xColumn);
            int indexY = Math.Abs(yColumn);
            int indexZ = Math.Abs(zColumn);
            int invertTime = Math.Sign(timeColumn);
            int invertX = Math.Sign(xColumn);
            int invertY = Math.Sign(yColumn);
            int invertZ = Math.Sign(zColumn);

            ToolsDebug.log("Sign : " + invertTime + " | " + invertX + " | " + invertY + " | " + invertZ);
            int minimumColumns = Math.Max(Math.Max(Math.Max(indexTime, indexX), indexY), indexZ);

            while ((line = sr.ReadLine()) != null)
            {
                if (line != "" && line != "\r\n")
                {
                    List<string> lineElements = line.Split(new char[] { ',' }).ToList<string>();
                    if (lineElements.Count < 2)
                        lineElements = line.Split(new char[] { ';' }).ToList<string>();
                    lineElements.RemoveAll(s => (s == "\r\n" || s == "\n" || s == "\r" || s == "" || s == ";"));

                    if (lineElements.Count >= minimumColumns)
                    {
                        Vector4 dataLine = new Vector4();
                        dataLine.w = invertTime * ToolsGeneral.stringToFloat(lineElements[indexTime - 1]);
                        dataLine.x = indexX > 0 ? invertX * ToolsGeneral.stringToFloat(lineElements[indexX - 1]) : 0;
                        dataLine.y = indexY > 0 ? invertY * ToolsGeneral.stringToFloat(lineElements[indexY - 1]) : 0;
                        dataLine.z = indexZ > 0 ? invertZ * ToolsGeneral.stringToFloat(lineElements[indexZ - 1]) : 0;
                        data.Add(dataLine);
                    }
                    else
                    {
                        ToolsDebug.logFatalError("Error on LawFileData, File " + dataFile + " doesn't have the good format");
                    }
                }
            }
        }
    }

    public bool computeGlobalMvt(float deltaTime, out Vector3 translation, out Vector3 rotation)
    {
        if (data == null)
            ToolsDebug.logFatalError("Error on LawFileData, data is null");

        cleanData(ToolsTime.TrialTime);

        if (data.Count == 1 || ToolsTime.TrialTime <= data[0].w)
        {
            translation = new Vector3(data[0].x, data[0].y, data[0].z);
        }
        else
        {
            translation = getNewPosition(ToolsTime.TrialTime);
        }

        if (!bX)
            translation.x = linkedAgent.transform.position.x;
        if (!bY)
            translation.y = linkedAgent.transform.position.y;
        if (!bZ)
            translation.z = linkedAgent.transform.position.z;


        Vector3 tmp = translation - linkedAgent.transform.position;

        if (tmp.sqrMagnitude > 0.001*ToolsTime.DeltaTime)
        {
            Quaternion q = Quaternion.LookRotation(tmp, linkedAgent.gameObject.transform.up);
            rotation = q.eulerAngles;
            rotation.x = 0;
            rotation.z = 0;
        }
        else
        {
            rotation = linkedAgent.transform.rotation.eulerAngles;
        }

        //translation.z = translation.magnitude;
        //translation.x = 0;
        //translation.y = 0;
        return true;
    }

    private void cleanData(float time)
    {
        while (data.Count >= 2 && data[1].w < time)
        {
            data.RemoveAt(0);
        }
    }

    public bool applyMvt(Agent a, Vector3 translation, Vector3 rotation)
    {
        a.transform.position = translation;
        a.transform.rotation = Quaternion.Euler(rotation);
        return true;
    }

    private Vector3 getNewPosition(float currentTime)
    {
        Vector3 newPosition = new Vector3(0, 0, 0);
        newPosition.x = data[0].x
                        + (currentTime - data[0].w)
                        / (data[1].w - data[0].w)
                        * (data[1].x - data[0].x);
        newPosition.y = data[0].y
                        + (currentTime - data[0].w)
                        / (data[1].w - data[0].w)
                        * (data[1].y - data[0].y);
        newPosition.z = data[0].z
                        + (currentTime - data[0].w)
                        / (data[1].w - data[0].w)
                        * (data[1].z - data[0].z);
        return newPosition;
    }
}
