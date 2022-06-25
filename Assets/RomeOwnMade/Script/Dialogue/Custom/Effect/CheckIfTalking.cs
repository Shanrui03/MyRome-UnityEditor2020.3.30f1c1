using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueQuests;

[CreateAssetMenu(fileName = "CheckIfTalking", menuName = "CustomEffect/CheckIfTalking")]
public class CheckIfTalking : CustomEffect
{
    public ActorData selectedActor;

    public override void DoEffect(Actor player)
    {
        selectedActor.isTalking = !selectedActor.isTalking;
    }
}
