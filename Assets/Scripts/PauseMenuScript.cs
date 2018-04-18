using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuScript : MonoBehaviour {

    //DON'T FORGET TO JUICIFY THIS TOO! WITH ANIMATION TRANSITIONS BETWEEN MENUS AND SOUND EFFECTS (MAYBE USE FADE IN FADE OUT FOR BACKGROUND OF MENUS)
    //REMEMBER TO SET UNSCALED TIME FOR UI ANIMATORS

    public static bool gamePaused = false;          //use this for other functions such as audio        //can also be set true from buttons
    [SerializeField]
    bool controlsPressed = false;
    [SerializeField]
    bool quitPressed = false;
    public GameObject pauseMenuUI;
    public GameObject controlsMenuUI;

    void Start()
    {
        StartCoroutine(Pause());
    }

    //void Resume()
    //{
    //    Time.timeScale = 1f;                //resumes the game
    //    pauseMenuUI.SetActive(false);
    //    gamePaused = false;
    //}

    //void Pause()
    //{
    //    pauseMenuUI.SetActive(true);
    //    Time.timeScale = 0f;                //freeze the game   
    //    gamePaused = true;
    //}

    public IEnumerator Pause()        //due to update loop inputs not triggering timescale when timescale is 0, use a coroutine that runs continuously in the background (coroutines being separate to main time)
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (Time.timeScale == 0)    //if the game is already paused
                {
                    pauseMenuUI.SetActive(false);
                    controlsMenuUI.SetActive(false);
                    Time.timeScale = 1;
                    gamePaused = false;
                }
                else
                {
                    gamePaused = true;
                    pauseMenuUI.SetActive(true);
                    Time.timeScale = 0;
                }
            }

            if (controlsPressed)
            {
                controlsPressed = false;
                print("loading control screen");
                controlsMenuUI.SetActive(true);
            }

            if (quitPressed)
            {
                quitPressed = false;
                print("quitting game");
                Application.Quit();
            }

            yield return null;
        }
    }

    public void Controls()
    {
        controlsPressed = true;           //method should only happen once
    }

    public void Quit()
    {
        quitPressed = true;
        
    }

    
}
