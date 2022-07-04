using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Answer : MonoBehaviour
{
    //Read files
    string[][] ArrayX;//questions data
    string[] lineArray;//Questions data read in
    private int topicMax = 0;//Maximum number of questions
    private List<bool> isAnserList = new List<bool>();//Store the status of whether or not you have answered a question

    //Load Questions
    public GameObject tipsbtn;//Tip button
    public Text tipsText;//Tips
    public List<Toggle> toggleList;//Answer Toggle
    public Text indexText;//current Question's index
    public Text TM_Text;//Current question
    public List<Text> DA_TextList;//Choices
    private int topicIndex = 0;//Index of Cureest Question in Array

    //Button functions and prompt messages
    public Button BtnBack;//Previous
    public Button BtnNext;//Next
    public Button BtnTip;//Tips
    public Button BtnJump;//GOTO
    public InputField jumpInput;//Upcoming questions to jump to
    public Text TextAccuracy;//accuracy
    private int anserint = 0;//Number of questions answered
    private int isRightNum = 0;//Number of correct questions

    public static bool isReload = false;
    public static float Accuracy = 0f;

    void Awake()
    {
        TextCsv();
        LoadAnswer();
        isReload = false;
        Accuracy = 0f;
    }

    void Start()
    {
        LoadQuiz();
    }

    void LoadQuiz()
    {
        toggleList[0].onValueChanged.AddListener((isOn) => AnswerRightRrongJudgment(isOn, 0));
        toggleList[1].onValueChanged.AddListener((isOn) => AnswerRightRrongJudgment(isOn, 1));
        toggleList[2].onValueChanged.AddListener((isOn) => AnswerRightRrongJudgment(isOn, 2));
        toggleList[3].onValueChanged.AddListener((isOn) => AnswerRightRrongJudgment(isOn, 3));

        BtnTip.onClick.AddListener(() => Select_Answer(0));
        BtnBack.onClick.AddListener(() => Select_Answer(1));
        BtnNext.onClick.AddListener(() => Select_Answer(2));
        BtnJump.onClick.AddListener(() => Select_Answer(3));
    }

    private void Update()
    {
        if (this.gameObject.activeSelf && !isReload)
        {
            ReloadQuizUI();
        }
    }


    /*****************Reload******************/
    void ReloadQuizUI()
    {
        Array.Clear(ArrayX, 0, ArrayX.Length);
        Array.Clear(lineArray, 0, lineArray.Length);
        isAnserList.Clear();
        topicIndex = 0;
        anserint = 0;
        isRightNum = 0;
        TextAccuracy.text = "Accuracy:" + 0.00 + "%";
        Accuracy = 0f;
        TextCsv();
        LoadAnswer();
        isReload = true;
    }


    /*****************Read Text******************/
    void TextCsv()
    {
        //Read csv binary files 
        TextAsset binAsset = Resources.Load("YW", typeof(TextAsset)) as TextAsset;
        //Read the contents of each line  
        lineArray = binAsset.text.Split('\r');
        //Creating two-dimensional arrays  
        ArrayX = new string[lineArray.Length][];
        //Storing data from csv in a two-dimensional array  
        for (int i = 0; i < lineArray.Length; i++)
        {
            ArrayX[i] = lineArray[i].Split(':');
        }
        //Set questions' status
        topicMax = lineArray.Length;
        for (int x = 0; x < topicMax + 1; x++)
        {
            isAnserList.Add(false);
        }
    }

    /*****************Load Questions******************/
    void LoadAnswer()
    {
        for (int i = 0; i < toggleList.Count; i++)
        {
            toggleList[i].isOn = false;
        }
        for (int i = 0; i < toggleList.Count; i++)
        {
            toggleList[i].interactable = true;
        }
        
        tipsbtn.SetActive(false);
        tipsText.text = "";

        indexText.text = "QUESTION " + (topicIndex + 1) + ":";//CURRENT QUESTION
        TM_Text.text = ArrayX[topicIndex][1];//QUESTION CONTENT
        int idx = ArrayX[topicIndex].Length - 3;//OPTIONS NUM
        for (int x = 0; x < idx; x++)
        {
            DA_TextList[x].text = ArrayX[topicIndex][x + 2];//OPTIONS
        }
    }

    /*****************Buttons******************/
    void Select_Answer(int index)
    {
        switch (index)
        {
            case 0://Tips
                int idx = ArrayX[topicIndex].Length - 1;
                int n = int.Parse(ArrayX[topicIndex][idx]);
                string nM = "";
                switch (n)
                {
                    case 1:
                        nM = "A";
                        break;
                    case 2:
                        nM = "B";
                        break;
                    case 3:
                        nM = "C";
                        break;
                    case 4:
                        nM = "D";
                        break;
                }
                tipsText.text = "<color=#FFAB08FF>" +"Correct answer is:"+ nM + "</color>";
                break;
            case 1://Previous Question
                if (topicIndex > 0)
                {
                    topicIndex--;
                    LoadAnswer();
                }
                else
                {
                    tipsText.text = "<color=#27FF02FF>" + "This's the first question!" + "</color>";
                }
                break;
            case 2://Next Question
                if (topicIndex < topicMax-1)
                {
                    topicIndex++;
                    LoadAnswer();
                }
                else
                {
                    tipsText.text = "<color=#27FF02FF>" + "This's the last question!" + "</color>";
                }
                break;
            case 3://GO TO
                int x = int.Parse(jumpInput.text) - 1;
                if (x >= 0 && x < topicMax)
                {
                    topicIndex = x;
                    jumpInput.text = "";
                    LoadAnswer();
                }
                else
                {
                    tipsText.text = "<color=#27FF02FF>" + "Out of range!" + "</color>";
                }
                break;
        }
    }

    /*****************Judging******************/
    void AnswerRightRrongJudgment(bool check,int index)
    {
        if (check)
        {
            //Check if player' answer is right or not
            bool isRight;
            int idx = ArrayX[topicIndex].Length - 1;
            int n = int.Parse(ArrayX[topicIndex][idx]) - 1;
            if (n == index)
            {
                tipsText.text = "<color=#27FF02FF>" + "Correct!" + "</color>";
                isRight = true;
                tipsbtn.SetActive(true);
            }
            else
            {
                tipsText.text = "<color=#FF0020FF>" + "Wrong!" + "</color>";
                isRight = false;
                tipsbtn.SetActive(true);
            }

            //Accuracy
            if (isAnserList[topicIndex])
            {
                tipsText.text = "<color=#FF0020FF>" + "This question has been answered!" + "</color>";
            }
            else
            {
                anserint++;
                if (isRight)
                {
                    isRightNum++;
                }
                isAnserList[topicIndex] = true;
                TextAccuracy.text = "Accuracy：" + ((float)isRightNum / anserint * 100).ToString("f2") + "%";
                Accuracy = (float)isRightNum / anserint * 100;
            }

            //Disable the option
            for (int i = 0; i < toggleList.Count; i++)
            {
                toggleList[i].interactable = false;
            }
        }
    }

    /*****************Submit******************/
    public void SubmitAnswer()
    {
        GameLogicMaster.lastAccuracy = Accuracy;
        GameLogicMaster.lastAnserint = anserint;
        Cursor.lockState = CursorLockMode.Locked;
        //PlayerMovement.isTalking = false;
        this.gameObject.SetActive(false);
    }
}