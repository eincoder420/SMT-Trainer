namespace ECE;

public class CapsuleColliderData : SphereColliderData
{
	public int Direction;

	public float Height;

	public void Clone(CapsuleColliderData data)
	{
		Clone((SphereColliderData)data);
		Direction = data.Direction;
		Height = data.Height;
	}
}
