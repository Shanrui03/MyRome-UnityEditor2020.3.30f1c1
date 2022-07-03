using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
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
    public GameObject PunishImage;
    public GameObject MilkPanel;
    public Text MilkSubmitNotice;
    public Text MilkCountDown;
    public Text MilkScore;

    [Header("FIGHT")]
    public GameObject markPoint;
    public GameObject startPoint;
    public GameObject playerPos;
    public GameObject EnemyPos;
    public GameObject fightPoint;
    public GameObject HpBarForPlayer;
    public GameObject playerHead;
    public Button finishBtn;
    public Button restartBtn;
    public Text finishTxt;
    public GameObject[] playerEquip;

    public static float lastAccuracy;
    public static int lastAnserint;
    public static bool isMilkUIShown;
    public static float countdownTime;
    public static int milkScroe;
    public static int finalmilkScore;

    private Vector3 waitPosition;
    private Vector3 fightPosition;
    // Start is called before the first frame update
    void Start()
    {
        waitPosition = markPoint.gameObject.transform.position;
        fightPosition = fightPoint.gameObject.transform.position;
        m_FinalQuest.title = "Complete Slinger's request";
        m_FinalQuest.desc = "You have suddenly travelled to Rome... First take Slinger's advice and go and help Vibia!";
        lastAccuracy = 0f;
        lastAnserint = 0;
        countdownTime = 30f;
        milkScroe = 0;
        finalmilkScore = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(isMilkUIShown)
        {
            PlayerMovement.isTalking = true;
            Cursor.lockState = CursorLockMode.None;
            countdownTime -= Time.deltaTime;
            if (milkScroe <= 0)
            {
                milkScroe = 0;
            }
            MilkCountDown.text = "Time:" + countdownTime;
            MilkScore.text = "Score:" + milkScroe;
            if(countdownTime <= 0f)
            {
                countdownTime = 30f;
                finalmilkScore = milkScroe;
                isMilkUIShown = false;
                MilkSubmitNotice.gameObject.SetActive(true);
                if (finalmilkScore >= 20)
                    MilkSubmitNotice.text = "Congratulations! You did it!\r\nPress T to Continue!";
                else
                    MilkSubmitNotice.text = "You have failed! Please try again!\r\nPress T to Continue!";
                MilkCountDown.text = "Time:0.0";
                RepeatCreating(false);
                for (int i = 0; i < MilkPanel.transform.childCount; i++)
                {
                    if (MilkPanel.transform.GetChild(i).gameObject.tag == "Milk")
                    {
                        Destroy(MilkPanel.transform.GetChild(i).gameObject);
                    }
                }
            }
        }

        if(MilkSubmitNotice.gameObject.activeSelf)
        {
            if(Input.GetKeyDown(KeyCode.T))
            {
                SubmitMilkScore();
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
            case 3://Titus's Quest End
                m_FinalQuest.title = "Go to Octavius";
                m_FinalQuest.desc = "Titus has given us a recommendation! Take the letter of recommendation and go to Octavius!";
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

    public void CreatePunish()
    {
        float x = Random.Range(-Screen.width * 2 / 3, Screen.width * 2 / 3);
        float y = Screen.height / 3;
        GameObject Punish = Instantiate(PunishImage, MilkPanel.transform);
        Punish.transform.localPosition = new Vector3(x, y, 0);
    }

    public void EnterMilk()
    {
        milkUI.SetActive(true);
        isMilkUIShown = true;
        MilkSubmitNotice.gameObject.SetActive(false);
        countdownTime = 30f;
        RepeatCreating(true);
        milkScroe = 0;

    }

    public void SubmitMilkScore()
    {
        milkUI.SetActive(false);
        PlayerMovement.isTalking = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void RepeatCreating(bool isReapting)
    {
        if(isReapting)
        {
            InvokeRepeating("CreateMilk", 0.1f, 0.6f);
            InvokeRepeating("CreatePunish", 0.1f, 1.5f);
        }
        else
        {
            CancelInvoke("CreateMilk");
            CancelInvoke("CreatePunish");
        }
    }

    public void EnterArena(bool isRespawn = false)
    {
        playerPos.transform.position = waitPosition;
        PlayerMovement.isInArena = true;
        HpBarForPlayer.SetActive(true);
        Camera.main.gameObject.transform.SetParent(playerHead.transform);
       
        if (isRespawn)
        {
            playerPos.gameObject.GetComponent<HealthSystemForDummies>().AddToCurrentHealth(1000);
            EnemyPos.gameObject.GetComponent<HealthSystemForDummies>().AddToCurrentHealth(2000);
            finishTxt.gameObject.SetActive(false);
            restartBtn.gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void LeaveArena()
    {
        playerPos.transform.position = startPoint.transform.position;
        PlayerMovement.isInArena = false;
        HpBarForPlayer.SetActive(false);
        Camera.main.gameObject.transform.SetParent(playerPos.transform);
        for (int i = 0; i < playerEquip.Length; i++)
        {
            playerEquip[i].gameObject.SetActive(false);
        }
        Cursor.lockState = CursorLockMode.Locked;
        finishBtn.gameObject.SetActive(false);
        finishTxt.gameObject.SetActive(false);
    }
    public void ShowWinTxt()
    {
        finishTxt.gameObject.SetActive(true);
        finishBtn.gameObject.SetActive(true);
        finishTxt.text = "Congratulations!you win!";
        Cursor.lockState = CursorLockMode.None;
    }
    public void ShowLoseTxt()
    {
        finishTxt.gameObject.SetActive(true);
        restartBtn.gameObject.SetActive(true);
        finishTxt.text = "Sorry!You Lose!";
        Cursor.lockState = CursorLockMode.None;
    }

    public void ReadyToFight()
    {
        playerPos.transform.position = fightPosition;
        for (int i = 0; i < playerEquip.Length; i++)
        {
            playerEquip[i].gameObject.SetActive(true);
        }
    }
}
