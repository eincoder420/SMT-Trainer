using System;
using Crosstales.Common.Util;
using Crosstales.RTVoice.Model.Enum;
using UnityEngine;
using UnityEngine.UI;

namespace Crosstales.RTVoice.Demo;

[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_demo_1_1_g_u_i_dialog.html")]
public class GUIDialog : MonoBehaviour
{
	[Header("Dialog Script")]
	public Dialog DialogScript;

	[Header("UI Objects")]
	public Color32 SpeakerColor = new Color32(0, byte.MaxValue, 0, 192);

	public Image PanelPersonA;

	public Image PanelPersonB;

	public Text PersonA;

	public Text PersonB;

	private Color32 baseColorA;

	private Color32 baseColorB;

	private void Start()
	{
		if (PanelPersonA != null)
		{
			baseColorA = PanelPersonA.color;
		}
		if (PanelPersonB != null)
		{
			baseColorB = PanelPersonB.color;
		}
	}

	private void Update()
	{
		if (!string.IsNullOrEmpty(DialogScript.CurrentDialogA))
		{
			Text personA = PersonA;
			personA.text = personA.text + DialogScript.CurrentDialogA + Environment.NewLine + Environment.NewLine;
			DialogScript.CurrentDialogA = string.Empty;
			PanelPersonA.color = SpeakerColor;
			PanelPersonB.color = baseColorB;
		}
		if (!string.IsNullOrEmpty(DialogScript.CurrentDialogB))
		{
			Text personB = PersonB;
			personB.text = personB.text + DialogScript.CurrentDialogB + Environment.NewLine + Environment.NewLine;
			DialogScript.CurrentDialogB = string.Empty;
			PanelPersonA.color = baseColorA;
			PanelPersonB.color = SpeakerColor;
		}
	}

	public void StartDialog()
	{
		Silence();
		if (DialogScript != null)
		{
			StartCoroutine(DialogScript.DialogSequence());
		}
		else
		{
			Debug.LogWarning("'DialogScript' is null - please assign it in the editor!", this);
		}
	}

	public void Silence()
	{
		StopAllCoroutines();
		if (DialogScript != null)
		{
			if (DialogScript.AudioPersonA != null)
			{
				DialogScript.AudioPersonA.Stop();
			}
			if (DialogScript.AudioPersonB != null)
			{
				DialogScript.AudioPersonB.Stop();
			}
			DialogScript.Running = false;
		}
		Singleton<Speaker>.Instance.Silence();
		if (PanelPersonA != null)
		{
			PanelPersonA.color = baseColorA;
		}
		if (PanelPersonB != null)
		{
			PanelPersonB.color = baseColorB;
		}
		if (PersonA != null)
		{
			PersonA.text = string.Empty;
		}
		if (PersonB != null)
		{
			PersonB.text = string.Empty;
		}
	}

	public void ChangeRateA(float value)
	{
		DialogScript.RateA = value;
	}

	public void ChangeRateB(float value)
	{
		DialogScript.RateB = value;
	}

	public void ChangePitchA(float value)
	{
		DialogScript.PitchA = value;
	}

	public void ChangePitchB(float value)
	{
		DialogScript.PitchB = value;
	}

	public void ChangeVolumeA(float value)
	{
		DialogScript.VolumeA = value;
	}

	public void ChangeVolumeB(float value)
	{
		DialogScript.VolumeB = value;
	}

	public void GenderAChanged(int index)
	{
		DialogScript.GenderA = (Gender)index;
	}

	public void GenderBChanged(int index)
	{
		DialogScript.GenderB = (Gender)index;
	}
}
