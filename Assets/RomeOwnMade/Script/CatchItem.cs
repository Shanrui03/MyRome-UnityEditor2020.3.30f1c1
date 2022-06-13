using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchItem : MonoBehaviour
{
    public GameObject ItemNotice;
    public float RayDistance = 3.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, RayDistance))
        {
            if (hit.collider.gameObject.tag == "Items")
            {
                ItemNotice.SetActive(true);
                if (Input.GetMouseButtonDown(0))
                {
                    CatchAndSave(hit);
                }
            }
            else
            {
                ItemNotice.SetActive(false);
            }

        }
        else
        {
            ItemNotice.SetActive(false);
        }


    }

    public void CatchAndSave(RaycastHit hitItem)
    {
        hitItem.collider.gameObject.SendMessage("ItemCollected");
        hitItem.collider.gameObject.SetActive(false);
        ItemNotice.SetActive(false);
    }
}
