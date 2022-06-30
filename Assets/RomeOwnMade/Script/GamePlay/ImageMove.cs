using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageMove : MonoBehaviour
{
    public bool isPunished;
    private float speed;
    private GameObject milkUI;
    // Start is called before the first frame update
    void Start()
    {
        milkUI = this.transform.parent.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        speed = Screen.height / 3;
        transform.Translate(0, -speed * Time.deltaTime, 0, Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            milkUI.SendMessage("PlayMilkSound", isPunished);
            if (!isPunished)
                GameLogicMaster.milkScroe++;
            else
                GameLogicMaster.milkScroe--;
           
            Destroy(this.gameObject);
        }
    }
}
