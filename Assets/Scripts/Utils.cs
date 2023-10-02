using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static List<Transform> GetChildrenFromParent(Transform parent)
    {
        List<Transform> toReturn = new List<Transform>();

        for (int i = 0; i < parent.childCount; i++)
        {
            toReturn.Add(parent.GetChild(i));
        }

        return toReturn;
    }

    public static float RoundToNearest(float value, float step)
    {
        return Mathf.Round(value / step) * step;
    }

    public static Vector3 RoundToNearest(Vector3 value, float step)
    {
        value.x = Mathf.Round(value.x / step) * step;
        value.y = Mathf.Round(value.y / step) * step;
        value.z = Mathf.Round(value.z / step) * step;
        return value;
    }
}
