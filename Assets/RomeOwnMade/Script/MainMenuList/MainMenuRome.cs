using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class MainMenuRome : MonoBehaviour
{
    public GameObject SettingsMenu;
    public GameObject BGM;
    public GameObject InGameUI;
    public GameObject loadingUI;

    [SerializeField]
    Text Aegis_text;

    [SerializeField]
    Slider slider;

    private AsyncOperation operation;
    float pointCount;
    float progress = 0;
    float total_time = 3f;
    float time = 0;

    bool isStart = false;

    private void Awake()
    {
        SettingsMenu.SetActive(false);
        isStart = false;
        pointCount = 0;
        progress = 0;
        total_time = 3f;
        time = 0;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        BGM.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("Volume");
        if(isStart)
        {
            time += Time.deltaTime;
            progress = time / total_time;
            if (progress >= 1)
            {
                operation.allowSceneActivation = true;
                return;
            }
            slider.value = progress;
        }
    }
    public void PlayGame()
    {
        SceneManager.LoadScene("RomeScene");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void ShowSettingsMenu()
    {
        SettingsMenu.SetActive(!SettingsMenu.activeSelf);
        this.gameObject.SetActive(!this.gameObject.activeSelf);
    }
    public void OnClickStart()
    {
        isStart = true;
        InGameUI.SetActive(false);
        loadingUI.SetActive(true);
        StartCoroutine("AegisAnimation");
        StartCoroutine("LoadLeaver");
    }
    IEnumerator AegisAnimation()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            float f = slider.value;
            string reminder = "";
            if (f < 0.25f)
            {
                reminder = "Testing balance in...";
            }
            else if (f < 0.5f)
            {
                reminder = "Injected into the Trojan horse...";
            }
            else if (f < 0.75f)
            {
                reminder = "Code breaking in progress...";
            }
            else
            {
                reminder = "Uploading data in...";
            }

            pointCount++;
            if (pointCount == 7)
            {
                pointCount = 0;
            }
            for (int i = 0; i < pointCount; i++)
            {
                reminder += ".";
            }

            Aegis_text.text = reminder;
        }
    }
    IEnumerator LoadLeaver()
    {
        operation = SceneManager.LoadSceneAsync("RomeScene"); 
        operation.allowSceneActivation = false;
        //while (!operation.isDone)
        //{
        //    slider.value = operation.progress;
        //    Aegis_text.text = (operation.progress * 100).ToString() + "%";
        //    yield return null;
        //}
        yield return null;
    }
    void OnDisable()
    {
        StopCoroutine("AegisAnimation");
        StopCoroutine("LoadLeaver");
    }
}
