using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;
public class ViewmodelFollow : MonoBehaviour
{
    public float VelocityDivisor = 0.003f;
    public float SmoothingAmount = 0.55f;
    public float AdditiveSmoothingAmount = 0.55f;
    public float DefaultLag = 0.024f;
    public float MaxLag = 0.024f;
    public bool AllowRotationalLag;
    public float RollSpeed = 2;
    public float RecoverySpeed = 2;
    public float MaxRoll = 4;

    public static ViewmodelFollow instance;
    public PlayerMovement player;
    void Start()
    {
        instance = this;
    }
    Vector3 interp = new Vector3();
    public Vector3 additive, additive2, additive3;
    float x, y;
    void FixedUpdate()
    {
        
    }

    public void AddImpulse(Vector3 imp)
    {
        additive += imp;
    }

    public void SetImpulse(Vector3 imp)
    {
        additive = imp;
    }

    public void StopLag()
    {
        transform.localPosition = Vector3.zero;
        interp = Vector3.zero;
        additive = Vector3.zero;
    }

    private void OnDisable()
    {
        if (AllowRotationalLag)
        {
            transform.localEulerAngles = Vector3.zero;
        }
    }

    void Update()
    {
        if (player.move.magnitude > 0.005f)
        {
            interp.y = Mathf.Lerp(interp.y, 0, 0.2f);
        }

        if (!Game.Paused && AllowRotationalLag)
        {
            y = Input.GetAxis("Mouse Y");
            x = -Input.GetAxis("Mouse X");
            x = Mathf.Clamp(x, -4, 4);
            y = Mathf.Clamp(y, -4, 4);

            additive2.y += x;
            additive2.x += y;

            additive2.y = Mathf.Clamp(additive2.y, -MaxRoll, MaxRoll);
            additive2.x = Mathf.Clamp(additive2.x, -MaxRoll, MaxRoll);

            additive2 = Vector3.Lerp(additive2, Vector3.zero, RollSpeed * Time.deltaTime);

            transform.localEulerAngles = Vector3.zero + additive2 + additive3;
        }

        if (!AllowRotationalLag)
        {
            transform.localEulerAngles = Vector3.zero;
        }

        additive3 = Vector3.Lerp(additive3, Vector3.zero, RecoverySpeed * Time.deltaTime);
        additive = Vector3.Lerp(additive, Vector3.zero, 1 - AdditiveSmoothingAmount);
        interp = Vector3.Lerp(interp, -player.body.velocity, 1 - SmoothingAmount);
        transform.localPosition = Vector3.zero + (transform.InverseTransformDirection(interp) + transform.InverseTransformDirection(additive)) * VelocityDivisor;
        transform.localPosition = Vector3.ClampMagnitude(transform.localPosition, MaxLag);
    }
}
