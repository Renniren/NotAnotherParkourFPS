using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRoll : MonoBehaviour
{
    public float recoverySpeed = 12;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SmoothRoll(float amt)
    {
        transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, Vector3.forward * amt, 12 * Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, Vector3.zero, recoverySpeed);
    }
}
