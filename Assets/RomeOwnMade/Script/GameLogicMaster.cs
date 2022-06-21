using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueQuests;
public class GameLogicMaster : MonoBehaviour
{
    public QuestData m_FinalQuest;
    // Start is called before the first frame update
    void Start()
    {
        m_FinalQuest.desc = "Please Compelete 3 Quests!\r\n1.Help Sam solve his problems.";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
