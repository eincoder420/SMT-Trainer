namespace Crosstales.RTVoice.Util;

public static class Context
{
	public static int NumberOfSpeeches;

	public static int NumberOfAudioFiles;

	public static int NumberOfCharacters;

	public static float TotalSpeechLength;

	public static int NumberOfCachedSpeeches;

	public static int NumberOfNonCachedSpeeches;

	public static float CacheEfficiency
	{
		get
		{
			if (NumberOfNonCachedSpeeches > 0)
			{
				return (float)NumberOfCachedSpeeches / (float)NumberOfNonCachedSpeeches;
			}
			return NumberOfCachedSpeeches;
		}
	}
}
