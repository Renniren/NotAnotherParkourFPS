using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;

public class RandomCriticalDamage : MonoBehaviour
{
    public int Chance = 4;
    public int Multiplier = 3;

    // Start is called before the first frame update
    void Start()
    {
        Events.OnDamageAttempted += Events_OnDamageAttempted;
    }

    private void Events_OnDamageAttempted(GameObject victim, ref int amt, GameObject from)
    {
        if (Random.Range(1, Chance + 1) == Chance / 2)
        {
            TimeManager.Instance.DramaticHit(0.1f);
            amt *= Multiplier;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
