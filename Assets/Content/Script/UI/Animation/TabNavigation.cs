using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TabNavigation : MonoBehaviour
{
    [Header("Lista de InputFields en orden de navegación")]
    [SerializeField] private List<Selectable> inputFields;

    private int currentIndex = 0;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SelectNextInputField();
        }
    }

    private void SelectNextInputField()
    {
        if (inputFields == null || inputFields.Count == 0) return;

        // Obtener el elemento actualmente seleccionado en el EventSystem
        GameObject selected = EventSystem.current.currentSelectedGameObject;

        if (selected != null)
        {
            int index = inputFields.IndexOf(selected.GetComponent<Selectable>());
            if (index != -1)
            {
                currentIndex = (index + 1) % inputFields.Count; // Cicla entre los elementos
            }
        }

        // Seleccionar el siguiente input
        inputFields[currentIndex].Select();
    }
}
