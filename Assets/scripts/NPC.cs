using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC : Entity
{
    public NPC npc
    {
        get
        {
            return this;
        }
    }

    
    public Rigidbody body;
    public NavMeshAgent ThisAgent;

    public bool AllowInvestigatingSounds = true;

    public float FieldOfView = 90;
    public float HearingRange = 200;
    public float MinimumDistance = 30;
    public float SightDistance = 400;
    public float PatrolInterval = 1;
    public static List<NPC> ActiveNPCs = new List<NPC>();
    public List<Vector3> Alerts = new List<Vector3>();

    protected float CurrentPatrolInterval;

    float limit = 200;
    protected Vector3 RandomPos
    {
        get
        {
            return new Vector3(Random.Range(-limit, limit), Random.Range(-limit, limit), Random.Range(-limit, limit));
        }
    }

    protected bool HasNoAlerts
    {
        get
        {
            return Alerts.Count == 0;
        }
    }

    public static void TryToMakeAlert(Vector3 where, float loudness = 0)
    {
        for (int i = 0; i < ActiveNPCs.Count; i++)
        {
            if (Vector3.Distance(where, ActiveNPCs[i].transform.position) < ActiveNPCs[i].HearingRange + loudness)
            {
                ActiveNPCs[i].Alerts.Add(where);
            }
        }
    }

    // Start is called before the first frame update
    protected void Start()
    {
        CurrentPatrolInterval = CurrentPatrolInterval * 0.99f;
    }

    protected void OnDisable()
    {
        if (ActiveNPCs.Contains(this)) ActiveNPCs.Remove(this);
    }

    protected Vector3 roamvec;
    protected void Roam()
    {
        if (HasNoAlerts)
        {
            CurrentPatrolInterval += Time.deltaTime;
            if (CurrentPatrolInterval >= PatrolInterval || Vector3.Distance(transform.position, ThisAgent.destination) < 4)
            {
                roamvec = transform.position + RandomPos;
                ThisAgent.SetDestination(roamvec);
                CurrentPatrolInterval = 0;
            }
        }

        //ThisAgent.destination += RandomPos * 0.4f;

        if(!HasNoAlerts && AllowInvestigatingSounds)
        {
            ThisAgent.SetDestination(Alerts[0]);
            if (Vector3.Distance(Alerts[0], transform.position) < 0.8f)
            {
                Alerts.RemoveAt(Mathf.Clamp(Alerts.Count - 1, 0, Alerts.Count));
            }
        }
    }

    // Update is called once per frame
    protected void Update()
    {
        body.velocity = Vector3.Lerp(body.velocity, Vector3.zero, 0.2f);
        body.angularVelocity = Vector3.Lerp(body.angularVelocity, Vector3.zero, 0.2f);


        if (!ActiveNPCs.Contains(this)) ActiveNPCs.Add(this);
        base.Update();
    }
}
