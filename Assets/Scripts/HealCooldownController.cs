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
        //the slider represents the heal cooldown
        if(slider.value < slider.maxValue)
        {
            slider.value += Time.fixedDeltaTime;

            //if the slider is full heal has cooldowned
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
        //reset cooldown bar if player used the heal
        healAvailable = false;
        slider.value = 0;
        healRdyText.SetActive(false);
    }
}
