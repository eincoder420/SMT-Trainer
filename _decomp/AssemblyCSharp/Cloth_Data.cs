using System;
using UnityEngine;

[Serializable]
public struct Cloth_Data
{
	public string Name;

	public bool Weared;

	public int Current_Variant;

	public Spawned_Cloth[] Spawned_Cloth;

	public Color main_color;

	public Color start_color;

	public Vector2 Tiling;

	public string Path_To_File;
}
