using UnityEngine;

namespace SKYNET.Editor.Tests {
public sealed class OptionTestComponent : MonoBehaviour {
    [Option(1)] [Option(2)] public int value;
}
}
