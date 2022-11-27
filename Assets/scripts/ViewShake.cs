using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;

public class ViewShake : MonoBehaviorExtended
{
    public Transform RelativeTo;
    public float ShakeAmount = 0;
    public float RollShakeAmount;
    public float MaxShake = 200, MinShake;
    public bool ForCamera;
    public bool UseLateUpdate;
    public float RecoveryTime = 0.05f;
    public float RollRecoveryTime = 0.08f;
    Vector3 StartingPosition;
    Vector3 defrot;
    private Transform tr;

    void Start()
    {
        if (!RelativeTo) tr = transform;
        if (RelativeTo) tr = RelativeTo;
        if (ForCamera) CameraShake = this;
        defrot = transform.localEulerAngles;
        StartingPosition = transform.localPosition;
    }

    public void DoShake(float shakeAmount)
    {
        ShakeAmount += shakeAmount;
    }   
    public void DoRotationalShake(float shakeAmount)
    {
        RollShakeAmount += shakeAmount;
    }

    void ManageCameraShake()
    {
        ShakeAmount = Mathf.Clamp(ShakeAmount, MinShake, MaxShake);
        RollShakeAmount = Mathf.Clamp(RollShakeAmount, MinShake, MaxShake);
        ShakeAmount = Mathf.Lerp(ShakeAmount, 0, RecoveryTime * Time.deltaTime);
        RollShakeAmount = Mathf.Lerp(RollShakeAmount, 0, RollRecoveryTime * Time.deltaTime);

        transform.localEulerAngles = defrot + tr.InverseTransformDirection(Random.onUnitSphere) * RollShakeAmount * Mathf.Clamp(Time.timeScale, 0.1f, 1);
        transform.localPosition = StartingPosition + Random.onUnitSphere * ShakeAmount * Mathf.Clamp(Time.timeScale, 0.1f, 1);
    
        if (ForCamera)
        {
            //ViewmodelFollow.instance.additive3 = 5 * ShakeAmount * Random.onUnitSphere;
        }
    }
    
    void Update()
    {
        if (UseLateUpdate) return;
        ManageCameraShake();
    }

    void LateUpdate()
    {
        if (!UseLateUpdate) return;
        ManageCameraShake();
    }
}
