using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAnimationCtrl : MonoBehaviour
{
    private Animator DoorAnimator;
    // Start is called before the first frame update
    void Start()
    {
        DoorAnimator = this.GetComponent<Animator>();
    }
    public void OpenDoors()
    {
        DoorAnimator.SetBool("Open", true);
    }
}
