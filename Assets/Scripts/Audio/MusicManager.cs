using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 


public class MusicManager : MonoBehaviour
{
    private void Awake()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        bool isMenuScene = currentSceneName == "StartScreen";


        GameObject[] musicObj = GameObject.FindGameObjectsWithTag("MusicPlayer"); 

        if (isMenuScene)
        {
            if (musicObj.Length > 1)
            {
                Destroy(this.gameObject);
            }
            DontDestroyOnLoad(this.gameObject);

        }
        else
        {
            Destroy(musicObj[0]); 
        }

    }

}
