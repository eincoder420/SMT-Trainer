namespace Invector.IK;

public static class vWeaponIKAdjustHelper
{
	public static IKAdjust Copy(this IKAdjust iKAdjust)
	{
		return new IKAdjust
		{
			name = iKAdjust.name,
			spineOffset = iKAdjust.spineOffset.Copy(),
			supportHandOffset = iKAdjust.supportHandOffset.Copy(),
			supportHintOffset = iKAdjust.supportHintOffset.Copy(),
			weaponHandOffset = iKAdjust.weaponHandOffset.Copy(),
			weaponHintOffset = iKAdjust.weaponHintOffset.Copy()
		};
	}

	public static IKAdjust Copy(this IKAdjust iKAdjust, string name)
	{
		return new IKAdjust
		{
			name = name,
			spineOffset = iKAdjust.spineOffset.Copy(),
			supportHandOffset = iKAdjust.supportHandOffset.Copy(),
			supportHintOffset = iKAdjust.supportHintOffset.Copy(),
			weaponHandOffset = iKAdjust.weaponHandOffset.Copy(),
			weaponHintOffset = iKAdjust.weaponHintOffset.Copy()
		};
	}

	public static IKOffsetSpine Copy(this IKOffsetSpine iKOffsetSpine)
	{
		return new IKOffsetSpine
		{
			head = iKOffsetSpine.head,
			spine = iKOffsetSpine.spine
		};
	}

	public static IKOffsetTransform Copy(this IKOffsetTransform iKOffsetTransform)
	{
		return new IKOffsetTransform
		{
			position = iKOffsetTransform.position,
			eulerAngles = iKOffsetTransform.eulerAngles
		};
	}
}
