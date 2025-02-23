using UnityEngine;

namespace CykieProductions.Cytools
{

    public abstract class SingletonComponent<T> : MonoBehaviour where T : SingletonComponent<T>
    {
        public static T instance { get; private set; }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                if (instance == null)
                {
                    Debug.LogError($"The Singleton type <{typeof(T)}> was invalid! Please ensure that T and the inheriting component matches!");
                    Debug.Break();
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

    }

    public interface IPlayerController { }
    public interface IGameManager
    {
        public static IGameManager Main { get; private set; } = null;
        public static bool TrySetMain(IGameManager gameManager)
        {
            if (Main == null)
            {
                Main = gameManager;
                return true;
            }
            return false;
        }

        public Transform Player { get; }
    }

    public interface IPauseHandler
    {
        public void Pause();
        public void Unpause();
        public void TogglePause();
    }

    public interface IInputTracker
    {
        public static IInputTracker Current { get; private set; }
        public static bool TrySetCurrent(IInputTracker inputTracker)
        {
            if (Current == null)
            {
                Current = inputTracker;
                return true;
            }
            return false;
        }
        public float HorizontalRaw();
        public float VerticalRaw();
    }
}
