using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public void ToGame()
    {
        SceneManager.LoadScene(0);
    }
    public void ToMenu()
    {
        SceneManager.LoadScene(1);
    }  
    public void ExitGame()
    {
        Application.Quit();
    }
    public void ToSurvey()
    {
        Application.OpenURL("https://docs.google.com/forms/d/1Ee7MXW0s_RsTq3yyAwkRUrxPPqh-JcGTkvFTmBWWfdw/edit");
    }
}
