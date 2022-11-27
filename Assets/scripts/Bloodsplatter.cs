using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bloodsplatter : MonoBehaviour
{
    private ParticleSystem ps;
    public List<ParticleCollisionEvent> CollisionEvents = new List<ParticleCollisionEvent>(20);
    private int numCollisionEvents;
    
    void Start()
    {
        TryGetComponent(out ps);
    }
    private int FuckYou;
    bool did;
    static int i = 0;
    void OnParticleCollision(GameObject other)
    {
        if (!did)
        {
            numCollisionEvents = ps.GetCollisionEvents(other, CollisionEvents);

            if(other.gameObject.CompareTag("Player")) return;
        
            for (i = 0; i < numCollisionEvents; i++)
            {
                FuckYou = Random.Range(1, 9);
                BloodManager.MakeBloodRequest(CollisionEvents[i].intersection, CollisionEvents[i].normal, other, false, FuckYou);
            }
        }
    }
}
