using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorVelocity : MonoBehaviour
{
    private Animator anim;
    public string FieldName;
    public Vector3 velocity;
    public bool swap;
    public bool AssignVelocityXZ;
    public bool GetSpeedAutomatically;
    private Speedometer speed;
    public Speedometer speed2;

    void Start()
    {
        anim = GetComponent<Animator>();
        
        if(GetSpeedAutomatically)
        {
            speed = GetComponent<Speedometer>();
        }
        
    }

    Vector3 currentVelocityLocal;

    void Update()
    {
        if(speed == null)
        {
            speed = speed2;
        }
        currentVelocityLocal = transform.InverseTransformDirection(speed.Velocity);

        if (GetSpeedAutomatically)
        {
            
            if (!AssignVelocityXZ)
            {
                velocity = speed.Velocity;
                anim.SetFloat("velocity", Mathf.Clamp(currentVelocityLocal.z + currentVelocityLocal.x, -1, 1));
            }
            else
            {
                velocity = speed.Velocity;
                if(swap)
                {
                    anim.SetFloat("velocityX", currentVelocityLocal.z);
                    anim.SetFloat("velocityZ", currentVelocityLocal.x);
                }
                else
                {

                    anim.SetFloat("velocityX", currentVelocityLocal.x);
                    anim.SetFloat("velocityZ", currentVelocityLocal.z);
                }

            }
        }
        else
        {
            if (!AssignVelocityXZ)
            {
                anim.SetFloat("velocity", currentVelocityLocal.z + currentVelocityLocal.x);
            }
            else
            {
                velocity = speed.Velocity;
                anim.SetFloat("velocityX", currentVelocityLocal.x);
                anim.SetFloat("velocityZ", currentVelocityLocal.z);
            }
        }
    }
}
