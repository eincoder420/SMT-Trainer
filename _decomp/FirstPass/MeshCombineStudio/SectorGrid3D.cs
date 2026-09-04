using UnityEngine;

namespace MeshCombineStudio;

public class SectorGrid3D<T>
{
	public FastIndexList<Sector3D<T>> sectorList = new FastIndexList<Sector3D<T>>();

	public Sector3D<T>[,,] sectors;

	public Rect rect;

	public Int3 sectorCount;

	public Vector3 sectorGridOffset;

	public Vector3 sectorSize;

	public Vector3 halfSectorSize;

	public Vector3 invSectorSize;

	public Vector3 totalSize;

	public Vector3 halfTotalSize;

	public SectorGrid3D(Int3 sectorCount, Vector3 sectorSize, Vector3 sectorGridOffset)
	{
		sectors = new Sector3D<T>[sectorCount.x, sectorCount.y, sectorCount.z];
		this.sectorCount = sectorCount;
		this.sectorSize = sectorSize;
		this.sectorGridOffset = sectorGridOffset;
		invSectorSize = Mathw.Divide(1f, sectorSize);
		halfSectorSize = sectorSize / 2f;
		totalSize = Mathw.Scale(sectorSize, sectorCount);
		halfTotalSize = totalSize * 0.5f;
		rect = new Rect(sectorGridOffset - halfTotalSize, totalSize);
	}

	public void GetSectors(FastList<Sector3D<T>> list, Vector3 pos, float radius)
	{
		list.FastClear();
		Int3 sectorIndex = GetSectorIndex(new Vector3(pos.x - radius, pos.y - radius, pos.z - radius));
		Int3 sectorIndex2 = GetSectorIndex(new Vector3(pos.x + radius, pos.y + radius, pos.z + radius));
		for (int i = sectorIndex.z; i < sectorIndex2.z; i++)
		{
			for (int j = sectorIndex.y; j <= sectorIndex2.y; j++)
			{
				for (int k = sectorIndex.x; k <= sectorIndex2.x; k++)
				{
					if (sectors[k, j, i] != null)
					{
						list.Add(sectors[k, j, i]);
					}
				}
			}
		}
	}

	public void GetOrCreateSector(Vector3 pos, out Sector3D<T> sector)
	{
		Int3 s = GetSectorIndex(pos);
		sector = sectors[s.x, s.y, s.z];
		if (sector == null)
		{
			sector = CreateSector(ref s);
		}
	}

	public Int3 GetSectorIndex(Vector3 pos)
	{
		pos += -sectorGridOffset + halfTotalSize + halfSectorSize;
		pos.x *= invSectorSize.x;
		pos.y *= invSectorSize.y;
		pos.z *= invSectorSize.z;
		return new Int3((int)pos.x, (int)pos.y, (int)pos.z);
	}

	public Sector3D<T> GetSector(Vector3 pos)
	{
		Int3 sectorIndex = GetSectorIndex(pos);
		return sectors[sectorIndex.x, sectorIndex.y, sectorIndex.z];
	}

	public Sector3D<T> CreateSector(ref Int3 s)
	{
		Sector3D<T> sector3D = new Sector3D<T>();
		sector3D.bounds = new Bounds(new Vector3((float)s.x * sectorSize.x, (float)s.y * sectorSize.y, (float)s.z * sectorSize.z) + (sectorGridOffset - halfTotalSize), sectorSize);
		sectors[s.x, s.y, s.z] = sector3D;
		sectorList.Add(sector3D);
		return sector3D;
	}

	public void RemoveSector(Vector3 pos)
	{
		Int3 sectorIndex = GetSectorIndex(pos);
		sectorList.Remove(sectors[sectorIndex.x, sectorIndex.y, sectorIndex.z]);
		sectors[sectorIndex.x, sectorIndex.y, sectorIndex.z] = null;
	}

	public void RemoveSector(Int3 sectorIndex)
	{
		sectorList.Remove(sectors[sectorIndex.x, sectorIndex.y, sectorIndex.z]);
		sectors[sectorIndex.x, sectorIndex.y, sectorIndex.z] = null;
	}

	public void Reset()
	{
		sectors = new Sector3D<T>[sectorCount.y, sectorCount.x, sectorCount.z];
		sectorList.Clear();
	}

	public void Draw()
	{
		DrawSectors(sectorList, Color.white);
	}

	public void DrawSectors(FastList<Sector3D<T>> sectors, Color color)
	{
		Gizmos.color = color;
		for (int i = 0; i < sectors.Count; i++)
		{
			Bounds bounds = sectors.items[i].bounds;
			Gizmos.DrawWireCube(bounds.center, bounds.size);
		}
	}
}
