using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDrift : MonoBehaviour
{
    public float RightWaveAmplitude = 1;
    public float RightWaveLength = 1;
    public float UpWaveAmplitude = 3;
    public float UpWaveLength = 2;
    public float RollWaveAmplitude = 3;
    public float RollWaveLength = 2;


    public Speedometer speedometer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    Vector3 to;
    float i, interval = 3;
    // Update is called once per frame
    void Update()
    {

        transform.localEulerAngles =
            (Vector3.up * Mathf.Sin(Time.time * RightWaveAmplitude) / RightWaveLength) +
            (Vector3.right * Mathf.Sin(Time.time * UpWaveAmplitude) / UpWaveLength) +
            (Vector3.forward * Mathf.Sin(Time.time * RollWaveAmplitude) / RollWaveLength);
    }
}
