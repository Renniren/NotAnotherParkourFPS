using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class PlayerState : MonoBehaviour
{
    [System.Serializable]
    public struct State
    {
        public string Name;
        public bool Active;

        public State(string name, bool active)
        {
            Name = name;
            Active = active;
        }
    }

    public static List<State> states = new List<State>();
    public List<State> n_states;

    private void Update()
    {
        n_states = states;
    }

    public static bool GetState(string name)
    {
        for (int i = 0; i < states.Count; i++)
        {
            return states.Find(d => d.Name == name).Active;
        }
        return false;
    }

    public static void SetState(string name, bool st)
    {
        if (states.Count == 0)
        {
            states.Add(new State(name, st));
            Debug.Log("Couldn't find state " + name);
            return;
        }

        if (states.Count > 0)
        {
            bool matched = false;
            int end = states.Count - 1;
            for (int i = 0; i < states.Count; i++)
            {
                if (states[i].Name.Equals(name))
                {
                    State fuck_off = states[i];
                    fuck_off.Active = st;
                    states[i] = fuck_off;
                    matched = true;
                }

                if (i == end && !matched)
                {
                    states.Add(new State(name, st));
                    Debug.Log("Couldn't find state " + name);
                    return;
                }
            }
        }
    }
}
