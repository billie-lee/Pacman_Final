using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InterfaceScript : MonoBehaviour {

    private Image playBtn;
    private Image configureBtn;
    private Image exitBtn;

    public void exitGame(){
        Debug.Log("Exit Game");
        Application.Quit();
    }

    public void loadGame(){
        Debug.Log("Load Normal Level");
        SceneManager.LoadScene("Normal_Level");
    }
}
