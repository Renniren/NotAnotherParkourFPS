using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ParkourFPS;
public class Bullet : BaseEntity
{
    public float speed = 12;
    public float upwardsSpeed = 5;
    public float GravityInfluence = 1;
    public int Damage = 12;
    public int ArmorPenetration;
    public bool PiercesArmor, IgnoresShields;
    public bool DestroyAutomatically = true;
    public bool OnlyHasInitialVelocity;
    public Rigidbody body;
    public UnityEvent CollisionEvents;

    void Start()
    {
        TryGetComponent(out body);
    }

    private void OnEnable()
    {
        TryGetComponent(out body);
        if (OnlyHasInitialVelocity)
        {
            body.velocity = (transform.forward * speed) + (transform.up * upwardsSpeed);
        }
    }


    void Update()
    {
        if (!OnlyHasInitialVelocity) body.velocity = transform.forward * speed;
    }

    private void FixedUpdate()
    {
        body.AddForce(Physics.gravity * GravityInfluence, ForceMode.Force);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryMakeHitEffects(collision.gameObject.GetEntity(), Damage, collision.GetContact(0).point);
        Entity.Hurt(owner, collision.gameObject.GetEntity(), Damage + ArmorPenetration, PiercesArmor, IgnoresShields);
        if (DestroyAutomatically) gameObject.SetActive(false);
    }
}
