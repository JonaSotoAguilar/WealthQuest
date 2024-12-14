using System.Collections;
using TMPro;
using UnityEngine;

public class Popup : MonoBehaviour
{
    public static Popup Instance { get; private set; }

    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TextMeshProUGUI messagee;

    [Header("Menus")]
    [SerializeField] private GameObject createContent;
    [SerializeField] private GameObject contentMenu;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    public IEnumerator SuccessCreateContent()
    {
        messagee.text = "Contenido creado con éxito";
        popupPanel.SetActive(true);

        createContent.SetActive(false);
        contentMenu.SetActive(true);

        yield return new WaitForSeconds(1.5f);
        popupPanel.SetActive(false);
    }

    public IEnumerator SuccessUpdateContent()
    {
        messagee.text = "Contenido modificado con éxito";
        popupPanel.SetActive(true);

        createContent.SetActive(false);
        contentMenu.SetActive(true);

        yield return new WaitForSeconds(1.5f);
        popupPanel.SetActive(false);
    }

    public IEnumerator SuccessExportContent()
    {
        messagee.text = "Contenido exportado con éxito en Descargas";
        popupPanel.SetActive(true);

        createContent.SetActive(false);
        contentMenu.SetActive(true);

        yield return new WaitForSeconds(1.5f);
        popupPanel.SetActive(false);
    }

    public IEnumerator SuccessImportContent()
    {
        messagee.text = "Contenido importado con éxito";
        popupPanel.SetActive(true);

        createContent.SetActive(false);
        contentMenu.SetActive(true);

        yield return new WaitForSeconds(1.5f);
        popupPanel.SetActive(false);
    }


}
