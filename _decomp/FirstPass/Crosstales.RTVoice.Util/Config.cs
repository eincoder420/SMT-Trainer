using Crosstales.Common.Util;

namespace Crosstales.RTVoice.Util;

public static class Config
{
	public static string ASSET_PATH = "/Plugins/crosstales/RTVoice/";

	public static bool DEBUG = BaseConstants.DEV_DEBUG;

	public static bool AUDIOFILE_AUTOMATIC_DELETE = true;

	public static bool ENFORCE_STANDALONE_TTS = true;

	public static string TTS_MACOS = "say";

	public static bool isLoaded;

	private static string audiofilePath = Constants.DEFAULT_AUDIOFILE_PATH;

	public static string AUDIOFILE_PATH
	{
		get
		{
			return audiofilePath;
		}
		set
		{
			audiofilePath = FileHelper.ValidatePath(value);
		}
	}
}
