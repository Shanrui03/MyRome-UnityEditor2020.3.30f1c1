using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DialogueQuests;
public class GameLogicMaster : MonoBehaviour
{
    [Header("QUEST")]
    public QuestData m_FinalQuest;

    [Header("QUIZ")]
    public GameObject quizUI;

    [Header("MILK")]
    public GameObject milkUI;
    public GameObject MilkImage;
    public GameObject MilkPanel;
    public Text MilkCountDown;
    public Text MilkScore;

    public static float lastAccuracy;
    public static int lastAnserint;
    public static bool isMilkUIShown;
    public static float countdownTime;
    public static int milkScroe;
    // Start is called before the first frame update
    void Start()
    {
        m_FinalQuest.title = "Complete Slinger's request";
        m_FinalQuest.desc = "You have suddenly travelled to Rome... First take Slinger's advice and go and help Vibia!";
        lastAccuracy = 0f;
        lastAnserint = 0;
        countdownTime = 15f;
        milkScroe = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            EnterMilk();
        }

        if(isMilkUIShown)
        {
            countdownTime -= Time.deltaTime;
            MilkCountDown.text = "Time:" + countdownTime;
            MilkScore.text = "Score:" + milkScroe;
            if(countdownTime <= 0f)
            {
                countdownTime = 15f;
                isMilkUIShown = false;
                MilkCountDown.text = "Time:0.0";
                CancelInvoke("CreateMilk");
                for (int i = 0; i < MilkPanel.transform.childCount; i++)
                {
                    if (MilkPanel.transform.GetChild(i).gameObject.tag == "Milk")
                    {
                        Destroy(MilkPanel.transform.GetChild(i).gameObject);
                    }
                }
            }
        }
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

    public void ChangeDescOfMain(int Quest)
    {
        switch (Quest)
        {
            case 1://Vibia's Quest Complete
                m_FinalQuest.title = "Go back to Slinger";
                m_FinalQuest.desc = "Success in helping Vibia! Let's go back to Slinger!";
                break;
            case 2://Titus's Quest Begin
                m_FinalQuest.title = "Go to Titus";
                m_FinalQuest.desc = "In order to see the Emperor, we have to go to Titus first to ask for recommendations!";
                break;
        }

    }

    public void CreateMilk()
    {
        float x = Random.Range(-Screen.width*2 / 3, Screen.width*2 / 3);
        float y = Screen.height / 3;
        GameObject Milk = Instantiate(MilkImage, MilkPanel.transform);
        Milk.transform.localPosition = new Vector3(x, y, 0);
    }

    public void EnterMilk()
    {
        milkUI.SetActive(!milkUI.activeSelf);
        isMilkUIShown = milkUI.activeSelf;
        countdownTime = 15f;
        if (isMilkUIShown)
        {
            InvokeRepeating("CreateMilk", 0.1f, 0.6f);
            milkScroe = 0;
        }
        else
        {
            CancelInvoke("CreateMilk");
            for (int i = 0; i < MilkPanel.transform.childCount; i++)
            {
                if (MilkPanel.transform.GetChild(i).gameObject.tag == "Milk")
                {
                    Destroy(MilkPanel.transform.GetChild(i).gameObject);
                }
            }
        }
    }

    public void SubmitMilkScore()
    {
        milkUI.SetActive(false);
    }
}
