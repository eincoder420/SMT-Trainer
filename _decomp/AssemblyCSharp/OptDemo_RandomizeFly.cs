using FIMSpace.Basics;
using UnityEngine;

public class OptDemo_RandomizeFly : MonoBehaviour
{
	public FBasic_FlyMovement flyMovement;

	public Vector2 rangeFromTo = new Vector2(20f, 100f);

	public Vector2 speedfromTo = new Vector2(0.5f, 1.5f);

	private void Start()
	{
		flyMovement.RangeMul = Random.Range(rangeFromTo.x, rangeFromTo.y);
		flyMovement.MainSpeed = flyMovement.RangeMul / 150f * Random.Range(speedfromTo.x, speedfromTo.y);
	}
}
