using UnityEngine;

public class UI_Model_Animated : MonoBehaviour
{
	public Animator actions_anim;

	public int Pose_id;

	public int Dance_id;

	private Roxanne_Control Rox;

	public int[] Poses_List;

	public Transform[] Positions;

	public Transform Root;

	private void Start()
	{
		Pose_id = 1;
		Dance_id = 1;
		Rox = Object.FindObjectOfType<Roxanne_Control>();
		Set_Root(0);
	}

	public void Set_Root(int Value)
	{
		if (Value == 1)
		{
			Root.position = Positions[Poses_List[Pose_id]].position;
			Root.rotation = Positions[Poses_List[Pose_id]].rotation;
		}
		if (Value == 2)
		{
			Root.position = Positions[1].position;
			Root.rotation = Positions[1].rotation;
		}
		if (Value == 3)
		{
			Root.position = Positions[2].position;
			Root.rotation = Positions[2].rotation;
		}
		if (Value == 0)
		{
			Root.position = Positions[1].position;
			Root.rotation = Positions[1].rotation;
		}
	}

	public void Choose_Dance(int id)
	{
		Dance_id = id;
		actions_anim.SetInteger("Animation_id", Dance_id);
	}

	public void Choose_Pose(int id)
	{
		Pose_id = id;
		actions_anim.SetInteger("Animation_id", Poses_List[Pose_id]);
		Rox.Mast_Block.color = ((!Rox.poses[Poses_List[Pose_id]].No_Requied_Unwear) ? new Color(0f, 0f, 0f, 0f) : new Color(0f, 0f, 0f, 0.5f));
	}

	public void Choose_And_Mark_Pose(int id)
	{
		Pose_id = id;
		actions_anim.SetInteger("Animation_id", Poses_List[Pose_id]);
		Rox.Mast_Block.color = ((!Rox.poses[Poses_List[Pose_id]].No_Requied_Unwear) ? new Color(0f, 0f, 0f, 0f) : new Color(0f, 0f, 0f, 0.5f));
	}
}
