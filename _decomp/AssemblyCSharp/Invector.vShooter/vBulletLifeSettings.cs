using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vShooter;

[CreateAssetMenu(menuName = "Invector/Shooter/New BulletLifeSettings")]
public class vBulletLifeSettings : ScriptableObject
{
	[Serializable]
	public class vBulletLostLife
	{
		public LayerMask layers = 1;

		public List<string> tags = new List<string> { "Untagged" };

		public int reduceLife = 100;

		public bool ricochet;

		[vHideInInspector("ricochet", true)]
		public float maxThicknessToCross = 0.2f;

		[Range(0f, 100f)]
		public int damageReducePercentage = 50;

		[Range(0f, 90f)]
		public float minChangeTrajectory = 2f;

		[Range(0f, 90f)]
		public float maxChangeTrajectory = 2f;

		public vBulletLostLife()
		{
			layers = 1;
			tags = new List<string> { "Untagged" };
			reduceLife = 100;
		}
	}

	public struct vBulletLifeInfo
	{
		public int lostLife;

		public int lostDamage;

		public float minChangeTrajectory;

		public float maxChangeTrajectory;

		public float maxThicknessToCross;

		public bool ricochet;
	}

	public List<vBulletLostLife> bulletLostLifeList;

	private bool seedGenerated;

	public vBulletLifeInfo GetReduceLife(string tag, int layer)
	{
		vBulletLostLife vBulletLostLife = bulletLostLifeList.Find((vBulletLostLife blf) => isValid(blf, tag, layer));
		vBulletLifeInfo result = default(vBulletLifeInfo);
		if (vBulletLostLife != null)
		{
			result.lostLife = vBulletLostLife.reduceLife;
			result.lostDamage = vBulletLostLife.damageReducePercentage;
			result.minChangeTrajectory = vBulletLostLife.minChangeTrajectory;
			result.maxChangeTrajectory = vBulletLostLife.maxChangeTrajectory;
			result.maxThicknessToCross = vBulletLostLife.maxThicknessToCross;
			result.ricochet = vBulletLostLife.ricochet;
		}
		return result;
	}

	private bool isValid(vBulletLostLife blf, string tag, int layer)
	{
		if ((int)blf.layers == ((int)blf.layers | (1 << layer)))
		{
			return blf.tags.Contains(tag);
		}
		return false;
	}
}
