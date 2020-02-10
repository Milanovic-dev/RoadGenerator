using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NColorPicker : MonoBehaviour
{
    private static NColorPicker instance;

    private void Awake()
    {
        instance = this;
    }

    public Color[] Colors;

    public static Color GetColor(int i)
    {
        return instance.Colors[i];
    }

    public static Color GetRandomColor()
    {
        return instance.Colors[Random.Range(0, instance.Colors.Length)];
    }
}
