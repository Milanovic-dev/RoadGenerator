using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; set; }
    [Header("Refs")]
    public TextMeshProUGUI Flips;
    public Image ProgressBar;
    public RectTransform ProgressBarStart;
    public RectTransform ProgressBarEnd;
    [Header("Prefabs")]
    public Image ProgressIcon;

    private Animator FlipsTextAnimator;
    private List<Image> PlayersIcons;

    private void Awake()
    {
        instance = this;
        FlipsTextAnimator = Flips.GetComponent<Animator>();

        Image go = Instantiate(ProgressIcon, ProgressBar.rectTransform);
        go.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Player";
        go.GetComponent<RectTransform>().anchoredPosition = ProgressBarStart.anchoredPosition;
        PlayersIcons = new List<Image>();
        PlayersIcons.Add(go);
    }

    public static void NumOfFlips(int flips)
    {
        instance.Flips.text = flips.ToString() + "X FLIPS";

        instance.FlipsTextAnimator.Play("FlipCountAnim",0,0f);
    }
    
    public static void Progress(float value)
    {
        value = Mathf.Clamp(value, 0f, 1f);
        float posX = (instance.ProgressBarEnd.anchoredPosition.x - instance.ProgressBarStart.anchoredPosition.x) * value;
        RectTransform icon = instance.PlayersIcons[0].GetComponent<RectTransform>();

        icon.anchoredPosition = new Vector3(posX, icon.anchoredPosition.y);
    }

}
