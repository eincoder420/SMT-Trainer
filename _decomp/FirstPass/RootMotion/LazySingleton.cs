using UnityEngine;

namespace RootMotion;

public abstract class LazySingleton<T> : MonoBehaviour where T : LazySingleton<T>
{
	private static T sInstance;

	public static bool hasInstance => (Object)sInstance != (Object)null;

	public static T instance
	{
		get
		{
			if ((Object)sInstance == (Object)null)
			{
				sInstance = new GameObject(typeof(T).ToString()).AddComponent<T>();
			}
			return sInstance;
		}
	}

	protected virtual void Awake()
	{
		sInstance = (T)this;
	}
}
