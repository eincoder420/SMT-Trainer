namespace Invector.vEventSystems;

public static class vIAnimatorStateInfoHelper
{
	public static void Register(this vIAnimatorStateInfoController animatorStateInfos)
	{
		if (animatorStateInfos.isValid())
		{
			animatorStateInfos.animatorStateInfos.RegisterListener();
		}
	}

	public static void UnRegister(this vIAnimatorStateInfoController animatorStateInfos)
	{
		if (animatorStateInfos.isValid())
		{
			animatorStateInfos.animatorStateInfos.RemoveListener();
		}
	}

	public static bool isValid(this vIAnimatorStateInfoController animatorStateInfos)
	{
		if (animatorStateInfos != null && animatorStateInfos.animatorStateInfos != null)
		{
			return animatorStateInfos.animatorStateInfos.animator != null;
		}
		return false;
	}
}
