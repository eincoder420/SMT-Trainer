using UnityEngine;

public class Set_Parent_Start : MonoBehaviour
{
	public Transform parent;

	public Vector3 Scale = new Vector3(1f, 1f, 1f);

	public bool Cloth;

	private void Start()
	{
		if (!Cloth)
		{
			SetParent();
		}
	}

	public void SetParent()
	{
		base.transform.parent = parent;
		string text = base.transform.name;
		base.transform.name = text + " new";
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		base.transform.localScale = Scale;
	}

	public void Set_Unwear_Cloth_Parent()
	{
		Vector3 localPosition = base.transform.localPosition;
		Quaternion localRotation = base.transform.localRotation;
		Vector3 localScale = base.transform.localScale;
		base.transform.parent = parent;
		base.transform.localPosition = localPosition;
		base.transform.localRotation = localRotation;
		base.transform.localScale = localScale;
	}
}
