using UnityEngine;

public class BlendShapeName : PropertyAttribute {
    public string methodName = "";
    public BlendShapeName(string methodName) {
        this.methodName = methodName;
    }
}