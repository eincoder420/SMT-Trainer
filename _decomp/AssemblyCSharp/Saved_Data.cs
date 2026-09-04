using System;
using UnityEngine;

[Serializable]
public struct Saved_Data
{
	public int Spawn_position_id;

	public int Inside_Building;

	public Jerk_Places_Data Jerk_Places;

	public Dance_Places_Data Dance_Places;

	public Toys_Places_Data Toys_Places;

	public Sex_Places_Data Sex_Places;

	public Vector3 Sam_Position;

	public Vector3 Sam_Rotation;

	public Vector3 Interior_Out_Position;

	public Vector3 Interior_Out_Rotation;
}
