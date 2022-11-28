using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;

public class HUDHurtEffect : MonoBehaviour
{
    public ElementFlasher group;

    private void Start()
    {
        group.group.alpha = 0;
        Events.OnEntityDamaged += Events_OnEntityDamaged;
    }

    private void Events_OnEntityDamaged(Entity ent, int amt, int final, GameObject from)
    {
        if (ent.gameObject.IsPlayer())
        {
            group.Flash();
        }
    }

    private void OnDestroy()
    {
        Events.OnEntityDamaged -= Events_OnEntityDamaged;
    }
}
