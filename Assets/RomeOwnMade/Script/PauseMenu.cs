using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;

    public GameObject myBag;
    bool isOpen = false;

    public GameObject FontSight;
    // Start is called before the first frame update
    void Start()
    {
        FontSight.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1f;
        GameIsPaused = false;
        isOpen = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isOpen = false;
            myBag.SetActive(false);
            FontSight.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
        if(!GameIsPaused)
        {
            OpenMyBag();
        }
    }


    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        FontSight.SetActive(true);
        Time.timeScale = 1f;
        GameIsPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }


    public void Again()
    {
        SceneManager.LoadScene("RomeScene");
        Resume();
    }


    public void LoadMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenMyBag()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isOpen = !isOpen;
            myBag.SetActive(isOpen);
            if(isOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                FontSight.SetActive(false);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                FontSight.SetActive(true);
                InventoryManager.RefreshItem();
            }
        }
    }

    public void CloseByButton()
    {
        isOpen = false;
        myBag.SetActive(false);
        FontSight.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        InventoryManager.RefreshItem();
    }

}
