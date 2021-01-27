using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ButtonCode
{
    Fire1 = 0,
    Fire2 = 1,
    Fire3 = 2,
    Fire4 = 3,
    Fire5 = 4,
    Fire6 = 5,
    Fire7 = 6,
    Fire8 = 7,
    Fire9 = 8,
    Fire10 = 9,
    Fire11 = 10
}

public enum ToolsAxis
{
    Horizontal=0,
    Vertical=1,
    Axis2=2,
    Horizontal2=3,
    Vertical2=4
}

/// <summary>
/// Control any inputs (can be modified for different platform/device)
/// </summary>
public static class ToolsInput {

    /// <summary>
    /// Check if a key from the keyboard has been pushed down during the current frame
    /// </summary>
    /// <param name="code"> Key code </param>
    /// <returns> True if the Key corresponding to the key code has been pushed down </returns>
    public static bool GetKeyDown(KeyCode code)
    {
#if MIDDLEVR
        return VRTools.GetKeyDown(code);
#else
        return Input.GetKeyDown(code);
#endif
    }

    /// <summary>
    /// Check if a button from an input device has been pushed down during the current frame
    /// </summary>
    /// <param name="code"> Key code </param>
    /// <returns> True if the button corresponding to the key code has been pushed down </returns>
    public static bool GetButtonDown(ButtonCode button)
    {
#if MIDDLEVR
        return VRTools.IsButtonToggled((uint)button, true);
        //return VRTools.IsButtonPressed((uint)button);
#else
        return Input.GetButtonDown(button.ToString());
#endif
    }

    public static float getAxisValue(ToolsAxis axis)
    {
#if MIDDLEVR
        if (axis == ToolsAxis.Horizontal)
            return VRTools.GetWandHorizontalValue();
        else if (axis == ToolsAxis.Vertical)
            return VRTools.GetWandVerticalValue();
        else
            return VRTools.GetWandAxisValue((uint)axis);
#else
        return Input.GetAxis(axis.ToString());
#endif
    }

}
