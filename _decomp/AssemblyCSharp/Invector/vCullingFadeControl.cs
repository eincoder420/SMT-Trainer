using System.Collections.Generic;
using UnityEngine;

namespace Invector;

public class vCullingFadeControl : MonoBehaviour
{
	public float distanceToStartFade = 0.55f;

	public float distanceToEndFade = 0.4f;

	public Vector3 offset = new Vector3(0f, 1.3f, 0f);

	public List<FadeMaterials> fadeMeshRenderers;

	public List<FadeMaterials> fadeSkinnedMeshRenderes;

	public bool usingTransp;

	public Transform targetObject => base.transform;

	public Transform cameraTransform
	{
		get
		{
			Transform transform = base.transform;
			if (Camera.main != null)
			{
				transform = Camera.main.transform;
			}
			if (transform == base.transform)
			{
				Debug.LogWarning("Invector : Missing MainCamera");
				base.enabled = false;
			}
			return transform;
		}
	}

	private void Start()
	{
		Init();
	}

	public void Init()
	{
		foreach (FadeMaterials fadeMeshRenderer in fadeMeshRenderers)
		{
			fadeMeshRenderer.originalAlpha = new float[fadeMeshRenderer.originalMaterials.Length];
			for (int i = 0; i < fadeMeshRenderer.originalMaterials.Length; i++)
			{
				if (fadeMeshRenderer.fadeMaterials[i] == null)
				{
					try
					{
						fadeMeshRenderer.originalAlpha[i] = fadeMeshRenderer.originalMaterials[i].color.a;
						fadeMeshRenderer.fadeMaterials[i] = fadeMeshRenderer.originalMaterials[i];
					}
					catch
					{
					}
				}
				else
				{
					try
					{
						fadeMeshRenderer.originalAlpha[i] = fadeMeshRenderer.fadeMaterials[i].color.a;
					}
					catch
					{
					}
				}
			}
		}
		foreach (FadeMaterials fadeSkinnedMeshRendere in fadeSkinnedMeshRenderes)
		{
			fadeSkinnedMeshRendere.originalAlpha = new float[fadeSkinnedMeshRendere.originalMaterials.Length];
			for (int j = 0; j < fadeSkinnedMeshRendere.originalMaterials.Length; j++)
			{
				if (fadeSkinnedMeshRendere.fadeMaterials[j] == null)
				{
					try
					{
						fadeSkinnedMeshRendere.originalAlpha[j] = fadeSkinnedMeshRendere.originalMaterials[j].color.a;
						fadeSkinnedMeshRendere.fadeMaterials[j] = fadeSkinnedMeshRendere.originalMaterials[j];
					}
					catch
					{
					}
				}
				else
				{
					try
					{
						fadeSkinnedMeshRendere.originalAlpha[j] = fadeSkinnedMeshRendere.fadeMaterials[j].color.a;
					}
					catch
					{
					}
				}
			}
		}
	}

	private void LateUpdate()
	{
		UpdateEffect();
		if (usingTransp)
		{
			ChangeAlphaFromDistance();
		}
	}

	private void UpdateEffect()
	{
		float num = Vector3.Distance(cameraTransform.position, targetObject.position + offset);
		if (num < distanceToStartFade && !usingTransp)
		{
			usingTransp = true;
			ChangeMaterialsToFade();
		}
		else if (usingTransp && num > distanceToStartFade)
		{
			usingTransp = false;
			ChangeMaterialsToOriginal();
		}
	}

	private void ChangeMaterialsToOriginal()
	{
		foreach (FadeMaterials fadeMeshRenderer in fadeMeshRenderers)
		{
			try
			{
				fadeMeshRenderer.renderer.sharedMaterials = fadeMeshRenderer.originalMaterials;
			}
			catch
			{
			}
		}
		foreach (FadeMaterials fadeSkinnedMeshRendere in fadeSkinnedMeshRenderes)
		{
			try
			{
				fadeSkinnedMeshRendere.renderer.sharedMaterials = fadeSkinnedMeshRendere.originalMaterials;
			}
			catch
			{
			}
		}
	}

	private void ChangeMaterialsToFade()
	{
		foreach (FadeMaterials fadeMeshRenderer in fadeMeshRenderers)
		{
			try
			{
				fadeMeshRenderer.renderer.sharedMaterials = fadeMeshRenderer.fadeMaterials;
			}
			catch
			{
			}
		}
		foreach (FadeMaterials fadeSkinnedMeshRendere in fadeSkinnedMeshRenderes)
		{
			try
			{
				fadeSkinnedMeshRendere.renderer.sharedMaterials = fadeSkinnedMeshRendere.fadeMaterials;
			}
			catch
			{
			}
		}
	}

	public void ChangeAlphaFromDistance()
	{
		float num = Vector3.Distance(cameraTransform.position, targetObject.position + offset);
		for (int i = 0; i < fadeMeshRenderers.Count; i++)
		{
			for (int j = 0; j < fadeMeshRenderers[i].fadeMaterials.Length; j++)
			{
				try
				{
					float num2 = fadeMeshRenderers[i].originalAlpha[j] / (distanceToStartFade - distanceToEndFade);
					Color color = fadeMeshRenderers[i].renderer.sharedMaterials[j].color;
					float num3 = distanceToStartFade - distanceToEndFade - (distanceToStartFade - num);
					color.a = num2 * num3;
					color.a = Mathf.Clamp(color.a, 0f, fadeMeshRenderers[i].originalAlpha[j]);
					fadeMeshRenderers[i].renderer.materials[j].color = color;
				}
				catch
				{
				}
			}
		}
		for (int k = 0; k < fadeSkinnedMeshRenderes.Count; k++)
		{
			for (int l = 0; l < fadeSkinnedMeshRenderes[k].fadeMaterials.Length; l++)
			{
				try
				{
					float num4 = fadeSkinnedMeshRenderes[k].originalAlpha[l] / (distanceToStartFade - distanceToEndFade);
					Color color2 = fadeSkinnedMeshRenderes[k].renderer.sharedMaterials[l].color;
					float num5 = distanceToStartFade - distanceToEndFade - (distanceToStartFade - num);
					color2.a = num4 * num5;
					color2.a = Mathf.Clamp(color2.a, 0f, fadeSkinnedMeshRenderes[k].originalAlpha[l]);
					fadeSkinnedMeshRenderes[k].renderer.materials[l].color = color2;
				}
				catch
				{
				}
			}
		}
	}
}
