using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;
using UnityEngine.SceneManagement;
using System;
using System.Linq;


/// <summary>
///Keeps record of player statistics for the current level and manages assets. 
///Object pooling is handled entirely by Game Managers.
/// </summary>
public class GameManager : MonoBehaviorExtended
{

    public static int Score = 0;
    public static int Attempts = 0;

    public int KillScore = 11;
    public int DeathPenalty = 100;
    public static float TimeInLevel;
    public delegate void onScoreAdd(int howMuch);
    public static event onScoreAdd OnScoreAdd;
    public delegate void onPoolRefreshRequested();
    public static event onPoolRefreshRequested OnPoolRefreshRequested;

    public static bool PlayerHasBlueKey;
    public static bool PlayerHasYellowKey;
    public static bool PlayerHasRedKey;
    public static bool PlayerHasAnyKey;

    //Allows effects and prefabs to be accessible globally without the need for a singleton instance.
    //Now that I think abou it... it'd be better if this was a singleton. Too late to refactor everything for that now though
    public MeleeAnimation[] meleeAnimations;
    public static MeleeAnimation[] MeleeAnimations;
    public Asset[] assets;
    public static Asset[] static_assets;
    public Sound[] sounds;
    public static Sound[] static_sounds;
    public Weapon[] weapons;
    public static Weapon[] static_weapons;

    public List<Pool> Pools = new List<Pool>(); // Retrieve pool by name?
    public static List<Pool> static_Pools;
    public static void InvokeScoreEvent(int s) { OnScoreAdd?.Invoke(s); }
    public static void RequestPoolRefresh() { OnPoolRefreshRequested?.Invoke(); }

    public static GameObject GetAssetObject(string name)
    {
        foreach (var item in static_assets) { if (item.name == name) return item.obj; }
        return null;
    }

    public static AudioClip GetSound(string name)
    {
        foreach (var item in static_sounds) { if (item.name == name) return item.clip; }
        return null;
    }

    public static Pool GetPool(string query)
    {
        foreach (var item in static_Pools) 
        { 
            if (item.Name.Equals(query))
            {
                //Debug.Log("Successfully found pool with name " + query);
                return item; 
            }
        }
        Debug.LogError("Could not find object of name " + query);
        return new Pool();
    }

    public static void GiveWeapon(string query, int playstyle, bool autoswap = true)
    {
        Weapon match = new Weapon();
        foreach (var item in static_weapons)
        {
            if (item.Name == query)
            {
                match = item;
            }
        }

        WeaponManager mgr = Player.GetComponentInChildren<WeaponManager>();
        PlayerPlaystyleManager playstyleManager = mgr.GetComponent<PlayerPlaystyleManager>();
        if (mgr)
        {
            if (mgr.transform.Find(match.Asset.name)) return;
            playstyleManager.current = playstyle;
            playstyleManager.currentPlaystyle = playstyleManager.Playstyles[playstyleManager.current];

            BaseWeapon wep = Instantiate(match.Asset, playstyleManager.currentPlaystyle.ContainingWeapons[0].transform).GetComponent<BaseWeapon>();
            wep.InternalName = query;
            if (autoswap && wep.AllowAutoSwap)
            {
                wep.re_initialize();
                mgr.state = wep.Position;
            }
        }
    }

    public static void GiveWeapon(GameObject query, int playstyle, bool autoswap = true)
    {
        WeaponManager mgr = Player.GetComponentInChildren<WeaponManager>();
        PlayerPlaystyleManager playstyleManager = mgr.GetComponent<PlayerPlaystyleManager>();
        if (mgr)
        {
            if (mgr.transform.Find(query.name)) return;

            playstyleManager.current = playstyle;

            
            BaseWeapon wep = Instantiate(query, playstyleManager.currentPlaystyle.ContainingWeapons[0].transform).GetComponent<BaseWeapon>();
            if (autoswap && wep.AllowAutoSwap)
            {
                wep.re_initialize();
                mgr.state = wep.Position;
            }
        }
    }



    public static void GiveWeapons(int playstyle, params string[] weapons)
    {
        foreach (var item in weapons)
        {
            GiveWeapon(item, playstyle, true);
        }
    }

    public static void ClearWeapons(bool override_no_clears = false)
    {
        WeaponManager mgr = Player.GetComponentInChildren<WeaponManager>();
        if (mgr)
        {
            foreach (Transform wep in mgr.transform)
            {
                if (wep.gameObject.IsWeapon())
                {
                    if (override_no_clears)
                    {
                        wep.gameObject.TryGetComponent(out BaseWeapon wp);
                        mgr.Weapons.Remove(wp);
                        Destroy(wep.gameObject);
                    }
                    else
                    {
                        wep.gameObject.TryGetComponent(out BaseWeapon wp);
                        if (wp.CanBeCleared)
                        {
                            mgr.Weapons.Remove(wp);
                            Destroy(wep.gameObject);
                        }
                    }
                }
            }
        }
    }

