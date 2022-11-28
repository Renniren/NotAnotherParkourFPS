using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;

public class BossEnemy : MonoBehaviour
{
	public string Name = "GENERIC BOSS";
	public bool InPlay;

	Entity ent;
    // Start is called before the first frame update
    void Start()
    {
		ent = gameObject.GetEntity();
    }
	bool b;
	public void Activate() { InPlay = true; }

	bool showedHealthBar;
    // Update is called once per frame
    void Update()
    {
        if (ent)
		{
			if (!ent.Dead)
            {
				if (InPlay)
				{
					HUDManager.ShowBossHealthBar(ent, Name);
					if (!b)
					{
					
						b = true;
					}
				}
            }
		}
    }
}
