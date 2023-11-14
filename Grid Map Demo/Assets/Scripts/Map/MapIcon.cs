using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using SupanthaPaul;

public class MapIcon : MonoBehaviour
{
    public static MapIcon instance {  get; private set; }
    Transform player;
    PlayerController pController;
    public string MapSceneName { get => GameManager.instance.MapSceneName; }

    string mapMarkerSaveKey = "map markers";

    [Space]
    bool isFullMapOpened;
    [SerializeField] bool snapToGrid = true;
    [SerializeField] Vector2 iconOffset = Vector2.zero;
    [SerializeField] Transform mapCursor;
    [SerializeField] Transform arrowToIcon;

    [Header("MAP CAMERA")]
    [SerializeField] Camera mapCam;
    [SerializeField] float camSpeed = 75;
    [SerializeField] float fullMapCamSize = 80;
    [SerializeField] float miniMapCamSize = 20;


    [Space, Header("MARKERS")]
    [SerializeField] Transform markerParent;
    public CanvasGroup markerSelection;

    public Transform[] markerButtonTransforms;
    public Transform markerButtonHighlighter;

    public GameObject[] markerPrefabs;
    int activeMarkerIndex;
    public float maxRemoveMarkerDist = 3f;

    public float snapToPointSpeed = 100f;
    List<Transform> searchMarkerByTypeList = new List<Transform>();
    int searchIndex = 0;
    Vector3 searchTargetPos = new Vector3(-1337, 420, 1337);

    [SerializeField] List<GameObject> fullMapHUD;
    [SerializeField] GameObject placeModeHUD;
    [SerializeField] GameObject selectModeHUD;

    void Start()
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
        
        player = GameManager.instance.player;
        pController = GameManager.instance.pController;

        //?pi = player.GetComponent<InputController>();

