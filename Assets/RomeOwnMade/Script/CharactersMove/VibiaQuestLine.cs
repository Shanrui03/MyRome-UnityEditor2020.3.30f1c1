using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueQuests;
public class VibiaQuestLine : MonoBehaviour
{
    static VibiaQuestLine instance;
    public MarkVibia VibiaEffect;
    public GameObject theCompass;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        instance = this;

        VibiaEffect.AddMark = true;
    }
    public static void AddorDeleteMarktoVibia(bool isMark)
    {
        if(isMark)
        {
            instance.gameObject.AddComponent<CompassMarkerScript>();
            instance.gameObject.GetComponent<CompassMarkerScript>().setMarkerIconType(TheCompassScript.IconType.eImportant);
            instance.gameObject.GetComponent<CompassMarkerScript>().showUpDownArrow = instance.theCompass.GetComponent<TheCompassScript>().drawUpDownArrows;
            instance.gameObject.GetComponent<CompassMarkerScript>().showDistanceLabel = instance.theCompass.GetComponent<TheCompassScript>().drawDistanceText;
            instance.gameObject.GetComponent<CompassMarkerScript>().showOffScreenHints = true;
            instance.gameObject.GetComponent<CompassMarkerScript>().initTrackingOfMarker();
        }
        else
        {
            Destroy(instance.gameObject.GetComponent<CompassMarkerScript>());
        }
    }
}
