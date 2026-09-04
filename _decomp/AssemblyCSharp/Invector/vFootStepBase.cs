using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector;

[Serializable]
public abstract class vFootStepBase : MonoBehaviour
{
	public vAudioSurface defaultSurface;

	public List<vAudioSurface> customSurfaces;

	public virtual void SpawnSurfaceEffect(FootStepObject footStepObject)
	{
		if (footStepObject != null)
		{
			for (int i = 0; i < customSurfaces.Count; i++)
			{
				if (customSurfaces[i] != null && ContainsTexture(footStepObject.name, customSurfaces[i]))
				{
					customSurfaces[i].SpawnSurfaceEffect(footStepObject);
					return;
				}
			}
		}
		if (defaultSurface != null)
		{
			defaultSurface.SpawnSurfaceEffect(footStepObject);
		}
	}

	protected virtual bool ContainsTexture(string name, vAudioSurface surface)
	{
		for (int i = 0; i < surface.TextureOrMaterialNames.Count; i++)
		{
			if (name.Contains(surface.TextureOrMaterialNames[i]))
			{
				return true;
			}
		}
		return false;
	}

	public abstract void StepOnTerrain(FootStepObject footStepObject);

	public abstract void StepOnMesh(FootStepObject footStepObject);

	public abstract void PlayFootStepEffect();

	public virtual void PlayFootStep(AnimationEvent evt)
	{
	}

	public virtual void PlayFootStepLeft(AnimationEvent evt)
	{
	}

	public virtual void PlayFootStepRight(AnimationEvent evt)
	{
	}
}
