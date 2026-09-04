using System.Collections.Generic;
using System.IO;
using System.Linq;
using Crosstales.Common.Audio;
using Crosstales.Common.Util;
using Crosstales.RTVoice.Model;
using Crosstales.RTVoice.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace Crosstales.RTVoice;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[HelpURL("https://crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_global_cache.html")]
public class GlobalCache : Singleton<GlobalCache>
{
	[FormerlySerializedAs("ClipCacheSize")]
	[Header("Cache Settings")]
	[Tooltip("Size of the clip cache in MB (default: 256)")]
	[Range(16f, 1024f)]
	[SerializeField]
	private int clipCacheSize = 256;

	[Tooltip("Automatically loads and saves the cache (default: false)")]
	[SerializeField]
	private bool persistCache;

	public readonly Dictionary<Wrapper, AudioClip> Clips = new Dictionary<Wrapper, AudioClip>();

	private readonly List<Wrapper> clipKeys = new List<Wrapper>();

	private Transform tf;

	private static string dataStorePath;

	public int ClipCacheSize
	{
		get
		{
			return clipCacheSize * 1048576;
		}
		set
		{
			clipCacheSize = Mathf.Clamp(value / 1048576, 1, 1024);
		}
	}

	public int CurrentClipCacheSize => Clips.Sum((KeyValuePair<Wrapper, AudioClip> pair) => pair.Value.samples * 2 * 4);

	public bool PersistCache
	{
		get
		{
			return persistCache;
		}
		set
		{
			persistCache = value;
		}
	}

	private void Start()
	{
		dataStorePath = Application.persistentDataPath + "/rtvoice_datastore.xml";
		if (persistCache)
		{
			LoadCache();
		}
	}

	private void OnValidate()
	{
		if (clipCacheSize <= 16)
		{
			clipCacheSize = 16;
		}
		else if (clipCacheSize > 1024)
		{
			clipCacheSize = 1024;
		}
	}

	protected override void OnApplicationQuit()
	{
		if (persistCache)
		{
			SaveCache();
		}
		ClearCache();
		base.OnApplicationQuit();
	}

	public static void ResetObject()
	{
		Singleton<GlobalCache>.DeleteInstance();
	}

	public AudioClip GetClip(Wrapper key)
	{
		if (key != null)
		{
			Clips.TryGetValue(key, out var value);
			return value;
		}
		return null;
	}

	public void RemoveClip(Wrapper key)
	{
		if (key != null && Clips.ContainsKey(key))
		{
			Object.Destroy(Clips[key]);
			Clips.Remove(key);
			clipKeys.Remove(key);
		}
	}

	public void AddClip(Wrapper key, AudioClip data)
	{
		if (key != null && data != null && !Clips.ContainsKey(key))
		{
			while (CurrentClipCacheSize >= ClipCacheSize)
			{
				RemoveClip(clipKeys[0]);
			}
			Clips.Add(key, data);
			clipKeys.Add(key);
		}
	}

	public void ClearClipCache()
	{
		Context.NumberOfCachedSpeeches = 0;
		Context.NumberOfNonCachedSpeeches = 0;
		foreach (KeyValuePair<Wrapper, AudioClip> clip in Clips)
		{
			if (BaseHelper.isEditorMode)
			{
				Object.DestroyImmediate(clip.Value);
			}
			else
			{
				Object.Destroy(clip.Value);
			}
		}
		Clips.Clear();
		clipKeys.Clear();
	}

	public void ClearCache()
	{
		ClearClipCache();
	}

	public void ClearAndDeleteCache()
	{
		ClearCache();
		if (File.Exists(dataStorePath))
		{
			File.Delete(dataStorePath);
		}
	}

	public void SaveCache()
	{
		XmlHelper.SerializeToFile(Clips.Select((KeyValuePair<Wrapper, AudioClip> kvp) => new DataStore(kvp.Key, WavMaster.FromAudioClip(kvp.Value))).ToList(), dataStorePath);
	}

	public void LoadCache()
	{
		if (!File.Exists(dataStorePath))
		{
			return;
		}
		List<DataStore> list = XmlHelper.DeserializeFromFile<List<DataStore>>(dataStorePath);
		if (list == null)
		{
			return;
		}
		foreach (DataStore item in list)
		{
			AddClip(item.wrapper, WavMaster.ToAudioClip(item.Data));
		}
	}
}
