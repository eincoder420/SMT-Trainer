using System;
using System.Reflection;
using UnityEngine;

namespace RootMotion;

public class AvatarUtility
{
	public static Quaternion GetPostRotation(Avatar avatar, AvatarIKGoal avatarIKGoal)
	{
		int num = (int)HumanIDFromAvatarIKGoal(avatarIKGoal);
		if (num == 55)
		{
			throw new InvalidOperationException("Invalid human id.");
		}
		MethodInfo method = typeof(Avatar).GetMethod("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
		if (method == null)
		{
			throw new InvalidOperationException("Cannot find GetPostRotation method.");
		}
		return (Quaternion)method.Invoke(avatar, new object[1] { num });
	}

	public static TQ GetIKGoalTQ(Avatar avatar, float humanScale, AvatarIKGoal avatarIKGoal, TQ bodyPositionRotation, TQ boneTQ)
	{
		int num = (int)HumanIDFromAvatarIKGoal(avatarIKGoal);
		if (num == 55)
		{
			throw new InvalidOperationException("Invalid human id.");
		}
		MethodInfo method = typeof(Avatar).GetMethod("GetAxisLength", BindingFlags.Instance | BindingFlags.NonPublic);
		if (method == null)
		{
			throw new InvalidOperationException("Cannot find GetAxisLength method.");
		}
		MethodInfo method2 = typeof(Avatar).GetMethod("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
		if (method2 == null)
		{
			throw new InvalidOperationException("Cannot find GetPostRotation method.");
		}
		Quaternion quaternion = (Quaternion)method2.Invoke(avatar, new object[1] { num });
		TQ tQ = new TQ(boneTQ.t, boneTQ.q * quaternion);
		if (avatarIKGoal == AvatarIKGoal.LeftFoot || avatarIKGoal == AvatarIKGoal.RightFoot)
		{
			float x = (float)method.Invoke(avatar, new object[1] { num });
			Vector3 vector = new Vector3(x, 0f, 0f);
			tQ.t += tQ.q * vector;
		}
		Quaternion quaternion2 = Quaternion.Inverse(bodyPositionRotation.q);
		tQ.t = quaternion2 * (tQ.t - bodyPositionRotation.t);
		tQ.q = quaternion2 * tQ.q;
		tQ.t /= humanScale;
		tQ.q = Quaternion.LookRotation(tQ.q * Vector3.forward, tQ.q * Vector3.up);
		return tQ;
	}

	public static TQ WorldSpaceIKGoalToBone(TQ goalTQ, Avatar avatar, AvatarIKGoal avatarIKGoal)
	{
		int num = (int)HumanIDFromAvatarIKGoal(avatarIKGoal);
		if (num == 55)
		{
			throw new InvalidOperationException("Invalid human id.");
		}
		MethodInfo method = typeof(Avatar).GetMethod("GetAxisLength", BindingFlags.Instance | BindingFlags.NonPublic);
		if (method == null)
		{
			throw new InvalidOperationException("Cannot find GetAxisLength method.");
		}
		MethodInfo method2 = typeof(Avatar).GetMethod("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
		if (method2 == null)
		{
			throw new InvalidOperationException("Cannot find GetPostRotation method.");
		}
		Quaternion rotation = (Quaternion)method2.Invoke(avatar, new object[1] { num });
		if (avatarIKGoal == AvatarIKGoal.LeftFoot || avatarIKGoal == AvatarIKGoal.RightFoot)
		{
			float x = (float)method.Invoke(avatar, new object[1] { num });
			Vector3 vector = new Vector3(x, 0f, 0f);
			goalTQ.t -= goalTQ.q * vector;
		}
		return new TQ(goalTQ.t, goalTQ.q * Quaternion.Inverse(rotation));
	}

	public static TQ GetWorldSpaceIKGoal(BakerHumanoidQT ikQT, BakerHumanoidQT rootQT, float time, float humanScale)
	{
		TQ tQ = ikQT.Evaluate(time);
		TQ tQ2 = rootQT.Evaluate(time);
		tQ.q = tQ2.q * tQ.q;
		tQ.t = tQ2.t + tQ2.q * tQ.t;
		tQ.t *= humanScale;
		return tQ;
	}

	public static HumanBodyBones HumanIDFromAvatarIKGoal(AvatarIKGoal avatarIKGoal)
	{
		return avatarIKGoal switch
		{
			AvatarIKGoal.LeftFoot => HumanBodyBones.LeftFoot, 
			AvatarIKGoal.RightFoot => HumanBodyBones.RightFoot, 
			AvatarIKGoal.LeftHand => HumanBodyBones.LeftHand, 
			AvatarIKGoal.RightHand => HumanBodyBones.RightHand, 
			_ => HumanBodyBones.LastBone, 
		};
	}
}
