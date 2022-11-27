using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    public float LerpSpeed = 4;
    public float RecoverySpeed = 12;
    public Vector3 RecoilDirection;
    float defaultRecovery;
    // Start is called before the first frame update
    void Start()
    {
        defaultRecovery = RecoverySpeed;
    }

    public void AddRecoil(Vector3 dir)
    {
        RecoverySpeed -= 1f + dir.magnitude / 5;
        RecoilDirection += dir;
    }

    // Update is called once per frame
    void Update()
    {
        RecoverySpeed = Mathf.Lerp(RecoverySpeed, defaultRecovery, 3 * Time.deltaTime);
        RecoilDirection = Vector3.Slerp(RecoilDirection, Vector3.zero, RecoverySpeed * Time.deltaTime);
        transform.localEulerAngles = RecoilDirection;
    }
}
