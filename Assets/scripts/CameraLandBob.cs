using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLandBob : MonoBehaviour
{
    public float intensity = 1;
    public float MaximumDisplacement = 2;
    public float RecoverySpeed = 0.09f;
    public float fallRollIntensity = 1.2f;
    //public ViewmodelFollow follow;
    public PlayerMovement player;
    public CameraWobble wobble;
    private Vector3 velocity;
    private Vector3 DefaultPosition;
    private bool DidBob;

    void Start()
    {
        DefaultPosition = transform.localPosition;
    }

    private void FixedUpdate()
    {
        if (!player.IsGrounded)
        {
            velocity = player.body.velocity;
            DidBob = false;
        }
    }

    void Update()
    {
        if(!player.IsGrounded)
        {
            velocity = player.body.velocity;
            DidBob = false;
        }
        else
        {
            if(!DidBob)
            {
                velocity.y = Mathf.Abs(velocity.y);
                velocity.y = Mathf.Clamp(velocity.y, 0, MaximumDisplacement);
                transform.localPosition = -Vector3.up * (velocity.y * intensity);

                if (wobble)
                {
                    wobble.AddFallImpulse(new Vector3(fallRollIntensity, 0, 0));
                }

                velocity.y = 0;
                DidBob = true;
            }
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, DefaultPosition, RecoverySpeed * Time.unscaledDeltaTime);
    }
}
