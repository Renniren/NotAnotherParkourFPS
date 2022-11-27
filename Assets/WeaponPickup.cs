using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;

public class WeaponPickup : MonoBehaviorExtended
{
    public bool lethal;
    public bool pickupActive; //This flag is here so that players don't pick up weapons immediately after throwing them.
    public string weapon;
    public int playstyle;

    TrailRenderer rend;

    float t;
    void Start()
    {
        rend = GetComponent<TrailRenderer>();
        rend.emitting = false;
    }

    bool hitSomething;

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;
        if (t >= 0.8f)
        {
            gameObject.layer = 0;
            pickupActive = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.IsPlayer() && pickupActive)
        {
            GameManager.GiveWeapon(weapon, playstyle, true);
            Destroy(gameObject);
        }

        if (!collision.gameObject.IsPlayer())
        {
            if (lethal)
            {
                if (!hitSomething)
                {
                    if (collision.gameObject.IsEnemy())
                    {
                        if(rend)rend.emitting = true;
                        if(WouldKill(60, collision.gameObject))TimeManager.Instance.DramaticHit(0.34f);
                        CameraShake.DoShake(0.9f);
                        Entity.Hurt(Player, collision.gameObject, 60, true, true);
                        Spawn("BulletSparks", collision.GetContact(0).point);
                        PlaySound("crit1", transform.position);
                        hitSomething = true;
                    }
                }
            }
        }
    }
}
