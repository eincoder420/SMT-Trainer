using System;
using UnityEngine;

namespace AmplifyBloom;

[Serializable]
[ImageEffectAllowedInSceneView]
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Amplify Bloom")]
public sealed class AmplifyBloomEffect : AmplifyBloomBase
{
}
