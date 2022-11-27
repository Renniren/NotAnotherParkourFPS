using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMouselookCamera : MonoBehaviour
{
    public float yaw, pitch;
    public float Sensitivity = 1;
    public bool Locked;

    private void Start()
    {
        yaw += transform.rotation.y;
        pitch += transform.rotation.x;
    }

    public void SetRotation(Quaternion rotation)
    {
        yaw = rotation.y;
        pitch = rotation.x;
        transform.rotation = rotation;
    }

    void Update()
    {
        if (!Locked)
        {
            yaw += Sensitivity * Input.GetAxis("Mouse X");
            pitch -= Sensitivity * Input.GetAxis("Mouse Y");
        }

        pitch = Mathf.Clamp(pitch, -90, 90);
        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
    }
}
