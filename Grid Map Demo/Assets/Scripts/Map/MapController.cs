using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Active Map Section box colliders are no longer set to false on load;
public class MapController : MonoBehaviour
{
    [SerializeField] string saveKey;
    [SerializeField] bool isSaveable = true;
    public string originalSceneName;
    [SerializeField, Tooltip("Bundled maps should be children of the base map")] MapController[] bundledMaps;

    [Space]
    public int width;
    public int height;
    public float CellSize => GameManager.instance.MapGridSize;

    [Space]
    public GameObject cellObject;
    [SerializeField] Transform gridParent;
    MapSection[] mapSections;
    [SerializeField] bool regenerateGridOnStart = true;

    private void OnEnable()
    {
        /*?GameEvents.OnGameSaved += Save;
        GameEvents.OnGameLoaded += Load;
        GameEvents.OnLoadComplete += MaskBorders;*/
        MaskBorders();
    }

    private void OnDisable()
    {
        /*?GameEvents.OnGameSaved -= Save;
        GameEvents.OnGameLoaded -= Load;
        GameEvents.OnLoadComplete -= MaskBorders;*/
    }


    void Start()
    {
        if (transform.parent == null)
            name += "; Key: " + saveKey;

        /*if (string.IsNullOrEmpty(originalSceneName))
            originalSceneName = SceneManager.GetActiveScene().name;*/

        if (isSaveable)
        {
            SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetSceneByName(MapIcon.instance.MapSceneName));
            if (GameObject.Find(name) != gameObject)
                Destroy(gameObject);
        }
        //cellSize = cellObject.transform.localScale.x;

        if (regenerateGridOnStart || gridParent.childCount == 0)
            CreateGrid();//It now replaces the possible existing grid on it's own
        else
            mapSections = gridParent.GetComponentsInChildren<MapSection>();
    }


    private void Update()
    {
#if UNITY_EDITOR
        /*if (Keyboard.current.nKey.wasPressedThisFrame)
        {
            Save();
        }
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            Load();
        }*/
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            var tilemaps = GetComponentsInChildren<UnityEngine.Tilemaps.TilemapRenderer>();

            for (int i = 0; i < tilemaps.Length; i++)
            {
                tilemaps[i].maskInteraction = SpriteMaskInteraction.None;   
            }
        }
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            MaskBorders();
        }

        if (originalSceneName == "")
            originalSceneName = SceneManager.GetActiveScene().name;
