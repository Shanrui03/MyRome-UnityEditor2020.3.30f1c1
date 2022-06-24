using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class MainMenuRome : MonoBehaviour
{
    public GameObject LoadingScreen;
    public GameObject EnterGameScreen;
    public GameObject SettingsMenu;
    public GameObject BGM;

    private void Start()
    {
        SettingsMenu.SetActive(false);
        //PlayerPrefs.SetFloat("Volume", 1);
        //PlayerPrefs.SetInt("Quality", 4);
        //QualitySettings.SetQualityLevel(4);
        //Screen.fullScreen = true;
        //Screen.fullScreenMode = FullScreenMode.FullScreenWindow;

    }

    private void Update()
    {
        BGM.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("Volume");
    }
    public void PlayGame()
    {
        //SceneManager.LoadScene("RomeScene");
        LoadingScreen.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void NextPage()
    {
        LoadingScreen.SetActive(false);
        EnterGameScreen.SetActive(true);
    }

    public void EnterGame()
    {
        SceneManager.LoadScene("RomeScene");
    }

    public void ShowSettingsMenu()
    {
        SettingsMenu.SetActive(!SettingsMenu.activeSelf);
        this.gameObject.SetActive(!this.gameObject.activeSelf);
    }
}
