using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class MainMenuRome : MonoBehaviour
{
    public GameObject LoadingScreen;
    public GameObject EnterGameScreen;
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
}
