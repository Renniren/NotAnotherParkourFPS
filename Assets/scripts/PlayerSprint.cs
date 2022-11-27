using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSprint : MonoBehaviour
{
    public Animator HandsAnimator;
    public PlayerMovement playerComponent;
    public bool IsSprinting;
    public float SprintAcceleration = 190;
    public float oldAcceleration;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && playerComponent.MovePure.z > 0 && !PlayerState.GetState("IsSliding") && PlayerState.GetState("IsGrounded"))
        {
            if (!IsSprinting)
            {
                oldAcceleration = playerComponent.Acceleration;
                IsSprinting = true;
            }
        }
        else
        {
            if (IsSprinting)
            {
                playerComponent.Acceleration = oldAcceleration;
                IsSprinting = false;
            }
        }

        if (IsSprinting && playerComponent.IsGrounded)
        {
            playerComponent.Acceleration = SprintAcceleration;
            HandsAnimator.Play("run");
        }

        PlayerState.SetState("IsSprinting", IsSprinting);
    }
}
