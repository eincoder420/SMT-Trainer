using System.IO;
using UnityEngine;

public class Json_Script : MonoBehaviour
{
	public Game_Data data;

	[ContextMenu("Load")]
	public void Load_Data()
	{
		JsonUtility.FromJsonOverwrite(File.ReadAllText(Application.streamingAssetsPath + "/saved_data.json"), data);
	}

	[ContextMenu("Save")]
	public void Save_Data()
	{
		File.WriteAllText(Application.streamingAssetsPath + "/saved_data.json", JsonUtility.ToJson(data));
	}
}