        activeMarkerIndex = 0;
        //?markerButtonHighlighter.position = markerButtonTransforms[activeMarkerIndex].position;
        //?UpdateMarkerAmountText();
    }

    private void OnEnable()
    {
        /*?GameEvents.OnGameSaved += SaveMarkers;
        GameEvents.OnGameLoaded += LoadMarkers;*/
    }
    private void OnDisable()
    {
        /*?GameEvents.OnGameSaved -= SaveMarkers;
        GameEvents.OnGameLoaded -= LoadMarkers;*/
    }
    void Update()
    {
        //! Snap to map grid
        if (/*?!pController.isBetweenRooms &&*/ !RoomController.CurrentRoom || RoomController.CurrentRoom.isMappableRoom)
        {
            float cellSize = GameManager.instance.MapGridSize;
            Vector3 roundedPlayerPos = player.position;
            if (snapToGrid)
                roundedPlayerPos = new(Mathf.Round(player.position.x / cellSize) * cellSize, 
                    Mathf.Round(player.position.y / cellSize) * cellSize);
            transform.position = roundedPlayerPos + (Vector3)iconOffset;
        }

        if (InputSystem.OpenMap())
        {
            if (!isFullMapOpened)
            {
                GameManager.instance.Pause();
                //?UpdateMarkerAmountText();

                //Enable the HUD on map open
                for (int i = 0; i < fullMapHUD.Count; i++)
                {
                    fullMapHUD[i].SetActive(true);
                }
                isFullMapOpened = true;
            }
            else
            {
                GameManager.instance.Unpause();
                isFullMapOpened = false;
                //Disable the HUD on map close
                for (int i = 0; i < fullMapHUD.Count; i++)
                {
                    fullMapHUD[i].SetActive(false);
                }
            }
        }

        if (isFullMapOpened)
        {
            /*?/! HUD Stuff //
            if (InputSystem.Jump())//HUD hiding
            {
                for (int i = 0; i < fullMapHUD.Count; i++)
                {
                    fullMapHUD[i].SetActive(!fullMapHUD[i].activeSelf);
                }
            }

            //Only show the needed button prompts
            if (InputSystem.PressingSwitch())//Select Mode Prompts
            {
                selectModeHUD.SetActive(true);
                placeModeHUD.SetActive(false);
            }
            else
            {
                placeModeHUD.SetActive(true);
                selectModeHUD.SetActive(false);
            }

            if (InputSystem.OpenMap())//Potential failsafe
            {
                markerSelection.alpha = 0;
                markerSelection.blocksRaycasts = false;
                markerSelection.interactable = false;
            }*/

            mapCam.orthographicSize = fullMapCamSize;
            if (!mapCursor.gameObject.activeSelf)
                mapCursor.gameObject.SetActive(true);

            if (Vector2.Distance(mapCam.transform.position, transform.position) > 12)
            {
                //The arrow points in the direction of the player
                if (!arrowToIcon.gameObject.activeSelf)
                    arrowToIcon.gameObject.SetActive(true);

                //The cursor must be on the same Z plane as the icon for this to work
                mapCursor.LookAt(transform.position);
                mapCursor.Rotate(0, -90, -90);
            }
            else//don't display the arrow if the cursor is too close to the icon
            {
                if (arrowToIcon.gameObject.activeSelf)
                    arrowToIcon.gameObject.SetActive(false);

                mapCursor.eulerAngles = new Vector3(0, 0, 0);
            }

            //! Camera Panning Logic
            //?if (!InputSystem.PressingSwitch())
            {
                searchTargetPos = new Vector3(-1337, 420, 1337);//Reset Search location
                var mov = new Vector3(InputSystem.HorizontalRaw(), InputSystem.VerticalRaw(), 0);
                var curSpeed = camSpeed;
                /*if (GameManager.instance.gameInput.pressedDashForward)
                    curSpeed = camSpeed * 2;*/

                mapCam.transform.position += curSpeed * Time.unscaledDeltaTime * mov;

                /*?
                //! Place Marker Logic
                markerSelection.alpha = 0.25f;
                markerSelection.blocksRaycasts = false;
                markerSelection.interactable = false;
                if (InputSystem.ActionDown())
                {
                    if (pController.stats.curState.numOfDLMMarkers[activeMarkerIndex] > 0)
                    {
                        var marker = Instantiate(markerPrefabs[activeMarkerIndex], fullCam.transform.position + Vector3.forward, Quaternion.identity, markerParent);
                        ResetMarkerSearch();
                        marker.name = activeMarkerIndex.ToString() + "~" + markerPrefabs[activeMarkerIndex].name;
                        marker.SetActive(true);
                        pController.stats.curState.numOfDLMMarkers[activeMarkerIndex]--;
                        markerButtonTransforms[activeMarkerIndex].GetComponentInChildren<TextMeshProUGUI>().text = pController.stats.curState.numOfDLMMarkers[activeMarkerIndex].ToString();
                    }
                }
                //Delete marker
                if (GameManager.instance.gameInput.pressedStomp && markerParent.childCount > 0)
                {
                    float shortestDist = float.MaxValue;
                    int closestMarkerIndex = -1;
                    for (int i = 0; i < markerParent.childCount; i++)
                    {
                        float dist = Vector2.Distance(markerParent.GetChild(i).position, fullCam.transform.position);
                        if (dist <= maxRemoveMarkerDist && dist < shortestDist)
                        {
                            closestMarkerIndex = i;
                        }
                    }

                    if (closestMarkerIndex > -1)
                    {
                        for (int i = 0; i < markerPrefabs.Length; i++)
                        {
                            //print(markerPrefabs[i].name + " and " + markerParent.GetChild(closestMarkerIndex).name.Split('~')[1] + " are equal: " + );
                            //print(markerPrefabs[i].name == markerParent.GetChild(closestMarkerIndex).name.Split('~')[1]);
                            if (i.ToString() == markerParent.GetChild(closestMarkerIndex).name.Split('~')[0])
                            {
                                //print(i + " & " + markerParent.GetChild(closestMarkerIndex).name.Split('~')[0]);
                                pController.stats.curState.numOfDLMMarkers[i]++;
                                markerButtonTransforms[i].GetComponentInChildren<TextMeshProUGUI>().text = pController.stats.curState.numOfDLMMarkers[i].ToString();
                                break;
                            }
                        }
                        Destroy(markerParent.GetChild(closestMarkerIndex).gameObject);
                        ResetMarkerSearch();
                    }
                }*/
            }
            //?else//Marker change mode
            {
#if false
                /*if (GameManager.instance.gameInput.pressedSwitch && markerSelection.blocksRaycasts == false)
                {
                    for (int i = 0; i < markerButtonTransforms.Length; i++)
                    {
                        markerButtonTransforms[i].GetComponentInChildren<TextMeshProUGUI>().text = pController.stats.numOfDLMMarkers[i].ToString();
                    }
                }*/

                //markerSelection.gameObject.SetActive(true);
                markerSelection.alpha = 1;
                markerSelection.blocksRaycasts = true;
                markerSelection.interactable = true;

                if (GameManager.instance.gameInput.pressedHorizontal)
                {
                    if (GameManager.instance.gameInput.horizontal > 0f)
                    {
                        //SetMarker(activeMarkerIndex++);
                        activeMarkerIndex++;
                        if (activeMarkerIndex >= markerPrefabs.Length)
                            activeMarkerIndex = 0;

                        ResetMarkerSearch();
                        markerButtonHighlighter.position = markerButtonTransforms[activeMarkerIndex].position;
                        GameManager.instance.gameInput.pressedHorizontal = false;
                    }
                    else if (GameManager.instance.gameInput.horizontal < 0f)
                    {
                        //SetMarker(activeMarkerIndex--);
                        activeMarkerIndex--;
                        if (activeMarkerIndex < 0)
                            activeMarkerIndex = markerPrefabs.Length - 1;

                        ResetMarkerSearch();
                        markerButtonHighlighter.position = markerButtonTransforms[activeMarkerIndex].position;
                        GameManager.instance.gameInput.pressedHorizontal = false;
                    }
                }

                //Move Camera to the next marker of selected type
                if (GameManager.instance.gameInput.pressedShoot)
                {
                    print("START SEARCH PROCESS");
                    if (searchMarkerByTypeList.Count == 0)
                    {
                        print("TRY POPULATE");
                        for (int i = 0; i < markerParent.childCount; i++)
                        {
                            if (markerParent.GetChild(i).name.Split('~')[1] == markerPrefabs[activeMarkerIndex].name)
                            {
                                searchMarkerByTypeList.Add(markerParent.GetChild(i));
                            }
                        }
                        if (searchMarkerByTypeList.Count == 0)//If you still can't find any markers, the go back to the player
                        {
                            BackToPlayer();
                        }
                    }

                    if (searchMarkerByTypeList.Count > 0)
                    {
                        print("GO TO NEXT");
                        if (searchIndex < searchMarkerByTypeList.Count)
                        {
                            //fullCam.transform.position = searchMarkerByTypeList[searchIndex].position - Vector3.forward;
                            searchTargetPos = searchMarkerByTypeList[searchIndex].position - Vector3.forward;
                            searchIndex++;
                        }
                        else//if search index is too high, reset to player
                        {
                            //fullCam.transform.localPosition = new Vector3(0.005f, 0, 0);
                            BackToPlayer();
                        }
                    }

                }
                if (searchTargetPos != new Vector3(-1337, 420, 1337))
                {
                    fullCam.transform.position = Vector3.Lerp(fullCam.transform.position, searchTargetPos, snapToPointSpeed * Time.unscaledDeltaTime);
                }

                void BackToPlayer()
                {
                    searchTargetPos = transform.position + new Vector3(0.005f, 0, 0);
                    searchIndex = 0;
                }
#endif
            }
        }
        else
        {
            mapCam.orthographicSize = miniMapCamSize;
            mapCam.transform.localPosition = Vector3.zero;
            mapCursor.eulerAngles = Vector3.zero;
            if (mapCursor.gameObject.activeSelf)
                mapCursor.gameObject.SetActive(false);
        }
    }

    /*?public void UpdateMarkerAmountText()
    {
        for (int i = 0; i < markerButtonTransforms.Length; i++)
        {
            markerButtonTransforms[i].GetComponentInChildren<TextMeshProUGUI>().text = pController.stats.curState.numOfDLMMarkers[i].ToString();
        }
    }*/

    void ResetMarkerSearch()
    {
        if (searchMarkerByTypeList.Count > 0)
            searchMarkerByTypeList.Clear();
        searchIndex = 0;
        searchTargetPos = new Vector3(-1337, 420, 1337);
    }

    /*public void SetMarker(int prefabIndex)
    {
        //Loop around function
        /*if (prefabIndex >= markerPrefabs.Length)
            prefabIndex = 0;
        else if (prefabIndex < 0)
            prefabIndex = markerPrefabs.Length - 1;*

        activeMarkerIndex = prefabIndex;
        markerButtonHighlighter.position = markerButtonTransforms[activeMarkerIndex].position;
    }*/
    public void MoveCamerasToMapScene()
    {
        SceneManager.MoveGameObjectToScene(mapCam.gameObject, SceneManager.GetSceneByName("Map Scene"));
        //x SceneManager.MoveGameObjectToScene(miniCam.gameObject, SceneManager.GetSceneByName("Map Scene"));
    }

    public void SaveMarkers()
    {
        /*?List<MarkerSaveData> markerSaveData = new List<MarkerSaveData>();
        for (int i = 0; i < markerParent.childCount; i++)
        {
            markerSaveData.Add(new MarkerSaveData(int.Parse(markerParent.GetChild(i).name.Split('~')[0]), markerParent.GetChild(i).position.x, markerParent.GetChild(i).position.y, markerParent.GetChild(i).position.z));
            //print("Marker with a typeIndex of " + int.Parse(markerParent.GetChild(i).name.Split('~')[0]) + " is being saved");
        }

        SaveLoad.Save(markerSaveData, GameManager.instance.curSaveSlot, mapMarkerSaveKey);*/
    }
    public void LoadMarkers()
    {
        /*?for (int i = 0; i < markerParent.childCount; i++)
        {
            Destroy(markerParent.GetChild(i).gameObject);
        }

        List<MarkerSaveData> data = SaveLoad.Load<List<MarkerSaveData>>(GameManager.instance.curSaveSlot, mapMarkerSaveKey);

        for (int i = 0; i < data.Count; i++)
        {
            var marker = Instantiate(markerPrefabs[data[i].typeIndex], new Vector3(data[i].posX, data[i].posY, data[i].posZ), Quaternion.identity, markerParent);
            marker.name = data[i].typeIndex.ToString() + "~" + markerPrefabs[data[i].typeIndex].name;
            marker.SetActive(true);
        }

        print("Update marker stuff on load");
        ResetMarkerSearch();
        UpdateMarkerAmountText();*/
    }

    [System.Serializable]
    class MarkerSaveData
    {
        public int typeIndex;
        public float posX;
        public float posY;
        public float posZ;

        public MarkerSaveData(int t, float x, float y, float z)
        {
            typeIndex = t;
            posX = x;
            posY = y;
            posZ = z;
        }
    }
}
