using UnityEngine;

public class vSetAnimatorLayerWeight : MonoBehaviour
{
	[Range(0f, 1f)]
	public float value;

	public int animatorLayerIndex;

	private void Start()
	{
		GetComponent<Animator>().SetLayerWeight(animatorLayerIndex, value);
	}
}
