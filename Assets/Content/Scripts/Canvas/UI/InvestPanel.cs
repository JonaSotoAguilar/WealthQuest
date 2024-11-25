using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Globalization;

public class InvestPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Button increaseAmount;
    [SerializeField] private Button lowerAmount;
    [SerializeField] private Button cancelButton;
    private int moneyPlayer;
    private CultureInfo chileanCulture = new CultureInfo("es-CL");

    public int MoneyPlayer { get => moneyPlayer; set => moneyPlayer = value; }

    private void Awake()
    {
        ResetAmount();
    }

    public void IncreaseAmount()
    {
        // Pasar de formato chileno a int
        string amount = amountText.text.Substring(1);
        int amountInt = int.Parse(amount.Replace(".", ""));
        if (amountInt + 100 <= moneyPlayer)
        {
            amountInt += 100;
            amountText.text = amountInt.ToString("C0", chileanCulture);
        }
    }

    public void LowerAmount()
    {
        string amount = amountText.text.Substring(1);
        int amountInt = int.Parse(amount.Replace(".", ""));
        if (amountInt - 100 >= 100)
        {
            amountInt -= 100;
            amountText.text = amountInt.ToString("C0", chileanCulture);
        }
    }

    public int GetInvestmentAmount()
    {
        string amount = amountText.text.Substring(1);
        return int.Parse(amount.Replace(".", ""));
    }

    public void ShowPanel(bool show)
    {
        gameObject.SetActive(show);
    }

    public void ResetAmount()
    {
        amountText.text = "$100";
    }

}
