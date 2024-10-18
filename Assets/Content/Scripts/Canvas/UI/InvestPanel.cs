using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class InvestPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Button increaseAmount;
    [SerializeField] private Button lowerAmount;
    [SerializeField] private Button cancelButton;

    // Aumenta en 100 el monto de inversión
    public void IncreaseAmount()
    {
        // El monto tiene el formato $X.XXX,XXX
        string amount = amountText.text.Substring(1);
        // Convertir el monto a entero
        int amountInt = int.Parse(amount.Replace(",", ""));
        // Aumentar el monto en 100
        amountInt += 100;
        // Actualizar el texto del monto
        amountText.text = "$" + amountInt.ToString("N0");
    }

    // Disminuye en 100 el monto de inversión, no puede ser menor a 0
    public void LowerAmount()
    {
        // El monto tiene el formato $X.XXX,XXX
        string amount = amountText.text.Substring(1);
        // Convertir el monto a entero
        int amountInt = int.Parse(amount.Replace(",", ""));
        // Si el monto es menor a 100
        if (amountInt <= 100)
            return;
        // Disminuir el monto en 100
        amountInt -= 100;
        // Actualizar el texto del monto
        amountText.text = "$" + amountInt.ToString("N0");
    }

    // Obtener el monto de inversión
    public int GetInvestmentAmount()
    {
        // El monto tiene el formato $X.XXX,XXX
        string amount = amountText.text.Substring(1);
        // Convertir el monto a entero
        return int.Parse(amount.Replace(",", ""));
    }

    // Mostrar u ocultar botones y texto
    public void ShowPanel(bool show)
    {
        // Activar este objeto
        gameObject.SetActive(show);
    }

}
