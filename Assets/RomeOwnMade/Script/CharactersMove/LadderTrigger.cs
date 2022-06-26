using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderTrigger : MonoBehaviour
{
    static LadderTrigger instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        instance = this;

        instance.gameObject.SetActive(false);
    }

    public static void LadderTriggerOn()
    {
        instance.gameObject.SetActive(true);
    }
}
