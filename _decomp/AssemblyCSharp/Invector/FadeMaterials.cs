using System;
using UnityEngine;

namespace Invector;

[Serializable]
public class FadeMaterials
{
	public Renderer renderer;

	public Material[] originalMaterials;

	public Material[] fadeMaterials;

	public float[] originalAlpha;
}
