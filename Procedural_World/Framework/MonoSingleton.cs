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
    private bool Persistent = false;
    [SerializeField] private bool IsDontDestroy = false;

    protected static T instance;
    private static bool Instantiated = false;

    public static bool IsInitialize { get => instance != null; }

    public static T Instance
    {
        get
        {
            if (instance == null)
                Create();
            return instance;
        }
        set
        {
            instance = value;
        }
    }

    public static void Create()
    {
        if (instance == null)
        {
            T[] objects = GameObject.FindObjectsOfType<T>();
            if (objects.Length > 0)
            {
                instance = objects[0];

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
                GameObject go = new GameObject(string.Format("[Singleton]{0}", typeof(T).Name));
                instance = go.AddComponent<T>();
            }

            if (!Instantiated)
            {
                PersistentAttribute attribute = Attribute.GetCustomAttribute(typeof(T), typeof(PersistentAttribute)) as PersistentAttribute;
                if (attribute != null && attribute.Persistent)
                {
                    instance.Persistent = attribute.Persistent;
                    GameObject.DontDestroyOnLoad(instance.gameObject);
                }
            }

            Instantiated = true;
        }
    }

    private void Awake()
    {
        Create();
        if (IsDontDestroy)
        {
            if (instance != this)
            {
                Destroy(this.gameObject);
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
        if (!this.Persistent)
        {
            Instantiated = false;
            instance = null;
        }
    }

    virtual protected void Update()
    {
        OnUpdate();
    }

    virtual protected void LateUpdate()
    {
        OnLateUpdate();
    }

    virtual protected void OnAwake() { }

    virtual protected void OnUpdate() { }

    virtual protected void OnLateUpdate() { }
}
