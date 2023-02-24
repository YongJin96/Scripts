using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class PersistentAttribute : Attribute
{
	public readonly bool Persistent;
	public PersistentAttribute(bool persistent)
	{
		this.Persistent = persistent;
	}
}

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
	protected static T _instance = null;
	private static bool instantiated = false;

	public static bool isInitialize { get => _instance != null; }

	static public T instance
	{
		get
		{
			if (_instance == null)
				Create();
			return _instance;
		}
		set
		{
			_instance = value;
		}
	}

	static public void Create()
	{
		if (_instance == null)
		{
			T[] objects = GameObject.FindObjectsOfType<T>();
			if (objects.Length > 0)
			{
				_instance = objects[0];

				for (int i = 1; i < objects.Length; ++i)
				{
					if (Application.isPlaying)
						GameObject.Destroy(objects[i].gameObject);
					else
						GameObject.DestroyImmediate(objects[i].gameObject);
				}
			}
			else
			{
				GameObject go = new GameObject(string.Format("{0}", typeof(T).Name));
				_instance = go.AddComponent<T>();
			}

			if (!instantiated)
			{
				PersistentAttribute attribute = Attribute.GetCustomAttribute(typeof(T), typeof(PersistentAttribute)) as PersistentAttribute;
				if (attribute != null && attribute.Persistent)
				{
					_instance.persistent = attribute.Persistent;
					GameObject.DontDestroyOnLoad(_instance.gameObject);
				}

				//_instance.OnAwake();
			}

			instantiated = true;
		}
	}

	private bool persistent = false;
	[SerializeField]
	private bool isDontDestroy = false;

	private void Awake()
	{
		Create();
		if (isDontDestroy)
		{
			if (instance != this)
			{
				Destroy(gameObject);
			}
			else
			{
				DontDestroyOnLoad(this);
			}
		}
		OnAwake();
	}

	virtual protected void OnDestroy()
	{
		if (!this.persistent)
		{
			instantiated = false;
			_instance = null;
		}
	}

	virtual protected void OnAwake() { }
}