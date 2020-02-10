using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum NPanel { Start, Reload, End}

public class NUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject StartPanel;
    public GameObject ReloadPanel;
    public GameObject EndPanel;
    [Header("Game")]
    public TextMeshProUGUI ScoreText;
    
    public void ShowPanel(NPanel panel)
    {
        switch(panel)
        {
            case NPanel.Start:
                StartPanel.SetActive(true);
                ReloadPanel.SetActive(false);
                EndPanel.SetActive(false);
                break;
            case NPanel.Reload:
                StartPanel.SetActive(false);
                ReloadPanel.SetActive(true);
                EndPanel.SetActive(false);
                break;
            case NPanel.End:
                StartPanel.SetActive(false);
                ReloadPanel.SetActive(false);
                EndPanel.SetActive(true);
                break;
        }
    }

    public void SetScore(int score,bool animation)
    {
        ScoreText.text = score.ToString();

        if (animation)
            ScoreText.GetComponent<Animation>().Play("GainAnim");
    }
}
