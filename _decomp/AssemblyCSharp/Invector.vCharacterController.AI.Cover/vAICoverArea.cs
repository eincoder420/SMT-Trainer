using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController.AI.Cover;

[SelectionBase]
[vClassHeader("AI Cover Area", true, "icon_v2", false, "", openClose = false)]
public class vAICoverArea : vMonoBehaviour
{
	[Serializable]
	public class CoverLine
	{
		public Transform p1;

		public Transform p2;

		public bool inverse;

		public List<vAICoverPoint> coverPoints = new List<vAICoverPoint>();

		public Vector3 forward;
	}

	public string coverLayer = "Triggers";

	public string coverTag = "CoverPoint";

	[vHelpBox("Collider Settings", vHelpBoxAttribute.MessageType.None)]
	public float colliderWidth = 1f;

	public float colliderHeight = 1f;

	public float colliderThickness = 0.5f;

	[SerializeField]
	protected float _colliderCenterY;

	[SerializeField]
	protected float _colliderCenterZ;

	[vHelpBox("Character Destination Settings", vHelpBoxAttribute.MessageType.None)]
	public bool drawTargetPosition;

	[HideInInspector]
	public bool closeLine;

	[HideInInspector]
	public List<CoverLine> coverLines = new List<CoverLine>();

	public float centerY => _colliderCenterY + colliderHeight * 0.5f;

	public float centerZ => _colliderCenterZ + colliderThickness * 0.5f;

	private void Reset()
	{
		for (int num = base.transform.childCount - 1; num > 0; num--)
		{
			UnityEngine.Object.DestroyImmediate(base.transform.GetChild(num).gameObject);
		}
	}

	private void Start()
	{
		int childCount = base.transform.childCount;
		int layer = LayerMask.NameToLayer(coverLayer);
		for (int i = 0; i < childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			child.gameObject.layer = layer;
			child.gameObject.tag = coverTag;
		}
	}

	private void OnDrawGizmos()
	{
	}
}
