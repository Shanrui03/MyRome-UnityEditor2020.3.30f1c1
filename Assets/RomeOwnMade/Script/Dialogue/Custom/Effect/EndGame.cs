using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueQuests;

[CreateAssetMenu(fileName = "EndGame", menuName = "CustomEffect/EndGame")]
public class EndGame : CustomEffect
{
    public override void DoEffect(Actor player)
    {
        PauseMenu.GameIsEnd = true;
    }
}
