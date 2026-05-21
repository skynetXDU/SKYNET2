using UnityEditor;
using UnityEngine;

namespace SKYNET.Editor {

[CustomEditor(typeof(ScriptableObject), true, isFallback = true)]
[CanEditMultipleObjects]
public class SkynetScriptableObjectEditor : UnityEditor.Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        ButtonDrawer.Draw(targets);
    }
}
}
