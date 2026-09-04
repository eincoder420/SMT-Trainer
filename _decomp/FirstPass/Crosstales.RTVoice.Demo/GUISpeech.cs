using System;
using System.Collections.Generic;
using Crosstales.Common.Util;
using Crosstales.RTVoice.Model;
using Crosstales.RTVoice.Model.Enum;
using UnityEngine;
using UnityEngine.UI;

namespace Crosstales.RTVoice.Demo;

[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_demo_1_1_g_u_i_speech.html")]
public class GUISpeech : MonoBehaviour
{
	[Header("Settings")]
	public bool StartAsNative;

	public GUIMultiAudioFilter AudioFilter;

	[Header("Table")]
	public GameObject ItemPrefab;

	public GameObject Target;

	public Scrollbar Scroll;

	public int ColumnCount = 1;

	public Vector2 SpaceWidth = new Vector2(8f, 8f);

	public Vector2 SpaceHeight = new Vector2(8f, 8f);

	[Header("UI Objects")]
	public InputField Input;

	public InputField Culture;

	public Text Cultures;

	public Text Voices;

	public static float Rate = 1f;

	public static float Pitch = 1f;

	public static float Volume = 1f;

	public static bool isNative;

	private string lastCulture = "unknown";

	private List<Voice> items = new List<Voice>();

	private Gender gender = Gender.UNKNOWN;

	private const string forceUpdate = "RT-Voice rulez!";

	private void Start()
	{
		Rate = 1f;
		Pitch = 1f;
		Volume = 1f;
		isNative = false;
		Singleton<Speaker>.Instance.OnProviderChange += onProviderChange;
		Singleton<Speaker>.Instance.OnVoicesReady += onVoicesReady;
		if (Singleton<Speaker>.Instance.isSSMLSupported)
		{
			if (Input != null)
			{
				Input.text = "Hi there, my name is RT-Voice, your runtime speaker!" + Environment.NewLine + "I can speak with the complete SSML specification <prosody rate=\"-50%\">at half speed</prosody> or <prosody pitch=\"-50%\">50% lower pitched.</prosody>. " + Environment.NewLine + "<prosody contour=\"(0%,+20%) (40%,+40%) (60%,+60%) (80%,+80%) (100%,+100%)\">I can talk with rising intonation</prosody> <prosody contour=\"(0%,-20%) (40%,-40%) (60%,-60%) (80%,-80%) (100%,-100%)\">or with falling intonation.</prosody>" + Environment.NewLine + "This is <emphasis level=\"strong\">awesome</emphasis>!";
			}
		}
		else if (Input != null)
		{
			Input.text = "Hi there, my name is RT-Voice, your runtime speaker!";
		}
		if (Culture != null)
		{
			Culture.text = string.Empty;
		}
		isNative = StartAsNative;
		if (Voices != null)
		{
			Voices.text = "Voices (" + items.Count + ")";
		}
	}

	private void Update()
	{
		if (Culture != null && !lastCulture.Equals(Culture.text) && Singleton<Speaker>.Instance.areVoicesReady)
		{
			buildVoicesList();
			lastCulture = Culture.text;
		}
	}

	private void OnDestroy()
	{
		if (Singleton<Speaker>.Instance != null)
		{
			Singleton<Speaker>.Instance.OnProviderChange -= onProviderChange;
			Singleton<Speaker>.Instance.OnVoicesReady -= onVoicesReady;
		}
	}

	public void Silence()
	{
		Singleton<Speaker>.Instance.Silence();
	}

	public void ChangeRate(float rate)
	{
		Rate = rate;
	}

	public void ChangeVolume(float volume)
	{
		Volume = volume;
	}

	public void ChangePitch(float pitch)
	{
		Pitch = pitch;
	}

	public void ChangeNative(bool native)
	{
		isNative = native;
	}

	public void GenderChanged(int index)
	{
		gender = (Gender)index;
		Invoke("buildVoicesList", 0.2f);
	}

	private void onProviderChange(string provider)
	{
		lastCulture = "RT-Voice rulez!";
	}

	private void onVoicesReady()
	{
		lastCulture = "RT-Voice rulez!";
		if (Cultures != null)
		{
			Cultures.text = string.Join(", ", Singleton<Speaker>.Instance.Cultures.ToArray());
		}
	}

	private void clearVoicesList()
	{
		if (AudioFilter != null)
		{
			AudioFilter.ClearFilters();
		}
		if (Target != null)
		{
			for (int num = Target.transform.childCount - 1; num >= 0; num--)
			{
				Transform child = Target.transform.GetChild(num);
				child.SetParent(null);
				UnityEngine.Object.Destroy(child.gameObject);
			}
		}
	}

	private void buildVoicesList()
	{
		clearVoicesList();
		if (Target != null)
		{
			RectTransform component = Target.GetComponent<RectTransform>();
			items = Singleton<Speaker>.Instance.VoicesForGender(gender, Culture.text);
			if (items.Count > 0)
			{
				float num = component.rect.width / (float)ColumnCount - SpaceWidth.x;
				float num2 = SpaceHeight.x + SpaceHeight.y;
				int num3 = items.Count / ColumnCount;
				if (num3 > 0 && items.Count % num3 > 0)
				{
					num3++;
				}
				float num4 = num2 * (float)num3;
				component.offsetMin = new Vector2(component.offsetMin.x, (0f - num4) / 2f);
				component.offsetMax = new Vector2(component.offsetMax.x, num4 / 2f);
				int num5 = 0;
				for (int i = 0; i < items.Count; i++)
				{
					if (i % ColumnCount == 0)
					{
						num5++;
					}
					GameObject gameObject = UnityEngine.Object.Instantiate(ItemPrefab, Target.transform, worldPositionStays: true);
					gameObject.transform.localPosition = Vector3.zero;
					gameObject.transform.rotation = Target.transform.rotation;
					gameObject.transform.localScale = Vector3.one;
					gameObject.name = Target.name + " item at (" + i + "," + num5 + ")";
					if (AudioFilter != null)
					{
						AudioFilter.Sources.Add(gameObject.GetComponent<AudioSource>());
						AudioFilter.ReverbFilters.Add(gameObject.GetComponent<AudioReverbFilter>());
						AudioFilter.ChorusFilters.Add(gameObject.GetComponent<AudioChorusFilter>());
						AudioFilter.EchoFilters.Add(gameObject.GetComponent<AudioEchoFilter>());
						AudioFilter.DistortionFilters.Add(gameObject.GetComponent<AudioDistortionFilter>());
						AudioFilter.LowPassFilters.Add(gameObject.GetComponent<AudioLowPassFilter>());
						AudioFilter.HighPassFilters.Add(gameObject.GetComponent<AudioHighPassFilter>());
					}
					SpeakWrapper component2 = gameObject.GetComponent<SpeakWrapper>();
					component2.SpeakerVoice = items[i];
					component2.Input = Input;
					component2.Label.text = items[i].Name;
					RectTransform component3 = gameObject.GetComponent<RectTransform>();
					float x = (num + SpaceWidth.x) * (float)(i % ColumnCount) + SpaceWidth.x;
					float y = (0f - num2) * (float)num5;
					Vector2 vector2 = (component3.offsetMin = new Vector2(x, y));
					x = vector2.x + num;
					y = vector2.y + SpaceHeight.x;
					component3.offsetMax = new Vector2(x, y);
				}
				if (AudioFilter != null)
				{
					AudioFilter.ResetFilters();
				}
			}
			if (Scroll != null)
			{
				Scroll.value = 1f;
			}
		}
		if (Voices != null)
		{
			Voices.text = "Voices (" + items.Count + ")";
		}
		onVoicesReady();
	}
}
