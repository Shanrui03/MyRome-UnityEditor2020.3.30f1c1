using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FullScreenController : MonoBehaviour
{
    [SerializeField] private Toggle fullscreenToggle;
    private bool startFullScreen;
    // Start is called before the first frame update
    void Awake()
    {
        LoadFullScreen();
    }
    private void LoadFullScreen()
    {
        fullscreenToggle.isOn = GetFullScreenState();
        Screen.fullScreen = fullscreenToggle.isOn;
        startFullScreen = fullscreenToggle.isOn;
    }

    private void SetFullScreen()
    {
        if(!Screen.fullScreen)
        {
            PlayerPrefs.SetInt("isFullScreen", 0);
        }
        else
        {
            PlayerPrefs.SetInt("isFullScreen", 1);
        }
    }

    private bool GetFullScreenState()
    {
        if(PlayerPrefs.GetInt("isFullScreen") == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void ApplyFullScreenSettings()
    {
        Screen.fullScreen = fullscreenToggle.isOn;
        startFullScreen = fullscreenToggle.isOn;
        SetFullScreen();
    }

    public void BackFromFullScreenSettings()
    {
        fullscreenToggle.isOn = startFullScreen;
        Screen.fullScreen = fullscreenToggle.isOn;
        SetFullScreen();

    }
}
