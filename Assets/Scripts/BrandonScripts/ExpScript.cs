using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpScript : MonoBehaviour
{
    int level = 0;
    public static int exp;
    int lastexpValue;
    [SerializeField] int expToNextLevel = 20;
    int overFillExp;
    [SerializeField]ExpBar Bar;
    void Update()
    {
        if (exp >= expToNextLevel)
        {
            LevelUp();
        }
        if (exp != lastexpValue)
        {
            GainExp();
        }
    }
    //call this whenever you want the player to gain exp
    void GainExp()
    {
        Bar.UpdateValue(exp);
        lastexpValue = exp;
    }
    void LevelUp()
    {
        level++;
        CheckForExpOverFill();
        //this makes it so levels will take longer and longer to achive
        expToNextLevel = (int)Mathf.Round(expToNextLevel * 1.5f);
        exp = overFillExp;
        Bar.newMax(expToNextLevel, exp);
    }
    void CheckForExpOverFill()
    {
        overFillExp = exp - expToNextLevel;
    }
}
