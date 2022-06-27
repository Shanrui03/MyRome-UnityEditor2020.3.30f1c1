using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueQuests;

[CreateAssetMenu(fileName = "MarkVibia", menuName = "CustomEffect/MarkVibia")]
public class MarkVibia : CustomEffect
{
    public bool AddMark = true;
    private void Awake()
    {
        AddMark = true;
    }
    public override void DoEffect(Actor player)
    {
        VibiaQuestLine.AddorDeleteMarktoVibia(AddMark);
        AddMark = !AddMark;
    }
}
