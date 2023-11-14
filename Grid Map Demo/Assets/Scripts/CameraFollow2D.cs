using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow2D : MonoBehaviour
{
    //PlayerController pController;
    public RoomController curRoom;

    public Camera cam;
    public LayerMask boundsLayer;
    [SerializeField] Transform target;
    Transform curTarget;
    Rigidbody2D tarRb;
    Vector3 followPos;

    [SerializeField] bool targetPlayer;
    [SerializeField] float panUpDownStartTime = 1f;
    float panUpDownTimer;
    [SerializeField] float panUpDownAmount = 4f;

    public bool followPrimeTarget = true;
    [HideInInspector] public bool lockTarget;
    [HideInInspector] public Transform secondaryTarget;

    [Space]
    [SerializeField] Vector3 offset = new Vector3(0, 1, -25);
    [SerializeField] float followSpeed;
    [SerializeField] float fallVelLimit = 18;
    bool inDeadZone;

    [Space, Header("BOUNDS")]
    public bool unrestricted;
    public float topBoundary = float.MaxValue;
    public float bottomBoundary = float.MinValue;
    public float leftBoundary = float.MinValue;
    public float rightBoundary = float.MaxValue;

    [HideInInspector] public float verticalRadius;
    [HideInInspector] public float horizontalRadius;

    /*[Space]
    [SerializeField] Vector4 innerFollowBounds;
    Vector3 anchorPos;
    [SerializeField] Vector4 outerFollowBounds;
    bool stopMovementSmoothing;*/
    public Coroutine shakeRoutine;
    private bool speedUp;

    void Awake()
    {
       //pController = GameManager.instance.player.GetComponent<PlayerController>();

        if (targetPlayer)
            target = GameObject.FindWithTag("Player").transform;
        curTarget = target;
        tarRb = curTarget.GetComponent<Rigidbody2D>();

        if (curTarget == null)
        {
            gameObject.SetActive(false);
            Debug.LogError("Camera: \"" + gameObject.name + "\" does not have a target and was disabled");
        }

        transform.position = curTarget.position;
        /*anchorPos = transform.position;
        inDeadZone = false;*/
    }

    private void OnEnable()
    {
        //GameEvents.OnLoadComplete += OnLoad;
    }
    private void OnDisable()
    {
        //GameEvents.OnLoadComplete -= OnLoad;
    }

    void Update()
    {
        if (Vector2.Distance(transform.position, target.position) > 20)
        {
            transform.position = target.position + offset;
        }

        #region Scrapped Dead Zone Logic
        //First Attempt
        /*Vector2 pos = target.position;
        //x is right; y is left; z is top; w is bottom
        float rightInner = transform.position.x + innerFollowBounds.x;
        float leftInner = transform.position.x + innerFollowBounds.y;
        float topInner = transform.position.y + innerFollowBounds.z;
        float bottomInner = transform.position.y + innerFollowBounds.w;

        if (pos.x > rightInner || pos.y < leftInner)
        {
            inDeadZone = false;
        }
        else if (pos.y > topInner || pos.y < bottomInner)
        {
            inDeadZone = false;
        }*/

        //Second Attempt
        /*if(Vector3.Distance(anchorPos, target.position) > 1)
        {
            inDeadZone = false;
        }*/
        #endregion

        if (followPrimeTarget)
        {
            curTarget = target;
        }
        else if (!followPrimeTarget)
        {
            curTarget = secondaryTarget;
        }

        verticalRadius = cam.orthographicSize;
        horizontalRadius = cam.orthographicSize * Screen.width / Screen.height;
        Vector2 pos = transform.position;
        Vector2 playerPos = target.position;
        Vector2 boxOrigin = pos;
        if (!followPrimeTarget)
            boxOrigin = followPos;

        bool isBound = !unrestricted;
        #region Find Camera Border
        Vector2 topEdge = new Vector2(boxOrigin.x, boxOrigin.y + verticalRadius);
        Vector2 bottomEdge = new Vector2(boxOrigin.x, boxOrigin.y - verticalRadius);
        Vector2 rightEdge = new Vector2(boxOrigin.x + horizontalRadius, boxOrigin.y);
        Vector2 leftEdge = new Vector2(boxOrigin.x - horizontalRadius, boxOrigin.y);
        /*Vector2 topRight = new Vector2(rightEdge.x, topEdge.y);
        Vector2 topLeft = new Vector2(leftEdge.x, topEdge.y);
        Vector2 bottomRight = new Vector2(rightEdge.x, bottomEdge.y);
        Vector2 bottomLeft = new Vector2(leftEdge.x, bottomEdge.y);*/
        #endregion

        #region Calculate Boundaries
        Collider2D playerRoomCollider = null;
        //Top Boundary
        RaycastHit2D hit;
        if (targetPlayer)
        {
            hit = Physics2D.Raycast(playerPos, topEdge - boxOrigin, 500f, boundsLayer);
            if (hit.collider)
            {
                topBoundary = hit.point.y;
                playerRoomCollider = hit.collider;
            }
        }

        hit = Physics2D.Raycast(leftEdge, topEdge - boxOrigin, 500f, boundsLayer);
        if (hit.collider && (playerRoomCollider == null || hit.collider == playerRoomCollider))
        {
            topBoundary = hit.point.y;

            hit = Physics2D.Raycast(rightEdge, topEdge - boxOrigin, 500f, boundsLayer);
            if (hit.collider && topBoundary > hit.point.y) //Did hit detect a lower top boundary?
                topBoundary = hit.point.y;
        }

        playerRoomCollider = null;
        //Bottom Boundary
        hit = Physics2D.Raycast(leftEdge, bottomEdge - boxOrigin, 500f, boundsLayer);
        if (hit.collider)
            bottomBoundary = hit.point.y;

        hit = Physics2D.Raycast(rightEdge, bottomEdge - boxOrigin, 500f, boundsLayer);
        if (hit.collider && bottomBoundary < hit.point.y) //Did hit detect a higher bottom boundary?
            bottomBoundary = hit.point.y;

        playerRoomCollider = null;
        //Left Boundary
        hit = Physics2D.Raycast(topEdge, leftEdge - boxOrigin, 500f, boundsLayer);
        if (hit.collider)
            leftBoundary = hit.point.x;
        //Debug.DrawLine(topEdge, hit.point, Color.blue, 0.2f);

        hit = Physics2D.Raycast(bottomEdge, leftEdge - boxOrigin, 500f, boundsLayer);
        if (hit.collider && leftBoundary < hit.point.x) //Did hit detect a closer left boundary
            leftBoundary = hit.point.x;
        //Debug.DrawLine(bottomEdge, hit.point, Color.blue, 0.2f);

        if (leftBoundary == 0)
        {
            hit = Physics2D.Raycast(boxOrigin, leftEdge - boxOrigin, 500f, boundsLayer);
            if (hit.collider)
                leftBoundary = hit.point.x;
            Debug.DrawLine(boxOrigin, hit.point, Color.yellow, 0.2f);
        }

        playerRoomCollider = null;
        //Right Boundary
        hit = Physics2D.Raycast(topEdge, rightEdge - boxOrigin, 500f, boundsLayer);
        if (hit.collider)
            rightBoundary = hit.point.x;
        //Debug.DrawLine(topEdge, hit.point, Color.red, 0.2f);

        hit = Physics2D.Raycast(bottomEdge, rightEdge - boxOrigin, 500f, boundsLayer);
        if (hit.collider && rightBoundary > hit.point.x) //Did hit detect a closer right boundary
            rightBoundary = hit.point.x;
        //Debug.DrawLine(bottomEdge, hit.point, Color.red, 0.2f);

        if (rightBoundary == 0)
        {
            hit = Physics2D.Raycast(boxOrigin, leftEdge - boxOrigin, 500f, boundsLayer);
            if (hit.collider)
                rightBoundary = hit.point.x;
            Debug.DrawLine(boxOrigin, hit.point, Color.yellow, 0.2f);
        }

        /*if (!hit.collider)
            isBound = false;*/
        #endregion

        if (isBound)
        {
            followPos = curTarget.position;

            //! Pan up or down while standing still
            /*?if (targetPlayer && pController.pi.horizontal < 0.1f && pController.pi.horizontal > -0.1f && pController.motor.grounded && !pController.pi.pressingShoot && !pController.shootController.isShielding)
            {
                if (pController.pi.vertical >= 0.9f)
                {
                    panUpDownTimer += Time.deltaTime;
                    if (panUpDownTimer >= panUpDownStartTime)
                        followPos = curTarget.position + (transform.up * (panUpDownAmount - offset.y - offset.y));
                }
                else if (pController.pi.vertical <= -0.9f)
                {
                    panUpDownTimer += Time.deltaTime;
                    if (panUpDownTimer >= panUpDownStartTime)
                        followPos = curTarget.position - (transform.up * panUpDownAmount);
                }
                else
                {
                    panUpDownTimer = 0;
                }
            }
            else
            {
                panUpDownTimer = 0;
            }*/

            /*x if (curRoom != pController.curRoom && pController.curRoom != null)
            {
                print("s");
                secondaryTarget = target;
                unrestricted = true;
                followPrimeTarget = false;
                //transform.position = followPos;
            }
            else
            {*/

                followPos = new Vector3
                (
                    Mathf.Clamp(followPos.x, leftBoundary + (horizontalRadius * 1.01f), rightBoundary - (horizontalRadius * 1.01f)),
                    Mathf.Clamp(followPos.y, bottomBoundary + (verticalRadius * .9f), topBoundary - (verticalRadius * 1.11f)),
                    followPos.z
                );
            //}
        }
        else
        {
            followPos = curTarget.position;
        }
    }

    void LateUpdate()
    {
        if (curTarget)
        {
            float deltaTime = Time.unscaledDeltaTime;
            float curSpeed = followSpeed;
            if (!followPrimeTarget)
                curSpeed = followSpeed * 1.5f;
            else if (followPrimeTarget && (Mathf.Abs(tarRb.velocity.y) >= 15 && Vector2.Distance(transform.position, new Vector2(transform.position.x, followPos.y)) > 2f) )
            {
                speedUp = true;
            }

            if (speedUp)
            {
                curSpeed = followSpeed * 5f;
                transform.position = followPos + offset;
                if (!followPrimeTarget || Mathf.Abs(tarRb.velocity.y) <= fallVelLimit &&(Vector2.Distance(transform.position, new Vector2(transform.position.x, followPos.y)) < 1f))
                    speedUp = false;
            }
            
            transform.position = Vector3.Lerp(transform.position, followPos + offset, curSpeed * deltaTime);


            if (curTarget == secondaryTarget)
            {
                //print(Vector2.Distance(transform.position, curTarget.position));
                if (Vector2.Distance(transform.position, curTarget.position) < 1.5f && !lockTarget)
                {
                    followPrimeTarget = true;
                    unrestricted = false;
                    secondaryTarget = null;
                }
            }
            #region Scrapped Dead Zone Logic
            /*if (Vector3.Distance(transform.position, target.position + offset) < 0.1f)
            {
                inDeadZone = true;
                anchorPos = target.position;
            }*/
            #endregion
        }
    }

    void OnLoad()
    {
        if (targetPlayer)
        {
            //transform.position = target.position + offset;
            //transform.position = target.GetComponent<PlayerStats>().checkpointPos + (Vector2)offset;
        }
    }

    public void RecieveMessage(string message)
    {
        if (message == "Shake")
        {
            Shake(0.1f);
        }
    }

    public void Shake(float d, float x = 0.25f, float y = 0.25f)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeRoutine(d, x, y));
    }

    IEnumerator ShakeRoutine(float duration, float xIntensity, float yIntensity)
    {
        while(duration > 0)
        {
            duration -= Time.unscaledDeltaTime;

            float x = xIntensity * Random.Range(-1f, 1f);
            float y = yIntensity * Random.Range(-1f, 1f);

            transform.position += new Vector3(x, y, 0);

            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawIcon(followPos, "Cam Follow Target", false, Color.yellow);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector2(leftBoundary, topBoundary), new Vector2(rightBoundary, topBoundary));
        Gizmos.DrawLine(new Vector2(rightBoundary, topBoundary), new Vector2(rightBoundary, bottomBoundary));
        Gizmos.DrawLine(new Vector2(rightBoundary, bottomBoundary), new Vector2(leftBoundary, bottomBoundary));
        Gizmos.DrawLine(new Vector2(leftBoundary, bottomBoundary), new Vector2(leftBoundary, topBoundary));
    }

    private void OnTriggerStay2D(Collider2D other)
    {

    }
    private void OnTriggerExit2D(Collider2D other)
    {

    }
}
