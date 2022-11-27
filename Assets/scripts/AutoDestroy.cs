using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
	public float time = 10;
	float t;

    void Update()
    {
		t += Time.deltaTime;
		if (t >= time) Destroy(gameObject);
    }
}
