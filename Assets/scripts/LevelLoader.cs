using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public void Load(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}
