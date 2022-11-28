using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ParkourFPS;

public class HUDManager : MonoBehaviorExtended
{
    public Canvas cnvs;
    public HUDNotification notification;
    public HUDSubtitles sub;
	public BossHealthBar BossHealth;
	public float PlaneDistance;
	//Because the stupid fucking retard who wrote this shit didn't allow you to adjust the Overlay's distance from the Camera. 

	public static BossHealthBar bossHealthBar;
    public static HUDSubtitles Subtitles;
    public static Canvas HUD;
    public static HUDNotification Notification;
    
    // Start is called before the first frame update
    void Start()
    {
        Events.BeforeGameInitialized += Events_BeforeGameInitialized;
        Events.OnPhotomodeEnter += Events_OnPhotomodeEnter;
        Events.OnPhotomodeExit += Events_OnPhotomodeExit;
        cnvs.planeDistance = PlaneDistance;
        cnvs.worldCamera = GetHUDCamera();
        HUD = cnvs;
        Subtitles = sub;
        Notification = notification;
        bossHealthBar = BossHealth;
    }

    private void Events_BeforeGameInitialized()
    {
        
    }

    public static void ShowBossHealthBar(Entity boss, string name)
	{
		bossHealthBar.show = true;
		bossHealthBar.bossName = name;
		bossHealthBar.boss = boss;
	}

    private void OnDestroy()
    {
        Events.BeforeGameInitialized -= Events_BeforeGameInitialized;
        Events.OnPhotomodeEnter -= Events_OnPhotomodeEnter;
        Events.OnPhotomodeExit -= Events_OnPhotomodeExit;
    }

    private void Events_OnPhotomodeExit()
    {
        cnvs.enabled = true;
    }

    private void Events_OnPhotomodeEnter()
    {
        cnvs.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
