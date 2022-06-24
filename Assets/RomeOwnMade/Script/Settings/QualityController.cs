using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class QualityController : MonoBehaviour
{
    [SerializeField] private Dropdown quality;
    private int startQuality;
    // Start is called before the first frame update
    private void Awake()
    {
        LoadQuality();
        BuildQualityMenu();
    }


    public void LoadQuality()
    {
        QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("Quality"));
        startQuality = PlayerPrefs.GetInt("Quality");
    }

    public void BuildQualityMenu()
    {
        quality.ClearOptions();
        quality.AddOptions(QualitySettings.names.ToList());
        quality.value = PlayerPrefs.GetInt("Quality");
    }

    public void ApplyQualitySettings()
    {
        QualitySettings.SetQualityLevel(quality.value);
        PlayerPrefs.SetInt("Quality", quality.value);
        startQuality = quality.value;
    }

    public void BackFromQualitySettings()
    {
        QualitySettings.SetQualityLevel(startQuality);
        quality.value = startQuality;
    }
}
