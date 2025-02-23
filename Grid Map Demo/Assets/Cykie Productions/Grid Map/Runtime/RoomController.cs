using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CykieProductions.General2D
{

    public class RoomController : MonoBehaviour
    {
        public static RoomController CurrentRoom { get; private set; }
        public static bool CanSwitchRooms { get; set; }

        public static event Action<RoomController> OnChangeRoom;
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

            /*?bool hasCamBounds = false;

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
        }

        private void OnEnable()
        {
            OnChangeRoom += ChangedRoom;
        }
        private void OnDisable()
        {
            //?GameEvents.OnGameSaved -= ReloadContent;
            OnChangeRoom -= ChangedRoom;
        }

        void Update()
        {
            bool lastFrameState = isRoomActive;
            bool changedStates = false;

            //? if (GameManager.instance.PlayState != GameManager.GameState.IN_CUTSCENE/* || GameManager.instance.gameState != GameManager.GameState.IN_REALTIME_CUTSCENE*/)
            if (CanSwitchRooms)
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
                if (objToEnable)
                    objToEnable.SetActive(true);

                //?pController.isInMappableRoom = isMappableRoom;
                //if (mapOverridePos != Vector2.zero)
                //?GameManager.instance.mapIcon.position = mapOverridePos;

                if (changedStates)
                {
                    ChangedRoom(this);
                }
            }
            else
            {
                if (objToEnable)
                    objToEnable.SetActive(false);
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

}