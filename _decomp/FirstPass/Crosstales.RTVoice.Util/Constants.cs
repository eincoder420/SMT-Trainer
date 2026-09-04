using System;
using Crosstales.Common.Util;
using UnityEngine;

namespace Crosstales.RTVoice.Util;

public abstract class Constants : BaseConstants
{
	public const string ASSET_NAME = "RT-Voice PRO";

	public const string ASSET_NAME_SHORT = "RTV PRO";

	public const string ASSET_VERSION = "2024.1.1";

	public const int ASSET_BUILD = 20240316;

	public static readonly DateTime ASSET_CREATED = new DateTime(2015, 4, 29);

	public static readonly DateTime ASSET_CHANGED = new DateTime(2024, 3, 16);

	public const string ASSET_PRO_URL = "https://assetstore.unity.com/packages/slug/41068?aid=1011lNGT";

	public const string ASSET_3P_URL = "https://assetstore.unity.com/lists/rt-voice-friends-42209?aid=1011lNGT";

	public const string ASSET_UPDATE_CHECK_URL = "https://www.crosstales.com/media/assets/rtvoice_versions.txt";

	public const string ASSET_CONTACT = "rtvoice@crosstales.com";

	public const string ASSET_MANUAL_URL = "https://www.crosstales.com/media/data/assets/rtvoice/RTVoice-doc.pdf";

	public const string ASSET_API_URL = "https://www.crosstales.com/en/assets/rtvoice/api/";

	public const string ASSET_FORUM_URL = "https://forum.unity.com/threads/rt-voice-run-time-text-to-speech-solution.340046/";

	public const string ASSET_WEB_URL = "https://www.crosstales.com/en/portfolio/rtvoice/";

	public const string ASSET_VIDEO_PROMO = "https://youtu.be/iVhTWDLY7g8?list=PLgtonIOr6Tb41XTMeeZ836tjHlKgOO84S";

	public const string ASSET_VIDEO_TUTORIAL = "https://youtu.be/OJyVgCmX3wU?list=PLgtonIOr6Tb41XTMeeZ836tjHlKgOO84S";

	public const string ASSET_3P_ADVENTURE_CREATOR = "https://assetstore.unity.com/packages/slug/11896?aid=1011lNGT";

	public const string ASSET_3P_CINEMA_DIRECTOR = "https://assetstore.unity.com/packages/slug/19779?aid=1011lNGT";

	public const string ASSET_3P_DIALOGUE_SYSTEM = "https://assetstore.unity.com/packages/slug/11672?aid=1011lNGT";

	public const string ASSET_3P_LOCALIZED_DIALOGS = "https://assetstore.unity.com/packages/slug/5020?aid=1011lNGT";

	public const string ASSET_3P_LIPSYNC = "https://assetstore.unity.com/packages/slug/32117?aid=1011lNGT";

	public const string ASSET_3P_NANINOVEL = "https://assetstore.unity.com/packages/slug/135453?aid=1011lNGT";

	public const string ASSET_3P_NPC_CHAT = "https://assetstore.unity.com/packages/slug/9723?aid=1011lNGT";

	public const string ASSET_3P_QUEST_SYSTEM = "https://assetstore.unity.com/packages/slug/63460?aid=1011lNGT";

	public const string ASSET_3P_SALSA = "https://assetstore.unity.com/packages/slug/148442?aid=1011lNGT";

	public const string ASSET_3P_SLATE = "https://assetstore.unity.com/packages/slug/56558?aid=1011lNGT";

	public const string ASSET_3P_AMPLITUDE = "https://assetstore.unity.com/packages/slug/111277?aid=1011lNGT";

	public const string ASSET_3P_KLATTERSYNTH = "https://assetstore.unity.com/packages/slug/95453?aid=1011lNGT";

	public const string ASSET_3P_WEBGL = "https://assetstore.unity.com/packages/slug/81861?aid=1011lNGT";

	public const string ASSET_3P_GOOGLE = "https://assetstore.unity.com/packages/slug/115170?aid=1011lNGT";

	public const string KEY_PREFIX = "RTVOICE_CFG_";

	public const string KEY_ASSET_PATH = "RTVOICE_CFG_ASSET_PATH";

	public const string KEY_DEBUG = "RTVOICE_CFG_DEBUG";

	public const string KEY_AUDIOFILE_PATH = "RTVOICE_CFG_AUDIOFILE_PATH";

	public const string KEY_AUDIOFILE_AUTOMATIC_DELETE = "RTVOICE_CFG_AUDIOFILE_AUTOMATIC_DELETE";

	public const string KEY_ENFORCE_STANDALONE_TTS = "RTVOICE_CFG_ENFORCE_STANDALONE_TTS";

	public static readonly string DEFAULT_AUDIOFILE_PATH = FileHelper.ValidatePath(Application.temporaryCachePath);

	public const bool DEFAULT_AUDIOFILE_AUTOMATIC_DELETE = true;

	public const bool DEFAULT_ENFORCE_STANDALONE_TTS = true;

	public const string DEFAULT_TTS_MACOS = "say";

	public const int DEFAULT_CACHE_SIZE_CLIPS = 256;

	public const int DEFAULT_MAX_CACHE_SIZE_CLIPS = 1024;

	public const int DEFAULT_TTS_KILL_TIME = 7000;

	public const string RTVOICE_SCENE_OBJECT_NAME = "RTVoice";

	public const string GLOBALCACHE_SCENE_OBJECT_NAME = "GlobalCache";

	public static string WINDOWS_TTS_SUBPATH = "RTVoiceTTSWrapper.exe";

	public static string WINDOWS_TTS_x86_SUBPATH = "RTVoiceTTSWrapper_x86.exe";

	public static string ESPEAK_FEMALE_MODIFIER = "+f3";

	public static string AUDIOFILE_PREFIX = "rtvoice_";

	public static float SPEAK_CALL_SPEED = 0.5f;

	public static string VOICE_AGE_ADULT = "adult";

	public static string VOICE_AGE_CHILD = "child";

	public static string VOICE_AGE_ELDERLY = "elderly";

	public static string VOICE_AGE_UNKNOWN = "unknown";
}
