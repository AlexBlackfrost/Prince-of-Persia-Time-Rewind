using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif



#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(BlendShapeName))]
public class BlendShapeDrawer: PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        BlendShapeName blendShapeName = attribute as BlendShapeName;
        if(blendShapeName.methodName == "") {
            base.OnGUI(position, property, label);
            return;
        }
        object obj = GetParent(property);
        var method = obj.GetType().GetMethod(blendShapeName.methodName);
        string[] list = method.Invoke(obj, null) as string[];
        int savedIndex = Array.IndexOf(list, property.stringValue);
        int index = Mathf.Clamp(savedIndex, 0, list.Length);
        index = EditorGUI.Popup(position, property.displayName, index, list);
        property.stringValue = list[index];
    }

    public object GetParent(SerializedProperty prop) {
        var path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        var elements = path.Split('.');
        foreach (var element in elements.Take(elements.Length - 1)) {
            if (element.Contains("[")) {
                var elementName = element.Substring(0, element.IndexOf("["));
                var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                obj = GetValue(obj, elementName, index);
            } else {
                obj = GetValue(obj, element);
            }
        }
        return obj;
    }

    public object GetValue(object source, string name) {
        if (source == null) {
            return null;
        }
            
        var type = source.GetType();
        var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (f == null) {
            var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p == null) {
                return null;
            }
            return p.GetValue(source, null);
        }
        return f.GetValue(source);
    }

    public object GetValue(object source, string name, int index) {
        var enumerable = GetValue(source, name) as IEnumerable;
        var enm = enumerable.GetEnumerator();
        while (index-- >= 0) {
            enm.MoveNext();
        }
            
        return enm.Current;
    }
}
#endif
