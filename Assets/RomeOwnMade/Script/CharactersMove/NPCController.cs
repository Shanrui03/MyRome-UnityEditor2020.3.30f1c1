using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueQuests;
public class NPCController : MonoBehaviour
{
    public ActorData NPCActor;
    public Transform player;
    public GameObject theCompass;
    public GameObject markerPrefab;
    private Animator NPCAnimator;
    private Transform startTransform;
    private GameObject tempMarker;
    private void Awake()
    {
        NPCAnimator = this.gameObject.GetComponent<Animator>();
        NPCActor.isTalking = false;
    }
    // Update is called once per frame
    void Update()
    {
        if(NPCActor.isTalking)
        {
            NPCAnimator.SetBool("Talk", true);
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        }
        else
        {
            NPCAnimator.SetBool("Talk", false);
        }
    }

    public void MarkNPCorNot(bool addOrNot)
    {
        if (addOrNot)
        {
            this.gameObject.AddComponent<CompassMarkerScript>();
            this.gameObject.GetComponent<CompassMarkerScript>().setMarkerIconType(TheCompassScript.IconType.eImportant);
            this.gameObject.GetComponent<CompassMarkerScript>().showUpDownArrow = this.theCompass.GetComponent<TheCompassScript>().drawUpDownArrows;
            this.gameObject.GetComponent<CompassMarkerScript>().showDistanceLabel = this.theCompass.GetComponent<TheCompassScript>().drawDistanceText;
            this.gameObject.GetComponent<CompassMarkerScript>().showOffScreenHints = true;
            this.gameObject.GetComponent<CompassMarkerScript>().initTrackingOfMarker();

            tempMarker = Instantiate(markerPrefab, transform);
        }
        else
        {
            Destroy(this.gameObject.GetComponent<CompassMarkerScript>());
            Destroy(tempMarker.gameObject);
        }
    }

    public void SwitchToWaving(bool changeTo)
    {
        NPCAnimator.SetBool("Waving", changeTo);
    }
}
