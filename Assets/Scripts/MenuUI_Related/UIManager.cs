using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

public class UIManager : MonoBehaviour
{
    public GameObject hudPanel;
    public GameObject pausePanel;
    public GameObject optionsPanel;
    public MouseLook playerCamera;

    public bool isPaused = false;

    public static UIManager instance;

    void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        // Check for Escape key to toggle pause menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // If the game is paused, show the pause menu
            TogglePause(!isPaused);
        }
        
        else if (Input.GetKeyDown(KeyCode.Escape) & isPaused == true )
        {
            TogglePause(false);
        }
    }

    public void TogglePause(bool state)
    {
        if(state == true) 
        { 
            isPaused = true;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ShowPauseMenu();
        }
        
        else if (state == false)
        {
            isPaused = false;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            ShowPlayerHUD();
        }

        else { return; }
    }

    public void ShowPlayerHUD()
    {
        hudPanel.SetActive(true);
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
        playerCamera.isMouseLookEnabled = true;
        Debug.Log("Unpausing game");
    }

    public void ShowPauseMenu()
    {
        hudPanel.SetActive(false);
        pausePanel.SetActive(true);
        optionsPanel.SetActive(false);
        playerCamera.isMouseLookEnabled = false;
        Debug.Log("Pausing game");
    }

    public void ShowOptionsMenu()
    {
        hudPanel.SetActive(false);
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);
        playerCamera.isMouseLookEnabled = false;
        Debug.Log("Showing options menu");
    }

}
