using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;

public class SkyboxModifier : MonoBehaviour
{
	public enum SkyType { Normal, FlatColor }
	public SkyType skyType = SkyType.FlatColor;
	public Color NewColor;
	public float NewFogEnd = 0.4f;

	Material sky_material, old_skybox;
	Color old_sky, old_fog;
	float old_end;
    void Start()
    {
		old_skybox = RenderSettings.skybox;
		old_end = RenderSettings.fogDensity;
		old_fog = RenderSettings.fogColor;
		sky_material = Instantiate(RenderSettings.skybox);
		old_sky = sky_material.GetColor("_Color");
    }

	bool in_trigger;
	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject.IsPlayer())
		{
			in_trigger = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		in_trigger = false;
	}
	
	// Update is called once per frame
	void Update()
    {
        if (in_trigger)
		{
			switch (skyType)
			{
				case SkyType.FlatColor:
					RenderSettings.skybox = sky_material;
					sky_material.SetColor("_Color", Color.Lerp(sky_material.GetColor("_Color"), NewColor, 8 * Time.deltaTime));
					
					DynamicGI.UpdateEnvironment();
					break;
			}
			RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, NewColor, 12 * Time.deltaTime);
			RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, NewFogEnd, 14 * Time.deltaTime);
		}
		else
		{
			switch (skyType)
			{
				case SkyType.FlatColor:
					sky_material.SetColor("_Color", Color.Lerp(sky_material.GetColor("_Color"), old_sky, 8 * Time.deltaTime));
					
					DynamicGI.UpdateEnvironment();
					break;
			}

			RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, old_fog, 12 * Time.deltaTime);
			RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, old_end, 14 * Time.deltaTime);
		}
    }
}
