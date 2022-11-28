using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;
using System.Linq;
using UnityEngine.Events;
using Text = TMPro.TMP_Text;

public class Objective : MonoBehaviorExtended
{
    public string Name;
    public Transform Where;
    public float ActivationRadius = 10;

    public bool Active = true;
    public bool CompleteAutomatically = true;

    public UnityEvent OnComplete;
    
    bool a;
    bool compl;

    static public List<Objective> Objectives = new List<Objective>();
    static public List<Objective> ActiveObjectives = new List<Objective>();

    GameObject marker;

    void Start()
    {
        if (!Where) Where = transform;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, ActivationRadius);
    }

    public void Complete()
    {
        if (!compl)
        {
            Active = false;
            a = false;
            Objectives.Remove(this);
            Events.InvokeObjectiveDeactivated(this);
            if (marker) marker.SetActive(false);
            Events.InvokeObjectiveCompleted(this);
            OnComplete?.Invoke();
            compl = true;
        }
    }

    public void Activate()
    {
        if (!Active)
        {
            Active = true;
            a = true;
            Events.InvokeObjectiveActivated(this);
            Objectives.AddOnce(this);
            if (!marker)
            {
                marker = Spawn("ObjectiveMarker", Where.position);
                DistanceIndicator = marker.GetComponentInChildren<Text>();
            }
            marker.SetActive(true);
        }
    }

    public void Deactivate()
    {
        if (Active)
        {
            Active = false;
            a = false;
            Objectives.Remove(this);
            Events.InvokeObjectiveDeactivated(this);
            if (marker) marker.SetActive(false);
        }
    }




    Text DistanceIndicator;
    void Update()
    {
        ActiveObjectives = Objectives.FindAll(obj => obj.Active);

        if (a != Active)
        {
            a = Active;
            if (a)
            {
                Events.InvokeObjectiveActivated(this);
                Objectives.AddOnce(this);
                if (!marker)
                {
                    marker = Spawn("ObjectiveMarker", Where.position);
                    DistanceIndicator = marker.GetComponentInChildren<Text>();
                }
                marker.SetActive(true);
            }
            else
            {
                Objectives.Remove(this);
                Events.InvokeObjectiveDeactivated(this);
                if (marker) marker.SetActive(false);
            }
        }

        if (Active)
        {
            if (marker)
            {
                marker.transform.position = Where.position;
                marker.transform.LookAt(PlayerPosition);
                marker.transform.localScale = Vector3.one * (distance(Where.position, PlayerPosition) / 55);

                DistanceIndicator.text = ((int)distance(Where.position, PlayerPosition)).ToString() + "m";
            }

            if (CompleteAutomatically)
            {
                if (distance(Where.position, PlayerPosition) <= ActivationRadius)
                {
                    Complete();
                }
            }
        }
    }
}
