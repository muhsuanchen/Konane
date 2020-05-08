using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	protected static T mInstance;
	public static T Instance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = (T)FindObjectOfType(typeof(T));
			}

			return mInstance;
		}
	}

	public static bool IsExist { get { return (mInstance != null); } }

	protected virtual void Awake()
	{
		mInstance = (T)(object)this;
	}

	protected virtual void OnDestroy()
	{
		mInstance = null;
	}

}