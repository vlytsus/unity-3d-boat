using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    public void play() {
        Debug.Log("On MainMenu Play");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void quit() {
        Debug.Log("On MainMenu Quit");
        Application.Quit();
    }
    
}
