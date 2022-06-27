using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatueTrigger : MonoBehaviour
{
    public GameObject Ladder;
    public void StatueTriggerOn()
    {
        this.gameObject.SetActive(false);
        Ladder.SetActive(true);
    }

}
