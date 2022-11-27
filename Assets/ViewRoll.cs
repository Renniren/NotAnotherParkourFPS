using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewRoll : MonoBehaviour
{
    public float Intensity;
    public float Limit = 4;
    public Rigidbody body;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localEulerAngles = Vector3.forward *
            Mathf.Clamp(-transform.InverseTransformDirection(body.velocity).x, -Limit, Limit) * Intensity;
    }
}
