using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;

public class ExplosiveProjectile : MonoBehaviour
{
    public bool AllowMidairDetonation;
    public Explosion exp;

    private void OnDrawGizmos()
    {
        exp.DrawEffectRegion(transform.position);
    }

    public void Explode()
    {
        exp.Explode(transform.position);
        gameObject.SetActive(false);
    }

    void OnCollisionEnter()
    {
        Explode();
    }
}
