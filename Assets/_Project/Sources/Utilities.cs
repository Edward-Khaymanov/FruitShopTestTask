using System;
using UnityEngine;

public static class Utilities
{
    public static Touch? GetTouchByFingerId(int fingerId)
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            var touch = Input.GetTouch(i);
            if (touch.fingerId == fingerId)
            {
                return touch;
            }
        }
        return null;
    }

    public static bool EnumInRange<T>(T value, params T[] values) where T : Enum
    {
        foreach (var v in values)
        {
            if (value.Equals(v))
                return true;
        }
        return false;
    }
}