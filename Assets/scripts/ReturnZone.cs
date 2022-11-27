using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnZone : MonoBehaviour
{
    Vector3 old;
    // Start is called before the first frame update
    void Start()
    {
        PlayerMovement.OnPlayerJump += GetOldPlayerPosition;
    }

    private void GetOldPlayerPosition(Vector3 where)
    {
        old = where;
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            other.transform.position = old;
            other.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }
}
