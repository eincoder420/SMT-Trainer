using System.Collections.Generic;
using UnityEngine;

namespace MeshCombineStudio;

public class SwapCombineKey : MonoBehaviour
{
	public static SwapCombineKey instance;

	public List<MeshCombiner> meshCombinerList = new List<MeshCombiner>();

	private MeshCombiner meshCombiner;

	private GUIStyle textStyle;

	private void Awake()
	{
		instance = this;
		meshCombiner = GetComponent<MeshCombiner>();
		meshCombinerList.Add(meshCombiner);
		QualitySettings.vSyncCount = 0;
	}

	private void OnDestroy()
	{
		instance = null;
	}

	private void Update()
	{
		if (!Input.GetKeyDown(KeyCode.Tab))
		{
			return;
		}
		for (int i = 0; i < meshCombinerList.Count; i++)
		{
			if (meshCombinerList[i].meshCombineJobs.Count > 0)
			{
				return;
			}
		}
		for (int j = 0; j < meshCombinerList.Count; j++)
		{
			meshCombinerList[j].SwapCombine();
		}
	}

	private void OnGUI()
	{
		if (textStyle == null)
		{
			textStyle = new GUIStyle("label");
			textStyle.fontStyle = FontStyle.Bold;
			textStyle.fontSize = 16;
		}
		textStyle.normal.textColor = ((this.meshCombiner.combinedActive && this.meshCombiner.combined) ? Color.green : Color.red);
		int num = 0;
		GUI.Box(new Rect(5f, 30f, 310f, 40 + meshCombinerList.Count * 22), GUIContent.none);
		for (int i = 0; i < meshCombinerList.Count; i++)
		{
			MeshCombiner meshCombiner = meshCombinerList[i];
			if (meshCombiner.meshCombineJobs.Count > num)
			{
				num = meshCombiner.meshCombineJobs.Count;
			}
			if (meshCombiner.combinedActive && meshCombiner.combined)
			{
				GUI.Label(new Rect(10f, 30 + i * 22, 300f, 30f), meshCombiner.gameObject.name + " is Enabled.", textStyle);
			}
			else
			{
				GUI.Label(new Rect(10f, 30 + i * 22, 300f, 30f), meshCombiner.gameObject.name + " is Disabled.", textStyle);
			}
		}
		if (num > 0)
		{
			GUI.Label(new Rect(10f, 45 + meshCombinerList.Count * 22, 250f, 30f), "Combining => Jobs Left " + num, textStyle);
		}
		else
		{
			GUI.Label(new Rect(10f, 45 + meshCombinerList.Count * 22, 200f, 30f), "Toggle with 'Tab' key.", textStyle);
		}
	}
}
