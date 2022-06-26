using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatueTrigger : MonoBehaviour
{
    static StatueTrigger instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        instance = this;
    }

    public static void StatueTriggerOn()
    {
        instance.gameObject.SetActive(!instance.gameObject.activeSelf);
    }

}
