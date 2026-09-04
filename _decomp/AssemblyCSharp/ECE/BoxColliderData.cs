using UnityEngine;

namespace ECE;

public class BoxColliderData : EasyColliderData
{
	public Vector3 Center;

	public Vector3 Size;

	public void Clone(BoxColliderData data)
	{
		Clone((EasyColliderData)data);
		Center = data.Center;
		Size = data.Size;
		Matrix = data.Matrix;
	}

	public override string ToString()
	{
		return "Rotated box collider. Center:" + Center.ToString() + " Size:" + Size.ToString();
	}
}
