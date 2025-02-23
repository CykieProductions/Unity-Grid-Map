using UnityEngine;
using UnityEngine.SceneManagement;
using CykieProductions.Cytools;

namespace CykieProductions.GridMap
{

    public class MapSystemManager : SingletonComponent<MapSystemManager>, IGameManager, IPauseHandler
    {
        //public static GameManager instance {  get; private set; }

        [SerializeField] Transform _player;
        //public IPlayerController pController { get; private set; }
        [field: SerializeField]
        public string MapSceneName { get; private set; }
        [field: SerializeField] public float MapGridSize { get; private set; } = 12;

        /// <summary>From <see cref="IGameManager"/></summary>
        public Transform Player => GetPlayer();

        protected override void Awake()
        {
            base.Awake();

            if (_player == null)
            {
                GetPlayer();
                //pController = player.GetComponentInChildren<IPlayerController>();
            }

            //! Load Map Scene
            bool mapIsLoaded = false;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i) == SceneManager.GetSceneByName(MapSceneName))
                {
                    mapIsLoaded = true;
                    break;
                }
            }
            if (!mapIsLoaded)
                SceneManager.LoadSceneAsync(MapSceneName, LoadSceneMode.Additive);
        }

        protected virtual void Start()
        {
            //This will only work is no other script has claimed true GameManager status
            //Can be controlled by using the Awake method or changing the Script Execution Order
            IGameManager.TrySetMain(this);
        }

        public virtual Transform GetPlayer()
        {
            if (_player == null)
            {
                _player = GameObject.FindWithTag("Player").transform;
                return _player;
            }
            return _player;
        }

        public virtual void TogglePause()
        {
            if (Time.timeScale != 0)
            {
                Pause();
            }
            else
            {
                Unpause();
            }
        }

        public virtual void Pause()
        {
            Time.timeScale = 0;
        }
        public virtual void Unpause()
        {
            Time.timeScale = 1;
        }
    }

}