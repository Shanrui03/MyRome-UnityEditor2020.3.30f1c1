using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueQuests;
public class GameLogicMaster : MonoBehaviour
{
    public QuestData m_FinalQuest;
    public GameObject quizUI;
    public static float lastAccuracy;
    public static int lastAnserint;
    // Start is called before the first frame update
    void Start()
    {
        m_FinalQuest.title = "Complete Slinger's request";
        m_FinalQuest.desc = "You have suddenly travelled to Rome... First take Slinger's advice and go and help Vibia!";
        lastAccuracy = 0f;
        lastAnserint = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void EnterQuiz()
    {
        quizUI.gameObject.SetActive(!quizUI.gameObject.activeSelf);
        Answer.isReload = false;
        PlayerMovement.isTalking = true;
        if (quizUI.gameObject.activeSelf)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
