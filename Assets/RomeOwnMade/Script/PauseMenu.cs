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
    public static bool isBagOpen = false;
    bool isSettingsShown = false;

    [Header("Start UI")]
    public GameObject LoadingUI;
    public GameObject StartUI;
    public GameObject inGameUI;
    public GameObject theCompass;

    public GameObject FontSight;
    // Start is called before the first frame update
    void Awake()
    {
        settingsMenuUI.SetActive(false);
        FontSight.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 1f;
        GameIsPaused = true;
        GameIsEnd = false;
        isBagOpen = false;
        isSettingsShown = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!PlayerMovement.isTalking && !isSettingsShown && PlayerMovement.playerCanMove)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                isBagOpen = false;
                myBag.SetActive(false);
                FontSight.SetActive(false);
                
                if (GameIsPaused)
                {
                    Resume();
                    Cursor.lockState = CursorLockMode.Locked;
                }
                else
                {
                    Pause();
                    Cursor.lockState = CursorLockMode.None;
                }
            }
            if (!GameIsPaused)
            {
                OpenMyBag();
            }
        }
        else if(PlayerMovement.isTalking && isBagOpen)
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
            inGameUI.SetActive(false);
            theCompass.SetActive(false);
            Time.timeScale = 0f;
            GameIsPaused = true;
        }
    }


    #region PauseMenuOptions
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
    public void ShowSettings()
    {
        settingsMenuUI.SetActive(!settingsMenuUI.activeSelf);
        pauseMenuUI.SetActive(!pauseMenuUI.activeSelf);
        isSettingsShown = !isSettingsShown;
    }
    public void LoadMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    #endregion

    #region BagOptions
    public void OpenMyBag()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isBagOpen = !isBagOpen;
            myBag.SetActive(isBagOpen);
            if(isBagOpen)
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
        isBagOpen = false;
        myBag.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        FontSight.SetActive(true);
    }
    public void CloseByButton()
    {
        isBagOpen = false;
        myBag.SetActive(false);
        FontSight.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        InventoryManager.RefreshItem();
    }
    #endregion

    #region startOption
    public void ContinueBtnFunction()
    {
        LoadingUI.SetActive(false);
        StartUI.SetActive(true);
    }
    public void StartBtnFunction()
    {
        StartUI.SetActive(false);
        inGameUI.SetActive(true);
        theCompass.SetActive(true);
        GameIsPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    #endregion
}
