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
        m_FinalQuest.title = "Complete Slinger's request";
        m_FinalQuest.desc = "You have suddenly travelled to Rome... First take Slinger's advice and go and help Vibia!";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
