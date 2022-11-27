using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;

public class Transporter : MonoBehaviour
{
    public Transform PointB;
    public GameObject TeleportEffect;
    public bool CopyRotation;
    public bool OnlyTeleportEntities = true;
    private Collider[] Colliders;
    private BoxCollider boxCollider;

    private void Start()
    {
        TryGetComponent(out boxCollider);
        boxCollider.enabled = false;
    }

    private void Update()
    {
        Colliders = Physics.OverlapBox(transform.position, boxCollider.size, transform.rotation);
        foreach(Collider other in Colliders)
        {
            if (!OnlyTeleportEntities)
            {
                Teleport(other.gameObject);
            }
            else
            {
                Entity info = other.gameObject.GetEntity();
                if(info)
                {
                    Teleport(other.gameObject);
                }
            }
        }
    }

    public void Teleport(GameObject traveller)
    {
        if(traveller.transform.root.gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>())
        {
            traveller.transform.root.gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>().Warp(PointB.position);
        }
        else
        {
            traveller.transform.root.position = PointB.position;
            if (CopyRotation)
            {
                if(traveller.IsPlayer())
                {
                    traveller.GetPlayerMovement3D().SetRotation(PointB.eulerAngles.x, PointB.eulerAngles.y);
                }
                traveller.transform.eulerAngles = PointB.eulerAngles;
            }
        }
        
        if(TeleportEffect)
        {
            Instantiate(TeleportEffect, traveller.transform.position, traveller.transform.rotation);
        }
    }
}