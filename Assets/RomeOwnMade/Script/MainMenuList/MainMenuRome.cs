using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class MainMenuRome : MonoBehaviour
{
    public GameObject SettingsMenu;
    public GameObject BGM;

    private void Start()
    {
        SettingsMenu.SetActive(false);

    }

    private void Update()
    {
        BGM.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("Volume");
    }
    public void PlayGame()
    {
        SceneManager.LoadScene("RomeScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }


    public void ShowSettingsMenu()
    {
        SettingsMenu.SetActive(!SettingsMenu.activeSelf);
        this.gameObject.SetActive(!this.gameObject.activeSelf);
    }
}
