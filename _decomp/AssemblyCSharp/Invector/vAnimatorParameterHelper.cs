using UnityEngine;

namespace Invector;

public static class vAnimatorParameterHelper
{
	public static AnimatorControllerParameter GetValidParameter(this Animator _Anim, string _ParamName)
	{
		AnimatorControllerParameter[] parameters = _Anim.parameters;
		foreach (AnimatorControllerParameter animatorControllerParameter in parameters)
		{
			if (animatorControllerParameter.name == _ParamName)
			{
				return animatorControllerParameter;
			}
		}
		return null;
	}

	public static bool ContainsParam(this Animator _Anim, string _ParamName)
	{
		AnimatorControllerParameter[] parameters = _Anim.parameters;
		for (int i = 0; i < parameters.Length; i++)
		{
			if (parameters[i].name == _ParamName)
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasParameterOfType(this Animator self, string name, AnimatorControllerParameterType type)
	{
		if (null == self)
		{
			return false;
		}
		AnimatorControllerParameter[] parameters = self.parameters;
		foreach (AnimatorControllerParameter animatorControllerParameter in parameters)
		{
			if (animatorControllerParameter.type == type && animatorControllerParameter.name == name)
			{
				return true;
			}
		}
		return false;
	}
}
