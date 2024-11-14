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
    private int amountInvest;
    private int moneyPlayer;
    private CultureInfo chileanCulture = new CultureInfo("es-CL");

    public int MoneyPlayer { get => moneyPlayer; set => moneyPlayer = value; }
    public int AmountInvest { get => amountInvest; }

    void OnEnable()
    {
        ResetAmount();
    }

    public void IncreaseAmount()
    {
        // Pasar de formato chileno a int
        string amount = amountText.text.Substring(1);
        amountInvest = int.Parse(amount.Replace(".", ""));
        if (amountInvest + 100 <= moneyPlayer)
        {
            amountInvest += 100;
            amountText.text = amountInvest.ToString("C0", chileanCulture);
        }
    }

    public void LowerAmount()
    {
        string amount = amountText.text.Substring(1);
        amountInvest = int.Parse(amount.Replace(".", ""));
        if (amountInvest - 100 >= 100)
        {
            amountInvest -= 100;
            amountText.text = amountInvest.ToString("C0", chileanCulture);
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
        amountInvest = 100;
    }

}
