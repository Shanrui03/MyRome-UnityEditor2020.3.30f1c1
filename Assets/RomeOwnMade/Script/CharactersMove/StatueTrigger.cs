using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatueTrigger : MonoBehaviour
{
    public GameObject Ladder;
    private AudioSource statueAudio;

    private void Awake()
    {
        statueAudio = GetComponent<AudioSource>();
    }
    public void StatueTriggerOn()
    {
        this.gameObject.SetActive(false);
        Ladder.SetActive(true);
    }
    public void Playmusic()
    {
        statueAudio.PlayOneShot(statueAudio.clip);
    }

}
