using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerArea : MonoBehaviour
{
	public bool AutoSetLayer = true;
	public bool DetectPlayerOnly = true;
	public bool OnlyActivateOnce = true;
	public bool DestroyAfterActivation;
	public UnityEvent TriggerEntered, TriggerExited;

	void Start()
	{
		GetComponent<MeshRenderer>().enabled = false;
		if(AutoSetLayer)gameObject.layer = 19;
	}

    void OnTriggerEnter(Collider col)
    {
		if(OnlyActivateOnce)
		{
			if(DetectPlayerOnly)
			{
				if(col.gameObject.CompareTag("Player"))
				{
					TriggerEntered.Invoke();
					if(DestroyAfterActivation)
                    {
						Destroy(gameObject);
                    }
				}
			}
			else
			{
				TriggerEntered.Invoke();
				if (DestroyAfterActivation)
				{
					Destroy(gameObject);
				}
			}
		}
    }

    private void OnTriggerExit(Collider col)
    {
		if (DetectPlayerOnly)
		{
			if (col.gameObject.CompareTag("Player"))
			{
				TriggerExited.Invoke();
				if (DestroyAfterActivation)
				{
					Destroy(gameObject);
				}
			}
		}
		else
		{
			TriggerExited.Invoke();
			if (DestroyAfterActivation)
			{
				Destroy(gameObject);
			}
		}
	}

    void OnTriggerStay(Collider col)
	{
		if(!OnlyActivateOnce)
		{
			if(DetectPlayerOnly)
			{
				if(col.gameObject.CompareTag("Player"))
				{
					TriggerEntered.Invoke();
					if (DestroyAfterActivation)
					{
						Destroy(gameObject);
					}
				}
			}
			else
			{
				TriggerEntered.Invoke();
				if (DestroyAfterActivation)
				{
					Destroy(gameObject);
				}
			}
		}
	}
}
