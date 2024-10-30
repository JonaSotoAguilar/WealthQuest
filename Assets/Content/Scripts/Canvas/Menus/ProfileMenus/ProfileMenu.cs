using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;


public class ProfileMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject profilePanel;
    [SerializeField] private GameObject historyPanel;
    private GameObject activePanel;
    
    public void ShowPanel(bool visible)
    {
        gameObject.SetActive(visible);
        if (activePanel == null)
        {
            activePanel = profilePanel;
            activePanel.SetActive(visible);
        }
    }

    public void ShowProfile(bool visible)
    {
        if (activePanel != profilePanel)
        {
            activePanel.SetActive(false);
            profilePanel.SetActive(visible);
            activePanel = profilePanel;
        }
    }

    public void ShowHistory(bool visible)
    {
        if (activePanel != historyPanel)
        {
            activePanel.SetActive(false);
            historyPanel.SetActive(visible);
            activePanel = historyPanel;
        }
    }
}
