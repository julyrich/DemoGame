using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealCooldownController : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] GameObject healRdyText;

    public bool healAvailable = true;

    void FixedUpdate()
    {
        if(slider.value < slider.maxValue)
        {
            slider.value += Time.fixedDeltaTime;

            if(slider.value >= slider.maxValue)
            {
                slider.value = slider.maxValue;
                healAvailable = true;
                healRdyText.SetActive(true);
            }
        }
    }

    public void SetCooldownTime(int time)
    {
        slider.maxValue = time;
        slider.value = time;
    }

    public void Healed()
    {
        healAvailable = false;
        slider.value = 0;
        healRdyText.SetActive(false);
    }
}
