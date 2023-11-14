using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//OUT DATED: Smallest room size should be 32(between gates) x 24 (Size of the Save Room as of 2/6/21)
//Smallest room is 24 by 12, including the gates
public class RoomController : MonoBehaviour
{
    public static RoomController CurrentRoom {  get; private set; }
    Transform player;
    //?PlayerController pController;

    public bool isMappableRoom = true;
    public Vector2 mapOverridePos;
    
    [Space]
    public Collider2D mainCollider;
    public GameObject originalContents;
    public GameObject originalQuickResetContents;
    string contentsName;
    string quickResetContentsName;
    GameObject contents;
    GameObject quickResetContents;
    //public Collider2D roomBoundaries;
    public bool isRoomActive = false;

    [Space]
    //?public Parallax[] parallaxAchors = new Parallax[1];
        
    [Space]
    public LayerMask roomBoundsLayer;
    public LayerMask camBoundsLayer;
    public LayerMask playerLayer;
    public float zoomLevel;

    GameObject objToEnable;
    private bool reloadOnExit;

    /*[Space]
public GameObject[] camBoundaryObjects;
int currentSection = 0;*/

    void Awake()
    {
        if (transform.Find("[enable with room]"))
            objToEnable = transform.Find("[enable with room]").gameObject;

        //if (!boundaries) boundaries = transform.Find("Boundaries").gameObject;
        if (!originalContents) originalContents = transform.Find("Contents").gameObject;

        contentsName = originalContents.name;
        originalContents.name += "(original)";

        if (originalQuickResetContents)
        {
            quickResetContentsName = originalQuickResetContents.name;
            originalQuickResetContents.name += "(original)";
        }

        ReloadContent();
        ReloadQuickResetConents();

        //?GameEvents.OnGameSaved += ReloadContent;

        originalContents.SetActive(false);
        if (originalQuickResetContents)
            originalQuickResetContents.SetActive(false);

        if (!mainCollider)
            mainCollider = GetComponent<Collider2D>();
        //?player = GameManager.instance.player;
        //?pController = GameManager.instance.pController;


        //?if (parallaxAchors[0] != null)
        {
            //?parallaxAchors[0].gameObject.SetActive(false);
        }
        /*bool hasCamBounds = false;
        
        foreach (Transform child in boundaries.GetComponentsInChildren<Transform>())
        {
            if (child.name.Contains("Cam"))
            {
                hasCamBounds = true;
                return;
            }
        }
        
        *if (!hasCamBounds)
        {
            var newCamBounds = Instantiate(mainCollider.gameObject, boundaries.transform);
            newCamBounds.TryGetComponent<CompositeCollider2D>(out var camCompositeCollider);
            if (camCompositeCollider != null)
            {
                camCompositeCollider.geometryType = CompositeCollider2D.GeometryType.Outlines;
            }
            newCamBounds.name = "Camera Bounds";
            newCamBounds.layer = LayerMask.NameToLayer("CameraBounds");
        }*/

        /*if (parallaxAchors[0] == null)
        {
            parallaxAchors[0] = transform;
            //parallaxAchor.position = transform.position;
        }*/
    }

    private void OnEnable()
    {
        GameEvents.OnChangeRoom += ChangedRoom;
    }
    private void OnDisable()
    {
        //?GameEvents.OnGameSaved -= ReloadContent;
        GameEvents.OnChangeRoom -= ChangedRoom;
    }

    void Update()
    {
        bool lastFrameState = isRoomActive;
        bool changedStates = false;

        //? if (GameManager.instance.PlayState != GameManager.GameState.IN_CUTSCENE/* || GameManager.instance.gameState != GameManager.GameState.IN_REALTIME_CUTSCENE*/)
        {
            isRoomActive = Physics2D.IsTouchingLayers(mainCollider, playerLayer);
            contents.SetActive(isRoomActive);
            if (quickResetContents)
                quickResetContents.SetActive(isRoomActive);
        }

        if (lastFrameState != isRoomActive)
            changedStates = true;

        if (isRoomActive)
        {
            //?if (parallaxAchors[0] != null)
                //?parallaxAchors[0].gameObject.SetActive(true);
            objToEnable?.SetActive(true);

            //?pController.isInMappableRoom = isMappableRoom;
            if (mapOverridePos != Vector2.zero)
                //?GameManager.instance.mapIcon.position = mapOverridePos;

            if (changedStates)
            {
                //? GameEvents.ChangeRooms(this);
            }
        }
        else
        {
            //?if (parallaxAchors[0] != null)
                //?parallaxAchors[0].gameObject.SetActive(false);
            objToEnable?.SetActive(false);
        }
    }

    void ChangedRoom(RoomController newRoom)
    {
        if (newRoom != this)
        {
            if (reloadOnExit)
            {
                ReloadContent();
                reloadOnExit = false;
            }
            return;
        }

        CurrentRoom = newRoom;
        ReloadQuickResetConents();

        //?if (parallaxAchors[0] != null)
        {
            //?parallaxAchors[0].DetachAnchor();
            //?parallaxAchors[0].gameObject.SetActive(false);
        }
    }
    void ReloadContent()
    {
        if (!originalContents)
            return;

        if (CurrentRoom == this)
        {
            reloadOnExit = true;
            return;
        }

        contents = Instantiate(originalContents, transform);
        if (transform.Find(contentsName))
        {
            Destroy(transform.Find(contentsName).gameObject);
        }
        contents.name = contentsName;

        //Destroy(contentClone);
        //newContents.name = name;
    }

    void ReloadQuickResetConents()
    {
        if (!originalQuickResetContents)
            return;
        quickResetContents = Instantiate(originalQuickResetContents, transform);
        if (transform.Find(quickResetContentsName))
        {
            Destroy(transform.Find(quickResetContentsName).gameObject);
        }
        quickResetContents.name = quickResetContentsName;
    }
}
