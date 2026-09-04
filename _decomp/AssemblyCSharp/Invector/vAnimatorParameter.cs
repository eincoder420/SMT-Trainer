using UnityEngine;

namespace Invector;

public class vAnimatorParameter
{
	private readonly AnimatorControllerParameter _parameter;

	public readonly bool isValid;

	public static implicit operator int(vAnimatorParameter a)
	{
		if (a.isValid)
		{
			return a._parameter.nameHash;
		}
		return -1;
	}

	public vAnimatorParameter(Animator animator, string parameter)
	{
		if ((bool)animator && animator.ContainsParam(parameter))
		{
			_parameter = animator.GetValidParameter(parameter);
			isValid = true;
		}
		else
		{
			isValid = false;
		}
	}
}
