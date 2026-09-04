using System.Collections;
using UnityEngine;

namespace Invector.vShooter;

[CreateAssetMenu(menuName = "Invector/Effects/New  Custom ImpactEffect", fileName = "CustomImpactEffect@")]
public class vCustomImpactEffectSample : vImpactEffectBase
{
	public enum Align
	{
		Right,
		Forward,
		UP,
		Left,
		Back,
		Down
	}

	public class Fade : MonoBehaviour
	{
		public void InitFade(Renderer renderer, float fadeSpeed)
		{
			StartCoroutine(FadeColor(renderer, fadeSpeed));
		}

		private IEnumerator FadeColor(Renderer renderer, float fadeSpeed)
		{
			float value2 = 0f;
			while (value2 < 1f)
			{
				renderer.material.color = Color.Lerp(renderer.material.color, Color.clear, value2);
				value2 += fadeSpeed * Time.deltaTime;
				value2 = Mathf.Clamp(value2, 0f, 1f);
				yield return null;
			}
			Object.Destroy(base.gameObject);
		}
	}

	public Mesh mesh;

	public float size = 0.02f;

	public float margin = 0.01f;

	public float fadeSpeed = 0.1f;

	public Align alignTransform;

	[ColorUsage(true, true)]
	public Color color;

	public Material material;

	public override void DoImpactEffect(Vector3 position, Quaternion rotation, GameObject sender, GameObject receiver)
	{
		Vector3 vector = rotation * Vector3.forward;
		GameObject gameObject = new GameObject();
		gameObject.transform.position = position + vector * margin;
		switch (alignTransform)
		{
		case Align.Right:
			gameObject.transform.right = vector;
			break;
		case Align.Forward:
			gameObject.transform.forward = vector;
			break;
		case Align.UP:
			gameObject.transform.up = vector;
			break;
		case Align.Left:
			gameObject.transform.right = -vector;
			break;
		case Align.Back:
			gameObject.transform.forward = -vector;
			break;
		case Align.Down:
			gameObject.transform.up = -vector;
			break;
		}
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.material = material;
		meshRenderer.material.color = color;
		gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
		gameObject.transform.localScale = Vector3.one * size;
		gameObject.transform.SetParent(vObjectContainer.root, worldPositionStays: true);
		gameObject.AddComponent<Fade>().InitFade(meshRenderer, fadeSpeed);
	}
}
