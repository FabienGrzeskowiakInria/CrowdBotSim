using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

/// <summary>
/// Control the creation of output file
/// </summary>
public class ToolsOutput
{
    string filePath;

    public ToolsOutput(string path)
    {
        //System.IO.Directory.CreateDirectory(_DirectoryPath);
        filePath = path;

        // Rename the file if it exists.
        int i = 0;
        while (File.Exists(filePath))
        {
            i++;
            filePath = path + ".c" + i.ToString();
        }

#if MIDDLEVR
        if (VRTools.IsMaster())
#endif
            using (FileStream stream = File.Create(filePath)) { }
    }

    /// <summary>
    /// Adds to current log file a text.
    /// </summary>
    /// <param name='s'>
    /// Text to add inside the current log file.
    /// </param>
    public void writeLine(string s)
    {
#if MIDDLEVR
        if (VRTools.IsMaster())
#endif
            using (StreamWriter Writer = new StreamWriter(filePath, true))
            {
                Writer.WriteLine(s);
            }
    }
}