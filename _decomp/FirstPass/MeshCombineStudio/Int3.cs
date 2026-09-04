namespace MeshCombineStudio;

public struct Int3
{
	public int x;

	public int y;

	public int z;

	public Int3(int x, int y, int z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public static Int3 operator +(Int3 a, Int3 b)
	{
		Int3 result = default(Int3);
		result.x = a.x + b.x;
		result.y = a.y + b.y;
		result.z = a.z + b.z;
		return result;
	}
}
