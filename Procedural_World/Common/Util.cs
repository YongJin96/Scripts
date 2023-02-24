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

    public static T[] FindObjectsOfType<T>()
    {
        T[] objects = Object.FindObjectsOfType(typeof(T)) as T[];
        return objects;
    }

    public static Vector3 GetDirection(Vector3 targetPos1, Vector3 targetPos2)
    {
        Vector3 direction = targetPos1 - targetPos2;
        return -direction.normalized;
    }
}
