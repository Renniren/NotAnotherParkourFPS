using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;

public class Destructable : MonoBehaviorExtended
{
    public Entity entity;
    public GameObject Gibs;

    void Update()
    {
        if (entity.Dead)
        {
            TransferRigidbodyVelocityToChildren(gameObject.GetRigidbody().velocity, Instantiate(Gibs, transform.position, transform.rotation).transform);
            gameObject.SetActive(false);
        }
    }
}
