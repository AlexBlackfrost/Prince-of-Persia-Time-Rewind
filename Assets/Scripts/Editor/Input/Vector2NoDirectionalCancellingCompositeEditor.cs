using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Composites;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.InputSystem.Editor;
[InitializeOnLoad]
internal class Vector2NoDirectionalCancellingCompositeEditor : InputParameterEditor<Vector2NoDirectionalCancellingComposite> {

    static Vector2NoDirectionalCancellingCompositeEditor() {
        Initialize();
    }

    private GUIContent m_ModeLabel = new GUIContent("Mode",
        "How to create synthesize a Vector2 from the inputs. Digital "
        + "treats part bindings as buttons (on/off) whereas Analog preserves "
        + "floating-point magnitudes as read from controls.");

    public override void OnGUI() {
        target.mode = (Vector2NoDirectionalCancellingComposite.Mode)EditorGUILayout.EnumPopup(m_ModeLabel, target.mode);
        
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Initialize() {
        // Do not add whitespaces in composite name or it doesn't work
        InputSystem.RegisterBindingComposite<Vector2NoDirectionalCancellingComposite>("2DVectorCompositeNoDirectionalCancelling");
    }
}
#endif
