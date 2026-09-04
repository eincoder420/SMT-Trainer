using Invector;
using UnityEngine;

public class vSnapToBody : MonoBehaviour
{
	public const string manuallyAssignBone = "ManuallyAssign";

	public vBodySnappingControl bodySnap;

	public Transform boneToSnap;

	public string boneName;

	private void Start()
	{
		bodySnap = base.transform.root.GetComponentInChildren<vBodySnappingControl>(includeInactive: true);
		if (boneName != "ManuallyAssign" && bodySnap != null && bodySnap.boneSnappingList != null)
		{
			boneToSnap = bodySnap.GetBone(boneName);
		}
		if ((bool)boneToSnap)
		{
			base.transform.parent = boneToSnap;
		}
	}
}
