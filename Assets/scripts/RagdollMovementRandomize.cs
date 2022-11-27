using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollMovementRandomize : MonoBehaviour
{
    public bool OnGround;
    Rigidbody rb;
    float interval = 0.12f;
    float curinterval;
    float force;
    float cur;
    float struggletime = 9;
    float limit = 15;
    private void Start()
    {
        TryGetComponent(out rb);
    }

    private void Update()
    {
        cur += Time.deltaTime;
       // enabled = cur >= struggletime || !rb;

        if (!OnGround && rb && cur < struggletime)
        {
            
            
            curinterval = curinterval + Time.deltaTime;

            if(curinterval > interval)
            {
                force = Random.Range(-limit, limit);
                curinterval = 0;
            }

            rb.AddForce(transform.up * force * rb.velocity.normalized.magnitude * rb.mass);
            rb.AddForce(transform.forward * force * rb.velocity.normalized.magnitude * rb.mass);
            rb.AddForce(transform.right * force * rb.velocity.normalized.magnitude * rb.mass);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        OnGround = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        OnGround = false;
    }
}
