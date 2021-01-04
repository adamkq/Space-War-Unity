using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level : MonoBehaviour
{
    // manages scene transitions

    public void Update()
    {
        // user can press escape to go back to main menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Scene _scene = SceneManager.GetActiveScene();
            if (_scene.name != "LevelSelect")
            {
                LoadStartMenu();
            }
        }
    }

    public void LoadStartMenu()
    {
        SceneManager.LoadScene("LevelSelect");
    }

    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
