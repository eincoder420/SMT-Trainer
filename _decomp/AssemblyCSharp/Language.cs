using System;
using UnityEngine;
using UnityEngine.UI;

public class Language : MonoBehaviour
{
	[Serializable]
	public struct Lang_Object
	{
		public string name;

		public string[] Text_Variant;

		public Text Text_Object;
	}

	public Lang_Object[] Lang_Objects;

	public Game_Data data;

	private void Start()
	{
		for (int i = 0; i < Lang_Objects.Length; i++)
		{
			if ((bool)Lang_Objects[i].Text_Object)
			{
				Lang_Objects[i].Text_Object.text = Lang_Objects[i].Text_Variant[data.Language];
				Lang_Objects[i].Text_Object.text = Lang_Objects[i].Text_Object.text.Replace("/", data.Name);
				Lang_Objects[i].Text_Object.text = Lang_Objects[i].Text_Object.text.Replace("]", data.Name3);
			}
		}
	}
}
