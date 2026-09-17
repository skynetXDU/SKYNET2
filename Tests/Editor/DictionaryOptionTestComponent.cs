using UnityEngine;

namespace SKYNET.Editor.Tests {
public sealed class DictionaryOptionTestComponent : MonoBehaviour {
    [KeyOption("hp")] [ValueOption(100)] [ValueOption(200)]
    public SDictionary<string, int> stats = new();
}
}
