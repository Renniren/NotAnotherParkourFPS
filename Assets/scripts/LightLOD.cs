using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;
using ShadowResolution = UnityEngine.Rendering.LightShadowResolution;

public class LightLOD : MonoBehaviorExtended
{
	public Light light;

	static float VERY_HIGH_RESOLUTION_DISTANCE = 3;
	static float HIGH_RESOLUTION_DISTANCE = 10;
	static float MEDIUM_RESOLUTION_DISTANCE = 25;
	static float LOW_RESOLUTION_DISTANCE = 70;
	static float NO_RESOLUTION_DISTANCE = 120;

	LightShadows old;
    
    void Start()
    {
		main = Camera.main;
		old = light.shadows;
    }

	private void OnDrawGizmos()
	{
		/*
		Gizmos.DrawWireSphere(light.transform.position, NO_RESOLUTION_DISTANCE);
		Gizmos.DrawWireSphere(light.transform.position, LOW_RESOLUTION_DISTANCE);
		Gizmos.DrawWireSphere(light.transform.position, MEDIUM_RESOLUTION_DISTANCE);
		Gizmos.DrawWireSphere(light.transform.position, HIGH_RESOLUTION_DISTANCE);
		Gizmos.DrawWireSphere(light.transform.position, VERY_HIGH_RESOLUTION_DISTANCE);
		*/
	}

	Camera main;
	float dist;
	float check = 2.0f, c;
    
    void LateUpdate()
	{
		c += Time.deltaTime;
		if (c >= check)
        {
			dist = distance(light.transform.position, main.transform.position);
			c = 0;
        }

		light.shadows = old;
		light.shadowResolution = ShadowResolution.Low;

		if (dist >= NO_RESOLUTION_DISTANCE) light.shadows =									LightShadows.None;
		if (dist >= LOW_RESOLUTION_DISTANCE) light.shadowResolution =				ShadowResolution.Low;

		if (dist <= LOW_RESOLUTION_DISTANCE) light.shadowResolution =				ShadowResolution.Low;
		if (dist <= MEDIUM_RESOLUTION_DISTANCE) light.shadowResolution =			ShadowResolution.Medium;
		if (dist <= HIGH_RESOLUTION_DISTANCE) light.shadowResolution	=				ShadowResolution.High;
		if (dist <= VERY_HIGH_RESOLUTION_DISTANCE) light.shadowResolution =	ShadowResolution.VeryHigh;
    }
}
