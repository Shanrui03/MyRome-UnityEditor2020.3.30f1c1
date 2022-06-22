using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    private AudioSource menuAudio;
    private Slider audioSlider;
    // Start is called before the first frame update
    void Start()
    {
        menuAudio = GameObject.FindGameObjectWithTag("MainMenu").transform.GetComponent<AudioSource>();
        audioSlider = GameObject.FindGameObjectWithTag("gameSettings").transform.GetComponentInChildren<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        VolumeControll();
    }

    public void VolumeControll()
    {
        menuAudio.volume = audioSlider.value;
    }
}
