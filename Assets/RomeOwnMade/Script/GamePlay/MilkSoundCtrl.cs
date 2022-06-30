using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilkSoundCtrl : MonoBehaviour
{
    public AudioSource milkAudio;
    public AudioClip gainSound;
    public AudioClip punishSound;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayMilkSound(bool isPunish)
    {
        if(isPunish)
            milkAudio.PlayOneShot(punishSound);
        else
            milkAudio.PlayOneShot(gainSound);

    }
}
