using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ParkourFPS;

public class BossHealthBar : MonoBehaviorExtended
{
	public bool show;
	public Transform showpos, hide;
	public Entity boss;
	public ViewShake BarShake;
	public Text caption;
	public string bossName;
	public Image LagBar;
	public Image[] bars;

	float d, h = 1;
    
    void Start()
    {
		d = 0;
		died = false;
		show = false;
		Events.OnEntityDamaged += Events_OnEntityDamaged;
    }

	private void Events_OnEntityDamaged(Entity ent, int amt, int final, GameObject from)
	{
		if (ent == boss) BarShake.DoShake(12);
	}

	private void OnDestroy()
	{
		Events.OnEntityDamaged -= Events_OnEntityDamaged;
	}

	bool died;
	float t;
	float lerprate;
	void Update()
    {
		if (show)
        {
			transform.position = Vector3.Lerp(transform.position, showpos.position, 12 * Time.deltaTime);
			caption.text = bossName;
			if (boss)
			{
				t += Time.deltaTime;
				if (t >= 1)
                {
					lerprate = Mathf.Lerp(lerprate, 12, 3 * Time.deltaTime);
					LagBar.fillAmount = Mathf.Lerp(LagBar.fillAmount, PercentOf(boss.Health, boss.MaximumHealth), 5 * Time.deltaTime);
					foreach (Image img in bars)
					{
						img.fillAmount = Mathf.Lerp(img.fillAmount, PercentOf(boss.Health, boss.MaximumHealth), lerprate * Time.deltaTime);
					}
                }

				if (boss.Dead)
				{
					if (!died)
                    {
						BarShake.DoShake(55);
						died = true;
                    }
				}

				if (died)
				{
					d += Time.deltaTime;
					if (d >= h)
					{
						d = 0;
						died = false;
						show = false;
					}
				}
			}
        }
		else
		{
			t = 0;
			LagBar.fillAmount = 0;
			foreach (Image img in bars)
			{
				img.fillAmount = Mathf.Lerp(img.fillAmount, 0, 9 * Time.deltaTime);
			}
			transform.position = Vector3.Lerp(transform.position, hide.position, 12 * Time.deltaTime);
		}
    }
}
