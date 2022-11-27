using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speedometer : MonoBehaviour
{
    public Vector3 OldPosition, Velocity;
    //public Vector3 OldRosition, Velocity;
    public Vector3 OldRotation, RotationalVelocity;
    public float SpeedMagnitude;
    public float SpeedMagnitudeRounded;

    void Start()
    {
        OldPosition = transform.position;
    }

    void Update()
    {
        Velocity = (transform.position - OldPosition) / Time.deltaTime;
        RotationalVelocity = (transform.eulerAngles - OldRotation) / Time.deltaTime;
        SpeedMagnitude = Velocity.magnitude;
        SpeedMagnitudeRounded = (int) SpeedMagnitude;
        OldPosition = transform.position;
        OldRotation = transform.eulerAngles;
    }
}
