using UnityEngine;
using UnityEngine.Rendering;

public class Shadows_Control : MonoBehaviour
{
	private void Start()
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Renderer obj = componentsInChildren[i];
			int shadowCastingMode3;
			if (!componentsInChildren[i].GetComponent<Shadow_Caster>())
			{
				ShadowCastingMode shadowCastingMode2 = (componentsInChildren[i].shadowCastingMode = ShadowCastingMode.Off);
				shadowCastingMode3 = (int)shadowCastingMode2;
			}
			else
			{
				shadowCastingMode3 = 1;
			}
			obj.shadowCastingMode = (ShadowCastingMode)shadowCastingMode3;
		}
	}
}
