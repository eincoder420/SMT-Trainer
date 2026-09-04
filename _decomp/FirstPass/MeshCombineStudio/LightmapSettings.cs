using UnityEngine;

namespace MeshCombineStudio;

[ExecuteInEditMode]
public class LightmapSettings : MonoBehaviour
{
	public MeshRenderer mr;

	public int lightmapIndex;

	public bool setLigthmapScaleOffset;

	public Vector4 lightmapScaleOffset;

	private void OnEnable()
	{
		if ((bool)mr)
		{
			mr.lightmapIndex = lightmapIndex;
			if (setLigthmapScaleOffset)
			{
				mr.lightmapScaleOffset = lightmapScaleOffset;
			}
		}
	}
}