#endif

        if (SceneManager.GetActiveScene().name != originalSceneName && !mapSections[mapSections.Length - 1].disableCollisions)
        {
            //print("disable all map sections in " + originalSceneName);
            for (int i = 0; i < mapSections.Length; i++)
            {
                mapSections[i].disableCollisions = true;
            }
        }
        else if (SceneManager.GetActiveScene().name == originalSceneName && mapSections[^1].disableCollisions)
        {
            //print("enable all map sections in " + originalSceneName);
            for (int i = 0; i < mapSections.Length; i++)
            {
                mapSections[i].disableCollisions = false;
            }
        }
    }

    /// <summary>
    /// Ensures that the map borders don't appear outside of active map sections
    /// </summary>
    public void MaskBorders()
    {
        var tilemaps = GetComponentsInChildren<UnityEngine.Tilemaps.TilemapRenderer>();

        for (int i = 0; i < tilemaps.Length; i++)
        {
            if (tilemaps[i].name.Contains("Visible") || tilemaps[i].CompareTag("Visible Map"))
                tilemaps[i].maskInteraction = SpriteMaskInteraction.None;
            else
                tilemaps[i].maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            //When editing you may want to make the tilemap transparent, so this corrects for that
            var tilemapComp = tilemaps[i].GetComponent<UnityEngine.Tilemaps.Tilemap>();
            tilemapComp.color = new Color(tilemapComp.color.r, tilemapComp.color.g, tilemapComp.color.b, 1);
        }
    }

    [ContextMenu("Create Grid")]
    public void CreateGrid()
    {
        //Removes any existing grid
        if (gridParent.childCount > 0)
        {
            var ngo = new GameObject();
            ngo.transform.parent = transform;
            ngo.transform.position = gridParent.position;
            ngo.layer = LayerMask.NameToLayer("Map");
            if (Application.isEditor && !Application.isPlaying)
                DestroyImmediate(gridParent.gameObject);//Normal Destroy doesn't work well in the editor
            else
                Destroy(gridParent.gameObject);
            ngo.name = "GRID PARENT";
            gridParent = ngo.transform;
        }

        //Generates a new grid
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                SpawnSection(x, y);
            }
        }
        mapSections = gridParent.GetComponentsInChildren<MapSection>();
    }

    private void SpawnSection(int x, int y)
    {
        Vector3 offset = new Vector3();
        //Enable this to center the pivot point of the grid
        //offset = new Vector3(-cellSize * (width / 2), cellSize * (height / 2));

        GameObject g = Instantiate(cellObject, gridParent);
        g.name = g.name.Replace("(Clone)", "") + $" <{x}, {y}>";
        g.transform.position = new Vector3(gridParent.position.x + (x * CellSize), gridParent.position.y - (y * CellSize)) + offset;
        g.transform.localScale = new Vector3(CellSize, CellSize, 1);
        g.SetActive(true);
    }

    //Disabled
    public void Save()
    {
        if (isSaveable)
        {
            List<bool> activeSections = new List<bool>();
            var spriteRenderers = gridParent.GetComponentsInChildren<SpriteRenderer>();

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                activeSections.Add(spriteRenderers[i].enabled);
            }

            if (bundledMaps.Length > 0)
            {
                for (int b = 0; b < bundledMaps.Length; b++)
                {
                    var sp = bundledMaps[b].gridParent.GetComponentsInChildren<SpriteRenderer>();
                    for (int i = 0; i < sp.Length; i++)
                    {
                        activeSections.Add(sp[i].enabled);
                    }
                }
            }

            //?SaveLoad.Save<List<bool>>(activeSections, "Slot " + GameManager.instance.curSaveSlot, saveKey);
        }
    }
    //Disabled
    public void Load()
    {
#if false
        if (isSaveable)
        {
            var mapSections = gridParent.GetComponentsInChildren<MapSection>();
            int numInMainMap = 0;
            List<bool> activeSections = new List<bool>();

            if (SaveLoad.SaveExists("Slot " + GameManager.instance.curSaveSlot, saveKey))
                activeSections = SaveLoad.Load<List<bool>>("Slot " + GameManager.instance.curSaveSlot, saveKey);
            else //Disable everything if you can't load
            {
                for (int i = 0; i < mapSections.Length; i++)
                {
                    mapSections[i].spriteRenderer.enabled = false;
                    mapSections[i].GetComponent<BoxCollider2D>().enabled = true;
                    mapSections[i].transform.GetChild(0).gameObject.SetActive(false);
                    mapSections[i].touchedByPlayer = false;
                }

                if (bundledMaps.Length > 0)
                {
                    for (int b = 0; b < bundledMaps.Length; b++)
                    {
                        var ms = bundledMaps[b].gridParent.GetComponentsInChildren<MapSection>();
                        for (int i = 0; i < ms.Length; i++)
                        {
                            ms[i].spriteRenderer.enabled = false;
                            ms[i].GetComponent<BoxCollider2D>().enabled = true;
                            ms[i].transform.GetChild(0).gameObject.SetActive(false);
                            ms[i].touchedByPlayer = false;
                        }
                    }
                }
                return;
            }

            for (int i = 0; i < mapSections.Length; i++)
            {
                numInMainMap++;
                mapSections[i].spriteRenderer.enabled = activeSections[i];
                if (mapSections[i].spriteRenderer.enabled/* && mapSections[i].transform.childCount > 0*/)
                {
                    //spriteRenderers[i].GetComponent<BoxCollider2D>().enabled = false;
                    mapSections[i].transform.GetChild(0).gameObject.SetActive(true);
                }
                else
                {
                    mapSections[i].GetComponent<BoxCollider2D>().enabled = true;
                    mapSections[i].transform.GetChild(0).gameObject.SetActive(false);
                    mapSections[i].touchedByPlayer = false;
                }
            }

            if (bundledMaps.Length > 0)
            {
                //print(saveKey + ": The number of sections in the main map is " + numInMainMap);
                for (int b = 0; b < bundledMaps.Length; b++)
                {
                    var ms = bundledMaps[b].gridParent.GetComponentsInChildren<MapSection>();
                    for (int i = 0; i < ms.Length; i++)
                    {
                        ms[i].spriteRenderer.enabled = activeSections[numInMainMap + i];
                        if (ms[i].spriteRenderer.enabled/* && ms[i].transform.childCount > 0*/)
                        {
                            //sp[i].GetComponent<BoxCollider2D>().enabled = false;
                            ms[i].transform.GetChild(0).gameObject.SetActive(true);
                        }
                        else
                        {
                            ms[i].GetComponent<BoxCollider2D>().enabled = true;
                            ms[i].transform.GetChild(0).gameObject.SetActive(false);
                            ms[i].touchedByPlayer = false;
                        }
                        //Debug.Log("Section " + numInMainMap + " now has an active state of " + ms[i].enabled, ms[i]);
                        if (i == ms.Length - 1)//if last one
                        {
                            numInMainMap += (i + 1);
                            /*print(saveKey + ": Last index of map" + (b + 2) + " is " + i);
                            print(saveKey + ": The total number of sections so far is " + numInMainMap);*/
                        }
                    }
                }
            }
            //MaskBorders();
        }
#endif
    }
}
