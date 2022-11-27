using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using ParkourFPS;
public struct BloodRequest
{
    public Vector3 where, normal;
    public GameObject on;
    public bool BindToParent;
    public int WhichOne;

    public BloodRequest(Vector3 wh, Vector3 norm, GameObject on, bool bind, int which)
    {
        this.where = wh;
        this.normal = norm;
        this.on = on;
        this.BindToParent = bind;
        this.WhichOne = which;
    }
}

//This class, paired with instances of the Bloodsplatter class is responsible for creating bloodsplatters.
public class BloodManager : MonoBehaviorExtended
{
    public static BloodManager Instance { get; private set; }
    public List<BloodRequest> requests = new List<BloodRequest>();
    public static float BloodCastCooldown = 0.01f;

    private void Awake()
    {
        if(!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    static BloodRequest template = new BloodRequest();

    public static void MakeBloodRequest(Vector3 where, Vector3 surf, GameObject onWhat, bool Parent, int WhichOne)
    {
        if (Instance.current < BloodCastCooldown) return;
        template.where = where;
        template.normal = surf;
        template.on = onWhat;
        template.BindToParent = Parent;
        template.WhichOne = WhichOne;
        Instance?.requests.Add(template);
    }

    int i;
    GameObject dec;

    public void ManageBloodRequests()
    {
		if (requests.Count >= 1024) requests.Clear();
        for (i = 0; i < requests.Count; i++)
        {
            if (requests.Count <= 0) break;
            switch (requests[i].WhichOne)
            {
                case 1:
                    dec = Spawn("Blood1", requests[i].where);
                    dec.transform.rotation = Quaternion.FromToRotation(Vector3.forward, -requests[i].normal);
                    if (requests[i].BindToParent && requests[i].on) dec.transform.SetParent(requests[i].on.transform, true);
                    dec.transform.localEulerAngles += Vector3.forward * random(0, 360.0f);
                    break;

                case 2:
                    dec = Spawn("Blood2", requests[i].where);
                    dec.transform.rotation = Quaternion.FromToRotation(Vector3.forward, -requests[i].normal);
                    if (requests[i].BindToParent && requests[i].on) dec.transform.SetParent(requests[i].on.transform, true);
                    dec.transform.localEulerAngles += Vector3.forward * random(0, 360.0f);
                    break;

                case 3:
                    dec = Spawn("Blood3", requests[i].where);
                    dec.transform.rotation = Quaternion.FromToRotation(Vector3.forward, -requests[i].normal);
                    if (requests[i].BindToParent && requests[i].on) dec.transform.SetParent(requests[i].on.transform, true);
                    dec.transform.localEulerAngles += Vector3.forward * random(0, 360.0f);
                    break;
            }
            requests.RemoveAt(i);
            //await Task.Yield();
        }
    }

    protected float current;
    void Update()
    {
        current += Time.unscaledDeltaTime;
        ManageBloodRequests();
    }
}
