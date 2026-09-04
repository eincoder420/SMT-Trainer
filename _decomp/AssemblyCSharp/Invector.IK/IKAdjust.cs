using System;
using UnityEngine;

namespace Invector.IK;

[Serializable]
public class IKAdjust
{
	public string name;

	[ContextMenuItem("Copy", "ResetBiography")]
	public IKOffsetTransform weaponHandOffset = new IKOffsetTransform();

	public IKOffsetTransform weaponHintOffset = new IKOffsetTransform();

	public IKOffsetTransform supportHandOffset = new IKOffsetTransform();

	public IKOffsetTransform supportHintOffset = new IKOffsetTransform();

	public IKOffsetSpine spineOffset = new IKOffsetSpine();

	public IKAdjust()
	{
	}

	public IKAdjust(string name)
	{
		this.name = name;
	}
}
