using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CatchItem : MonoBehaviour
{
    public GameObject ItemNotice;
    public GameObject NPCNotice;
    public GameObject TriggerNotice;
    public Camera followCamera;
    public float RayDistance = 3.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = followCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, RayDistance) && !PauseMenu.GameIsEnd)
        {
            if (hit.collider.gameObject.tag == "Items")
            {
                ItemNotice.SetActive(true);
                NPCNotice.SetActive(false);
                TriggerNotice.SetActive(false);
                if (Input.GetMouseButtonDown(0))
                {
                    CatchAndSave(hit);
                }
            }
            else if(hit.collider.gameObject.tag == "NPC")
            {
                ItemNotice.SetActive(false);
                NPCNotice.SetActive(true);
                TriggerNotice.SetActive(false);
            }
            else if (hit.collider.gameObject.tag == "Trigger")
            {
                ItemNotice.SetActive(false);
                NPCNotice.SetActive(false);
                TriggerNotice.SetActive(true);
            }
            else
            {
                ItemNotice.SetActive(false);
                NPCNotice.SetActive(false);
                TriggerNotice.SetActive(false);
            }

        }
        else
        {
            ItemNotice.SetActive(false);
            NPCNotice.SetActive(false);
            TriggerNotice.SetActive(false);
        }


    }

    public void CatchAndSave(RaycastHit hitItem)
    {
        hitItem.collider.gameObject.SendMessage("ItemCollected");
        hitItem.collider.gameObject.SetActive(false);
        ItemNotice.SetActive(false);
    }
}
