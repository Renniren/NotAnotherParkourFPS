using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;
using System.Linq;

[System.Serializable]
public struct PlayerPlaystyle
{
    public string Name;
    public int LastState;
    public List<GameObject> ContainingWeapons;

    public PlayerPlaystyle(int FUCKOFF = -0)
    {
        Name = "";
        ContainingWeapons = new List<GameObject>();
        LastState = -1;
    }

    public void SetLastState(int v)
    {
        this.LastState = v;
    }
}

public class PlayerPlaystyleManager : MonoBehaviorExtended
{
    public int current;

    public PlayerPlaystyle currentPlaystyle;
    public WeaponManager WeaponHandler;
    public bool IsSwitchingPlaystylesAllowed = true; 
    public List<int> LastStates = new List<int>();
    public List<PlayerPlaystyle> Playstyles = new List<PlayerPlaystyle>();

    void Start()
    {
        currentPlaystyle = Playstyles[0];
        for (int i = 0; i < Playstyles.Count; i++)
        {
            LastStates.Add(0);
        }
    }
    
    public bool DoesPlaystyleContainWeapon(GameObject query)
    {
        return currentPlaystyle.ContainingWeapons.Contains(query);
    }

    void RefreshActiveWeapons()
    {
        foreach(Transform weapon in transform)
        {
            weapon.gameObject.SetActive(currentPlaystyle.ContainingWeapons.Contains(weapon.gameObject));
            //WeaponHandler.RefreshWeapons();
        }
    }

    Weapon weapon2;


    void Update()
    {

        foreach(Transform weapon in transform)
        {
            /*weapon.TryGetComponent(out weapon2);
            if (weapon2)
            {
                weapon.gameObject.SetActive(currentPlaystyle.ContainingWeapons.Contains(weapon.gameObject) && WeaponHandler.WeaponKeys.Contains(weapon2.WeaponKeyID));
            }   
            else
            {
                
            }*/

            weapon.gameObject.SetActive(currentPlaystyle.ContainingWeapons.Contains(weapon.gameObject));
            //WeaponHandler.RefreshWeapons();
        }
        
        if(Input.GetKeyDown(KeyCode.Tab) && IsSwitchingPlaystylesAllowed)
        {
            LastStates[current] = WeaponHandler.state;
            current = LoopingClamp(current + 1, 0, Playstyles.Count - 1);
            currentPlaystyle = Playstyles[current]; 
            RefreshActiveWeapons();
            WeaponHandler.state = LastStates[current];
            //WeaponHandler.RefreshWeapons();
            //WeaponHandler.SwitchedWeapons.Invoke();
        }
        
        
        current = LoopingClamp(current, 0, Playstyles.Count - 1);
        currentPlaystyle = Playstyles[current];
    }
}