    public static void ClearWeapon(GameObject weapon, bool overrideNoClearFlag)
    {
        WeaponManager mgr = Player.GetComponentInChildren<WeaponManager>();
        BaseWeapon wp = null;
        if (overrideNoClearFlag)
        {
            mgr.state--;
            mgr.Weapons.Remove(wp);
            Destroy(weapon.gameObject);
        }
        else
        {
            weapon.gameObject.TryGetComponent(out wp);
            if (wp.CanBeCleared)
            {
                mgr.state--;
                mgr.Weapons.Remove(wp);
                Destroy(weapon.gameObject);
            }
        }
    }

    
    public static void ClearWeapon(string name, bool overrideNoClearFlag, int playstyle)
    {
        WeaponManager mgr = Player.GetComponentInChildren<WeaponManager>();
        BaseWeapon wp = null;
        if (mgr)
        {
            foreach (Transform wep in mgr.transform)
            {
                if (wep.gameObject.IsWeapon())
                {
                    wep.gameObject.TryGetComponent(out wp);
                    if (overrideNoClearFlag)
                    {
                        if (wp.InternalName == name)
                        {
                            mgr.state--;
                            mgr.Weapons.Remove(wp);
                            Destroy(wep.gameObject);
                        }
                    }
                    else
                    {
                        wep.gameObject.TryGetComponent(out wp);
                        if (wp.CanBeCleared)
                        {
                            if (wp.InternalName == name)
                            {
                                mgr.state--;
                                mgr.Weapons.Remove(wp);
                                Destroy(wep.gameObject);
                            }
                        }
                    }
                }
            }
        }
    }



    static bool LevelLoaded;
    void Start()
    {
        TimeInLevel = 0;
#if UNITY_EDITOR
        Game.init();
#else
        Debug.unityLogger.logEnabled = false;
#endif
        Game.PauseGame(false);

        OnPoolRefreshRequested += GameManager_OnPoolRefreshRequested;
        Game.OnGamePaused += Game_OnGamePaused;
        Game.OnGameUnpaused += Game_OnGameUnpaused;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Score = 0;
        static_assets = assets;
        static_weapons = weapons;
        static_sounds = sounds;
        static_Pools = Pools;
        SceneManager.LoadScene("hud", LoadSceneMode.Additive);
        MeleeAnimations = meleeAnimations;
        PlayerHasBlueKey = false;
        PlayerHasRedKey = false;
        PlayerHasYellowKey = false;
        RefreshGlobalPool();
        Game.InitializeVariables();
    }

    private void Game_OnGameUnpaused()
    {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Game_OnGamePaused()
    {
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) )
        {
            Game.PauseGame(!Game.Paused);
        }
        TimeInLevel += Time.deltaTime;
        PlayerHasAnyKey = PlayerHasBlueKey || PlayerHasYellowKey || PlayerHasRedKey;
    }

    //Empty existing object pools.
    //Create a new Pool for each Asset, according to how much the Asset should be pooled.
    //Iterate over every Pool. Fill its pool with the appropriate object according to the appropriate amount.
    //Collect garbage, since heavy use of Instantiate is made here.
    //Optionally, make the refresh of object pools a blocking function.

    void RefreshGlobalPool()
    {
        Pools.Clear();
        for (int i = 0; i < assets.Length; i++)
        {
            Pools.Add(new Pool(assets[i].AmountToPool, assets[i].name, assets[i]));
        }

        GameObject temp = null;
        for (int i = 0; i < Pools.Count; i++)
        {
            if (Pools[i].Generated)
            {
                Pools[i].FillHoles(Pools[i].HasHoles());
            }
            else
            {
                for (int b = 0; b < Pools[i].capacity; b++)
                {
                    DontDestroyOnLoad(Pools[i].poolObject);
                    temp = Instantiate(Pools[i].poolObject, Vector3.one * -999, Quaternion.identity);
                    temp.SetActive(false);
                    Pools[i].objects.Add(temp);
                    if (b == Pools[i].capacity - 1) Pools[i].SetGenerated();
                }
            }
        }

        GC.Collect();
        static_Pools = Pools;
    }

    private void GameManager_OnPoolRefreshRequested()
    {
        RefreshGlobalPool();
    }
}
