using UnityEngine;

namespace Invector.vItemManager;

public class vLeaveDropItemExample : MonoBehaviour
{
	private vItemManager itemManager;

	private Rect windowRect;

	private Vector2 scroll;

	private void OnGUI()
	{
		windowRect = GUILayout.Window(0, windowRect, vLeaveDropItensWindow, "Leave and Drop Items test by Invector:");
	}

	private void vLeaveDropItensWindow(int windowID)
	{
		GUILayout.BeginVertical();
		itemManager = Object.FindObjectOfType<vItemManager>();
		if ((bool)itemManager)
		{
			scroll = GUILayout.BeginScrollView(scroll, GUILayout.Width(300f), GUILayout.MinHeight(300f));
			for (int i = 0; i < itemManager.items.Count; i++)
			{
				GUILayout.BeginHorizontal("box");
				GUILayout.Label(new GUIContent("Name:" + itemManager.items[i].name + "\nAmount :" + itemManager.items[i].amount), GUILayout.Width(200f), GUILayout.Height(40f));
				GUILayout.BeginVertical("box");
				if (GUILayout.Button("Leave") || GUILayout.Button("Drop"))
				{
					break;
				}
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();
		}
		GUILayout.EndVertical();
		GUI.DragWindow();
	}
}
