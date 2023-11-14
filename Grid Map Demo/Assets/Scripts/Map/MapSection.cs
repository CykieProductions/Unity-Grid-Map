using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSection : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public SpriteMask borderMask;
    [SerializeField, Tooltip("Offsets raycasts for adjacent checks to get around normal borders")] 
    float rayNormalPadding = 1.5f;
    [SerializeField, Tooltip("Used to check around abnormal border shapes")] 
    float raySecondaryPadding = .75f;

    [Space]
    public bool disableCollisions;
    public GameObject[] connectedSections;
    public bool touchedByPlayer;
    MapSection _overrideSection;
    MapSection OverrideSection { get => _overrideSection; set
        {
            _overrideSection = value;
            connectedSections = OverrideSection.connectedSections;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Full Activate")]
    void e_ForceFullActivation()
    {
        touchedByPlayer = false;//for testing adjacent detection repeatedly
        Activate(true);
    }
    [ContextMenu("Soft Activate")]
    void e_ForceSoftActivation() => Activate(false);
#endif

    public void Activate(bool activateAdjacent)
    {
        if (touchedByPlayer || disableCollisions)
            return;
        touchedByPlayer = activateAdjacent;

        if (borderMask == null)
            borderMask = GetComponentInChildren<SpriteMask>();
        //Displays the surrounding border
        borderMask.enabled = true;

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        if (!activateAdjacent)
            return;

        //!Send rays to activate the adjacent sections above, below, left and right
        Vector3 rayOrigin;
        float cellSize = transform.localScale.x;
        Color dRayColor1 = Color.cyan;
        Color dRayColor2 = Color.blue;

        //! Up rays
        CheckAdjacent(Vector2.up, Vector2.left, rayNormalPadding);
        CheckAdjacent(Vector2.up, Vector2.right, rayNormalPadding);
        CheckAdjacent(Vector2.up, Vector2.left, raySecondaryPadding);
        CheckAdjacent(Vector2.up, Vector2.right, raySecondaryPadding);

        //! Down rays
        CheckAdjacent(Vector2.down, Vector2.left, rayNormalPadding);
        CheckAdjacent(Vector2.down, Vector2.right, rayNormalPadding);
        CheckAdjacent(Vector2.down, Vector2.left, raySecondaryPadding);
        CheckAdjacent(Vector2.down, Vector2.right, raySecondaryPadding);

        //! Left rays
        CheckAdjacent(Vector2.left, Vector2.up, rayNormalPadding);
        CheckAdjacent(Vector2.left, Vector2.down, rayNormalPadding);
        CheckAdjacent(Vector2.left, Vector2.up, raySecondaryPadding);
        CheckAdjacent(Vector2.left, Vector2.down, raySecondaryPadding);

        //! Right rays
        CheckAdjacent(Vector2.right, Vector2.up, rayNormalPadding);
        CheckAdjacent(Vector2.right, Vector2.down, rayNormalPadding);
        CheckAdjacent(Vector2.right, Vector2.up, raySecondaryPadding);
        CheckAdjacent(Vector2.right, Vector2.down, raySecondaryPadding);

        void CheckAdjacent(Vector2 _dir, Vector2 _offsetDir, float padding = 1.5f)
        {
            //rayCornerPadding of 1.5f ensures rays don't spawn in the inner corner tiles of the border, but you may not have those
            float paddedEdgeDist = (cellSize / 2 - padding);

            rayOrigin = (Vector2)transform.position - (0.1f * cellSize * _dir) + (_offsetDir * paddedEdgeDist);
            RaycastHit2D[] rayHits = Physics2D.RaycastAll(rayOrigin, _dir, cellSize, LayerMask.GetMask("Map"));

            MapSection adjacentSection = null;
            RaycastHit2D rayHit = new RaycastHit2D();
            foreach (var r in rayHits)
            {
                //Ignore yourself
                if (r.collider.gameObject == gameObject)
                    continue;
                //Only count border tiles or other MapSections
                if (r.collider.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>() || r.collider.TryGetComponent(out adjacentSection))
                {
                    rayHit = r; 
                    break;
                }
            }

            //Only continue if the ray hit something
            if (rayHit.collider == null)
                return;
            
            //debug line
            Color c = (_dir.x < 0 || _dir.y < 0) ? dRayColor1 : dRayColor2;
            Debug.DrawLine(rayOrigin, rayHit.point, c, 5f);

            //The ray hit a MapSection!
            if (adjacentSection)
                adjacentSection.Activate(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!disableCollisions)
        {
            //! Touched by Player
            if (other.transform == MapIcon.instance.transform)
            {
                Activate(true);

                if (connectedSections.Length > 0)
                {
                    for (int i = 0; i < connectedSections.Length; i++)
                    {
                        connectedSections[i].SetActive(true);
                    }
                }
            }

            //!Touched by an Override object
            if (other.name.Contains("verride"))
            {
                if (other.name.Contains("ollider"))
                    GetComponent<BoxCollider2D>().size = other.GetComponentInChildren<BoxCollider2D>().size;
                if (other.name.Contains("olor"))
                    GetComponent<SpriteRenderer>().color = other.GetComponentInChildren<SpriteRenderer>().color;
                if (other.name.Contains("onnected"))
                {
                    OverrideSection = other.GetComponent<MapSection>();//Missing?
                    /*var omp = other.GetComponent<MapSection>();
                    connectedSections = new GameObject[omp.connectedSections.Length];
                    for (int i = 0; i < connectedSections.Length; i++)
                    {
                        connectedSections[i] = omp.connectedSections[i];
                    }*/

                    if (borderMask == null)
                        borderMask = GetComponentInChildren<SpriteMask>();

                    for (int i = 0; i < OverrideSection.connectedSections.Length; i++)
                    {
                        OverrideSection.connectedSections[i].transform.parent = borderMask.transform;
                        OverrideSection.connectedSections[i].SetActive(true);
                    }
                }
                if (other.name.Contains("ctivat"))
                    Activate(false);

                //If not a group override, destroy it after use
                if (!other.name.Contains("roup") && !other.name.Contains("onnected"))
                    Destroy(other.gameObject);
            }
        }

    }
}
