using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class MainMenuRome : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void PlayGame()
    {
        Application.LoadLevel("RomeScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
