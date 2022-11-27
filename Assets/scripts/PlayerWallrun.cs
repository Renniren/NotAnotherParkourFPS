using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class PlayerWallrun : MonoBehaviour
{
    public CameraRoll cam;
    public Transform WallrunRoll;
    public bool IsWallrunning;
    public float RollAmount = 20;
    public float WallrunSpeed = 9;
    public float CounterGravityAmount = 4;
    public float RayDistance = 1.8f;
    public float WalljumpForce;
    public float WalljumpUpwardsForce;
    public PlayerMovement playerComponent;
    public Transform head;
    public UnityEvent WallrunLeft;
    public UnityEvent WallrunRight;

    // Start is called before the first frame update
    void Start()
    {
        
    }


    RaycastHit hitright;
    RaycastHit hitleft;
    RaycastHit hitDown;
    RaycastHit viewhit;
    RaycastHit ValidHit;
    bool AddedJumpYet;
    float StickToWallForce = 65;

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, transform.right * RayDistance);
        Gizmos.DrawRay(transform.position, -transform.right * RayDistance);
    }

    void HandleWallrunning()
    {
        Vector3 CurrentPosition = transform.position;
        Vector3 OrVec = new Vector3();

        Physics.Raycast(CurrentPosition, head.forward, out viewhit, Mathf.Infinity);
        Physics.Raycast(CurrentPosition, -transform.right, out hitright, RayDistance, LayerMask.GetMask(playerComponent.GroundLayers));
        Physics.Raycast(CurrentPosition, transform.right, out hitleft, RayDistance, LayerMask.GetMask(playerComponent.GroundLayers));
        Physics.Raycast(CurrentPosition, -transform.up, out hitDown, 100, LayerMask.GetMask(playerComponent.GroundLayers));

        if (hitright.collider != null && !playerComponent.IsGrounded)
        {
            if (Mathf.Round(Vector3.Dot(hitright.normal, Vector3.up)) == 0 && !hitright.collider.gameObject.CompareTag("NoWallrun"))
            {
                WallrunRight.Invoke();

                OrVec = Vector3.Cross(hitright.normal, Vector3.up);
                IsWallrunning = true;
                ValidHit = hitright;
            }
        }


        if (hitleft.collider != null && !playerComponent.IsGrounded)
        {
            if (Mathf.Round(Vector3.Dot(hitleft.normal, Vector3.up)) == 0 && !hitleft.collider.gameObject.CompareTag("NoWallrun"))
            {
                WallrunLeft.Invoke();

                OrVec = Vector3.Cross(hitleft.normal, Vector3.up);
                IsWallrunning = true;
                ValidHit = hitleft;
            }
        }

        if (hitleft.collider == null && hitright.collider == null)
        {
            IsWallrunning = false;
        }
        if (hitleft.collider != null && hitright.collider != null) // this keeps you from wallrunning on two different surfaces at once.
        {
            IsWallrunning = false;
        }

        if (IsWallrunning)
        {
            if (AddedJumpYet != true && IsWallrunning)
            {
                playerComponent.body.velocity = new Vector3(playerComponent.body.velocity.x, playerComponent.body.velocity.y + 4, playerComponent.body.velocity.z);
                AddedJumpYet = true;
            }

            playerComponent.move.x = Mathf.Clamp(playerComponent.move.x, -0.2f, 0.2f);

            playerComponent.body.AddForce(transform.up * CounterGravityAmount * playerComponent.body.mass);
            playerComponent.body.AddForce(transform.forward * WallrunSpeed * playerComponent.body.mass);
            
            if (hitright.collider != null && Input.GetKeyDown(KeyCode.Space))
            {
                playerComponent.CancelDrag(transform.position);
                //body.AddForce(transform.forward * jumpForce * 0.32f * body.mass * Time.fixedDeltaTime, ForceMode.Impulse);
                playerComponent.body.velocity = new Vector3(playerComponent.body.velocity.x, WalljumpUpwardsForce, playerComponent.body.velocity.z);
                playerComponent.body.velocity = playerComponent.body.velocity + (transform.right * WalljumpForce);
                playerComponent.CurrentJumps = 1;
            }
                
            if (hitleft.collider != null && Input.GetKeyDown(KeyCode.Space))
            {
                playerComponent.CancelDrag(transform.position);
                //body.AddForce(transform.forward * jumpForce * 0.32f * body.mass * Time.fixedDeltaTime, ForceMode.Impulse);
                playerComponent.body.velocity = new Vector3(playerComponent.body.velocity.x,WalljumpUpwardsForce, playerComponent.body.velocity.z);
                playerComponent.body.velocity = playerComponent.body.velocity + -(transform.right * WalljumpForce);
                playerComponent.CurrentJumps = 1;
            }
            playerComponent.CurrentJumps = 0;
        }
        else
        {
            /*if (Vector3.Distance(JumpedFrom, transform.position) > 14)
            {
                InAirFromWallrun = false;
            }*/

            AddedJumpYet = false;
        }
        if (playerComponent.MovePure.z == 0)
        {
            IsWallrunning = false;
        }
    }
    float r;
    // Update is called once per frame
    void Update()
    {
        HandleWallrunning();   
        if (IsWallrunning)
        {
            if (hitleft.collider)
            {
                r = Mathf.Lerp(r, RollAmount, 12 * Time.deltaTime);
                WallrunRoll.localEulerAngles = new Vector3(0, 0, r);
            }
            if (hitright.collider)
            {
                r = Mathf.Lerp(r, -RollAmount, 12 * Time.deltaTime);
                WallrunRoll.localEulerAngles = new Vector3(0, 0, r);
            }
        }
        else
        {
            r = Mathf.Lerp(r, 0, 12 * Time.deltaTime);
            WallrunRoll.localEulerAngles = new Vector3(0, 0, r);
        }
    }

    private void LateUpdate()
    {
    }
}
