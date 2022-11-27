using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeRotation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
 		transform.eulerAngles += new Vector3(0, Random.Range(0.0f, 360.0f), 0);       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
