using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHeadbob : MonoBehaviour
{
    public PlayerMovement PlayerComponent;
    public Rigidbody body;
    public float WaveLength = 1;
    public float WaveAmplitude = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerComponent.IsGrounded)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, 
                new Vector3(0, 0.7f, 0) + 
                Vector3.up * ((Mathf.Sin(Time.time * WaveAmplitude) / WaveLength) * 
                Mathf.Clamp(body.transform.InverseTransformDirection(body.velocity).magnitude, -1, 1)), 12 * Time.deltaTime);
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(0, 0.7f, 0), 12 * Time.deltaTime);
        }
    }
}
