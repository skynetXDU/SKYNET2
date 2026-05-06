using SKYNET;
using UnityEngine;

public class SDictionaryExample : MonoBehaviour {
    [InspectorLabel("人物表")]
    [SDictionaryLabel("名称", "人物")]
    public SDictionary<string, Character> characters;
}
