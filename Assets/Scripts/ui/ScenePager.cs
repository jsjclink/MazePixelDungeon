using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePager : MonoBehaviour
{
    public void ScenePagerToGame()
    {
        SendingInfo.is_loaded = false;
        SceneManager.LoadScene(1);
    }

    public void ScenePagerToMain()
    {
        SceneManager.LoadScene(0);
    }

}
