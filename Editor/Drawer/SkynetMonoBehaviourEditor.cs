using UnityEditor;
using UnityEngine;

namespace SKYNET.Editor {

[CustomEditor(typeof(MonoBehaviour), true, isFallback = true)]
[CanEditMultipleObjects]
public class SkynetMonoBehaviourEditor : UnityEditor.Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        ButtonDrawer.Draw(targets);
    }
}
}
