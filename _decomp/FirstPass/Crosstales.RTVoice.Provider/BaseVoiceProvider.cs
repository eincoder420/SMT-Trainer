namespace Crosstales.RTVoice.Provider;

public abstract class BaseVoiceProvider<T> : MainVoiceProvider where T : new()
{
	protected static T instance;

	public static T Instance
	{
		get
		{
			if (instance != null)
			{
				return instance;
			}
			return instance = new T();
		}
	}
}
