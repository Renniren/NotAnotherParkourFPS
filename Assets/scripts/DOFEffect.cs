using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class DOFEffect : MonoBehaviour
{
    public float AdjustTime = 15;
    public PostProcessVolume volume;
    public DepthOfField dof;
    // Start is called before the first frame update
    void Start()
    {
        volume.profile.TryGetSettings(out dof);
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward * 1000);
        Physics.Raycast(transform.position, transform.forward, out hit, 1000);
        if (hit.collider)  dof.focusDistance.value = Mathf.Lerp(dof.focusDistance.value, Vector3.Distance(transform.position, hit.point), AdjustTime * Time.deltaTime);
        if (!hit.collider) dof.focusDistance.value = Mathf.Lerp(dof.focusDistance.value, Vector3.Distance(transform.position, ray.GetPoint(600)), AdjustTime * Time.deltaTime);
    }
}
