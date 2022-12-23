using UnityEngine;
using System.Collections;
public static class ExtensionMethods
{
    public const string HORIZONTAL = "Horizontal";
    public const string VERITCAL = "Vertical";
    public const string MOUSEX = "Mouse X";
    public const string MOUSEY = "Mouse Y";
    public const string MOUSE_WHEEL = "Mouse ScrollWheel";
    public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
    {
        var component = obj.GetComponent<T>();
        if (component == null) component = obj.AddComponent<T>();
        return component;
    }
    public static T GetOrAddComponent<T>(this Component comp) where T : Component
    {
        var component = comp.GetComponent<T>();
        if (component == null) component = comp.gameObject.AddComponent<T>();
        return component;
    }
    public static ScriptableObject Clone(this ScriptableObject obj)
    {
        return Object.Instantiate(obj) as ScriptableObject;
    }
    public static T Clone<T>(this T obj) where T : ScriptableObject
    {
        return Object.Instantiate<T>(obj);
    }
    public static Vector3 GetMidpointTo(this Vector3 vect, Vector3 other)
    {
        var result = other - vect;
        result /= 2;
        result += vect;
        return result;
    }
}