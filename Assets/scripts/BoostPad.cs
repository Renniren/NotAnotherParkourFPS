using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;

public class BoostPad : MonoBehaviour
{
    public float BoostAmount = 10;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (transform.up * (BoostAmount * GameObject.FindWithTag("Player").GetPlayerMovement3D().GravityMultiplier)));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.IsPlayer())
        {
            collision.gameObject.GetRigidbody().velocity = transform.up * (BoostAmount * collision.gameObject.GetPlayerMovement3D().GravityMultiplier);
        }
        else
        {
            if (collision.gameObject.GetRigidbody())
            {
                collision.gameObject.GetRigidbody().velocity = transform.up * BoostAmount;
            }
        }
    }
}
