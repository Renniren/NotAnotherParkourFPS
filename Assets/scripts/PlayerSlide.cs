using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSlide : MonoBehaviour
{
    public Animator HandsAnimator;
    new public CapsuleCollider collider;
    public PlayerMovement playerComponent;
    public Transform GroundCheck;
    public float SlidingHeight = 0.25f;
    public float SlidingGravityMultiplier = 4;
    public float SlidingDrag = 0.5f;
    public float SlideBoostPercent = 1.4f;
    public bool IsSliding;
    public float SlideCancelVelocityThreshhold = 3;
    Vector3 oldgcpos;
    Vector3 vel;
    // Start is called before the first frame update
    void Start()
    {
        oldgcpos = GroundCheck.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        vel = playerComponent.body.velocity;
        vel.y = 0;
        if (Input.GetKeyDown(KeyCode.LeftControl) && playerComponent.DistanceFromGround < 4)
        {
            if (!IsSliding)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y - ((collider.height / 2) * 0.9f), transform.position.z);
                IsSliding = true;
            }
            if (playerComponent.IsGrounded)
            {
                playerComponent.body.velocity *= SlideBoostPercent;
            }
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            if (IsSliding)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y + (0.7f), transform.position.z);
                IsSliding = false;
            }
        }

        if (IsSliding)
        {
            PlayerState.SetState("IsSliding", true);
            playerComponent.CurrentDrag = SlidingDrag;
            if(playerComponent.IsGrounded)playerComponent.body.AddForce(vel * playerComponent.body.mass);
            playerComponent.LockMovement(PlayerMovement.lockMovementMode.LockAll);
            collider.height = SlidingHeight;
            GroundCheck.position = Vector3.one * SlidingHeight;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                IsSliding = false;
                playerComponent.ForceJump();
            }

            if (playerComponent.body.velocity.magnitude > SlideCancelVelocityThreshhold)
            {
                HandsAnimator.Play("slide");
            }
        }
        else
        {
            PlayerState.SetState("IsSliding", false);
            GroundCheck.localPosition = oldgcpos;
            playerComponent.LockMovement(PlayerMovement.lockMovementMode.LockNone);
            collider.height = 2;
        }
    }

    private void FixedUpdate()
    {
        if (IsSliding)
        {
            playerComponent.body.AddForce(transform.up * -9.81f * SlidingGravityMultiplier * playerComponent.body.mass);
        }
    }

    private void LateUpdate()
    {
        if (IsSliding)
        {
            playerComponent.CurrentDrag = SlidingDrag;
        }
    }
}
