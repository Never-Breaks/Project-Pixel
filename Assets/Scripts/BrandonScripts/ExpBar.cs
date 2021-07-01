using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpBar : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] CanvasGroup cg;
    float fadeTimer = 3;
    void Update()
    {
        fadeTimer -= Time.fixedDeltaTime;
        if (fadeTimer < 0)
        {
            cg.alpha -= Time.fixedDeltaTime;
        }
        if (cg.alpha < 0)
        {
            slider.gameObject.SetActive(false);
        }
    }

    public void newMax(int maxValue, int overFillValue)
    {
        slider.maxValue = maxValue;
        slider.value = overFillValue;
    }
    public void UpdateValue(int newValue)
    {
        slider.value = newValue;
        slider.gameObject.SetActive(true);
        fadeTimer = 3;
        cg.alpha = 1;
    }
}
