using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionController : MonoBehaviour
{
    [SerializeField] private Dropdown resolutions;
    [SerializeField] private Toggle fullscreenToggle;
    private Resolution[] resolutionsList;
    private Resolution startResolution;
    private int startResolutionValue;
    // Start is called before the first frame update
    void Awake()
    {
        LoadResolutions();
        BuildResolutionMenu();
        startResolution = Screen.currentResolution;
    }

    private void LoadResolutions()
    {
        resolutionsList = Screen.resolutions;
        List<Resolution> res = new List<Resolution>();
        int hz = resolutionsList[resolutionsList.Length - 1].refreshRate;
        for (int i = 0; i < resolutionsList.Length; i++)
        {
            if (resolutionsList[i].refreshRate == hz)
            {
                res.Add(resolutionsList[i]);
            }
        }
        resolutionsList = res.ToArray();
    }

    private void BuildResolutionMenu()
    {
        resolutions.options = new List<Dropdown.OptionData>();
        for (int i = 0; i < resolutionsList.Length; i++)
        {
            Dropdown.OptionData option = new Dropdown.OptionData();
            option.text = ResToString(resolutionsList[i]);
            resolutions.options.Add(option);
            if (resolutionsList[i].height == Screen.height && resolutionsList[i].width == Screen.width)
            {
                resolutions.value = i;
                startResolutionValue = resolutions.value;
            }
        }
    }

    string ResToString(Resolution res)
    {
        return res.width + " x " + res.height;
    }

    public void RefreshDropdown()
    {
        for (int i = 0; i < resolutionsList.Length; i++)
        {
            if (resolutionsList[i].height == Screen.height && resolutionsList[i].width == Screen.width)
            {
                resolutions.value = i;
                startResolutionValue = resolutions.value;
            }
        }
        resolutions.RefreshShownValue();
    }

    public void ApplyResolutionSettings()
    {
        Screen.SetResolution(resolutionsList[resolutions.value].width, resolutionsList[resolutions.value].height, fullscreenToggle.isOn);
        startResolution = Screen.currentResolution;
    }

    public void BackFromResolutionSettings()
    {
        RefreshDropdown();
    }

}
