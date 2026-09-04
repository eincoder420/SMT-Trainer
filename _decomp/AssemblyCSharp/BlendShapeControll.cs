using System.Collections;
using UnityEngine;

public class BlendShapeControll : MonoBehaviour
{
	public Animator animator;

	private void Start()
	{
		animator = GetComponent<Animator>();
		StartCoroutine(BlinkAndWait(Random.Range(2, 5)));
	}

	public IEnumerator BlinkAndWait(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		animator.SetBool("blink", value: true);
		yield return new WaitForSeconds(1f);
		animator.SetBool("blink", value: false);
		StartCoroutine(BlinkAndWait(Random.Range(2, 5)));
	}
}
