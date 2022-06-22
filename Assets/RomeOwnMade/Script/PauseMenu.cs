using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public static bool GameIsEnd = false;
    public GameObject pauseMenuUI;
    public GameObject settingsMenuUI;

    public GameObject myBag;
    public GameObject endUI;
    bool isOpen = false;

    public GameObject FontSight;
    // Start is called before the first frame update
    void Start()
    {
        FontSight.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1f;
        GameIsPaused = false;
        GameIsEnd = false;
        isOpen = false;
        PlayerPrefs.SetFloat("Volume", 1);
        PlayerPrefs.SetInt("Quality", 4);
        QualitySettings.SetQualityLevel(4);
    }

    // Update is called once per frame
    void Update()
    {
        if (!PlayerMovement.isTalking)
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
            if (!GameIsPaused)
            {
                OpenMyBag();
            }
        }
        else if(PlayerMovement.isTalking && isOpen)
        {
            CloseMyBag();
        }

        if(GameIsEnd && !endUI.activeSelf)
        {
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetActive(false);
            }
            endUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            FontSight.SetActive(false);
            Time.timeScale = 0f;
            GameIsPaused = true;
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

    public void CloseMyBag()
    {
        isOpen = false;
        myBag.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        FontSight.SetActive(true);
    }

    public void CloseByButton()
    {
        isOpen = false;
        myBag.SetActive(false);
        FontSight.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        InventoryManager.RefreshItem();
    }

    public void ShowSettings()
    {
        settingsMenuUI.SetActive(!settingsMenuUI.activeSelf);
        pauseMenuUI.SetActive(!pauseMenuUI.activeSelf);
    }
}
