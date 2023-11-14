using SupanthaPaul;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance {  get; private set; }
    public Transform player { get; private set; }
    public PlayerController pController { get; private set; }
    [field: SerializeField]
    public string MapSceneName { get; private set; }
    [field: SerializeField] public float MapGridSize { get; private set; } = 12;

    void Awake()
    {
        //Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (player == null)
        {
            player = GameObject.FindWithTag("Player").transform;
            pController = player.GetComponentInChildren<PlayerController>();
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

    void Update()
    {

    }

    public void TogglePause()
    {
        if (Time.timeScale != 0)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    internal void Pause()
    {
        Time.timeScale = 0;
    }
    internal void Unpause()
    {
        Time.timeScale = 1;
    }
}
