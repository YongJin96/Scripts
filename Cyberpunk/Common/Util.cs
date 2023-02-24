using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util
{
    public static Color ParseHexToColor(string hex)
    {
        Color color;
        if (hex != null)
            hex = hex.Contains("#") ? hex : "#" + hex;
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }

    public static string ParseColorToHex(Color color)
    {
        return ColorUtility.ToHtmlStringRGB(color);
    }

    public static void SetLayer(GameObject target, int layer)
    {
        target.layer = layer;
        for (int i = 0; i < target.transform.childCount; i++)
        {
            SetLayer(target.transform.GetChild(i).gameObject, layer);
        }
    }

    public static void SetIgnoreLayer(GameObject target1, GameObject target2, bool isIgnore)
    {
        Physics.IgnoreLayerCollision(target1.layer, target2.layer, isIgnore);
    }
}
