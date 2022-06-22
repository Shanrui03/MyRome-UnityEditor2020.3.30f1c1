using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    private AudioSource menuAudio;
    private Slider audioSlider;
    private float startVolume;
    // Start is called before the first frame update
    void Start()
    {
        menuAudio = GameObject.FindGameObjectWithTag("MainMenu").transform.GetComponent<AudioSource>();
        audioSlider = GameObject.FindGameObjectWithTag("gameSettings").transform.GetComponentInChildren<Slider>();
        LoadVolume();
    }

    // Update is called once per frame
    void Update()
    {
        //VolumeControll();
    }

    public void VolumeControll()
    {
        menuAudio.volume = audioSlider.value;
    }

    public void LoadVolume()
    {
        audioSlider.value = PlayerPrefs.GetFloat("Volume");
        menuAudio.volume = audioSlider.value;
        startVolume = audioSlider.value;
    }

    public void ApplyVolumeSettings()
    {
        PlayerPrefs.SetFloat("Volume", audioSlider.value);
        startVolume = audioSlider.value;
    }

    public void BackFromVolumeSettings()
    {
        audioSlider.value = startVolume;
    }
}
