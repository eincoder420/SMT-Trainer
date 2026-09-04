using UnityEngine;

namespace MeshCombineStudio;

public class BaseOctree
{
	public class Cell
	{
		public Cell mainParent;

		public Cell parent;

		public bool[] cellsUsed;

		public Bounds bounds;

		public int cellIndex;

		public int cellCount;

		public int level;

		public int maxLevels;

		public Cell()
		{
		}

		public Cell(Vector3 position, Vector3 size, int maxLevels)
		{
			bounds = new Bounds(position, size);
			this.maxLevels = maxLevels;
		}

		public Cell(Cell parent, int cellIndex, Bounds bounds)
		{
			if (parent != null)
			{
				maxLevels = parent.maxLevels;
				mainParent = parent.mainParent;
				level = parent.level + 1;
			}
			this.parent = parent;
			this.cellIndex = cellIndex;
			this.bounds = bounds;
		}

		public void SetCell(Cell parent, int cellIndex, Bounds bounds)
		{
			if (parent != null)
			{
				maxLevels = parent.maxLevels;
				mainParent = parent.mainParent;
				level = parent.level + 1;
			}
			this.parent = parent;
			this.cellIndex = cellIndex;
			this.bounds = bounds;
		}

		protected int AddCell<T, U>(ref T[] cells, Vector3 position, out bool maxCellCreated) where T : Cell, new() where U : Cell, new()
		{
			Vector3 vector = position - bounds.min;
			int num = (int)(vector.x / bounds.extents.x);
			int num2 = (int)(vector.y / bounds.extents.y);
			int num3 = (int)(vector.z / bounds.extents.z);
			int num4 = num + num2 * 4 + num3 * 2;
			AddCell<T, U>(ref cells, num4, num, num2, num3, out maxCellCreated);
			return num4;
		}

		protected T GetCell<T>(T[] cells, Vector3 position)
		{
			if (cells == null)
			{
				return default(T);
			}
			Vector3 vector = position - bounds.min;
			int num = (int)(vector.x / bounds.extents.x);
			int num2 = (int)(vector.y / bounds.extents.y);
			int num3 = (int)(vector.z / bounds.extents.z);
			int num4 = num + num2 * 4 + num3 * 2;
			return cells[num4];
		}

		protected void AddCell<T, U>(ref T[] cells, int index, int x, int y, int z, out bool maxCellCreated) where T : Cell, new() where U : Cell, new()
		{
			if (cells == null)
			{
				cells = new T[8];
			}
			if (cellsUsed == null)
			{
				cellsUsed = new bool[8];
			}
			if (!cellsUsed[index])
			{
				Bounds bounds = new Bounds(new Vector3(this.bounds.min.x + this.bounds.extents.x * ((float)x + 0.5f), this.bounds.min.y + this.bounds.extents.y * ((float)y + 0.5f), this.bounds.min.z + this.bounds.extents.z * ((float)z + 0.5f)), this.bounds.extents);
				if (level == maxLevels - 1)
				{
					cells[index] = new U() as T;
					cells[index].SetCell(this, index, bounds);
					maxCellCreated = true;
				}
				else
				{
					maxCellCreated = false;
					cells[index] = new T();
					cells[index].SetCell(this, index, bounds);
				}
				cellsUsed[index] = true;
				cellCount++;
			}
			else
			{
				maxCellCreated = false;
			}
		}

		public bool InsideBounds(Vector3 position)
		{
			position -= bounds.min;
			if (position.x >= bounds.size.x || position.y >= bounds.size.y || position.z >= bounds.size.z || position.x <= 0f || position.y <= 0f || position.z <= 0f)
			{
				return false;
			}
			return true;
		}

		public void Reset(ref Cell[] cells)
		{
			cells = null;
			cellsUsed = null;
		}
	}
}
