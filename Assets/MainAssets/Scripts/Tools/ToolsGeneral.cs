using System;
using System.Globalization;
using UnityEngine;

public class ToolsGeneral {

    /// <summary>
    /// Convert strings to float.
    /// </summary>
    /// <returns>the float.</returns>
    /// <param name="s">String to convert.</param>
    public static float stringToFloat(string s)
    {
        s = numberInUSFormat(s);
        float result;
        float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        return result;
    }

    /// <summary>
    /// Transform a string representing a number un FR format into a number in US Format
    /// </summary>
    /// <param name="s"> the string to transform</param>
    /// <returns>the string representing the number in us format</returns>
    public static string numberInUSFormat(string s)
    {
        return s.Replace(",", ".");
    }

    public static Vector2 convert(Vector3 v)
    {
        return new Vector2(-v.x, v.z);
    }

    public static Vector3 convert(Vector2 v)
    {
        return new Vector3(-v.x, 0, v.y);
    }

}
