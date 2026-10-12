using System;
using JKFrame;
using UnityEngine;
using UnityEngine.UI;

[UIWindowData(nameof(UI_LoadingWindow), false,nameof(UI_LoadingWindow), 4)]
public class UI_LoadingWindow : UI_WindowBase
{
    [SerializeField] private Text desciptionText;
    [SerializeField] private Slider loadingBarSlider;
    [SerializeField] private Text progressText;

    public void Set(string desciption)
    {
        
        desciptionText.text = desciption;
    }

    public void UpdateProgress(float current, float max)
    {
        loadingBarSlider.maxValue = max;
        loadingBarSlider.value = current;
        progressText.text = $"{Math.Round(current, 2)}/{Math.Round(max, 2)}";
    }
    
    public void UpdateDownloadProgress(float currentBytes, float maxBytes)
    {
        float currentMB = currentBytes / 1024f / 1024f;
        float maxMB = maxBytes / 1024f / 1024f;
        loadingBarSlider.maxValue = maxMB;
        loadingBarSlider.value = currentMB;
        progressText.text = $"{Math.Round(currentMB, 2)}MB / {Math.Round(maxMB, 2)}MB";
    }
}
