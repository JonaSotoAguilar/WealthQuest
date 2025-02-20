using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;

    [Header("Canvas Group")]
    public CanvasGroup canvasGroup;
    public float fadeDuration = 1f;

    [Header("Traslation")]
    [SerializeField] private RectTransform image1;
    [SerializeField] private RectTransform image2;
    public float traslationDuration = 1f;

    // Variable control
    private bool firstLoad = true;
    private string previousScene = "";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (firstLoad)
        {
            firstLoad = false;
            previousScene = scene.name;
            return;
        }

        // Verificar si la escena cargada es diferente a la anterior
        if (scene.name == previousScene)
        {
            Debug.Log("La escena cargada es la misma. No se ejecuta FadeOut ni TranslateOut.");
            return;
        }

        previousScene = scene.name;
        FadeOut();
        TranslateOut();
    }


    public void LoadScene(string sceneName)
    {
        FadeIn();
        LoadSceneTranslateIn(sceneName);
    }

    public void LoadSceneNet()
    {
        FadeIn();
        TranslateIn();
    }

    #region Fade effect

    public void LoadSceneFadeIn(string sceneName)
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            LeanTween.alphaCanvas(canvasGroup, 1f, fadeDuration)
                .setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(() => SceneManager.LoadScene(sceneName));
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    public void FadeIn()
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            LeanTween.alphaCanvas(canvasGroup, 1f, fadeDuration)
                .setEase(LeanTweenType.easeInOutQuad);
        }
    }


    public void FadeOut()
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            LeanTween.alphaCanvas(canvasGroup, 0f, fadeDuration)
                .setEase(LeanTweenType.easeInOutQuad);
        }
    }

    #endregion


    #region Traslation effect 

    public void LoadSceneTranslateIn(string sceneName)
    {
        if (image1 != null && image2 != null)
        {
            // Mueve los paneles desde fuera (-1440, 1440) hasta el centro (-480, 480)
            image1.anchoredPosition = new Vector2(-1440, image1.anchoredPosition.y);
            image2.anchoredPosition = new Vector2(1440, image2.anchoredPosition.y);

            LeanTween.moveX(image1, -480, traslationDuration).setEase(LeanTweenType.easeInOutQuad);
            LeanTween.moveX(image2, 480, traslationDuration)
                .setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(() =>
                {
                    LeanTween.delayedCall(0.5f, () => // Espera 0.5 segundos antes de cambiar la escena
                    {
                        SceneManager.LoadScene(sceneName);
                    });
                });
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    public void TranslateIn()
    {
        if (image1 != null && image2 != null)
        {
            // Mueve los paneles desde fuera (-1440, 1440) hasta el centro (-480, 480)
            image1.anchoredPosition = new Vector2(-1440, image1.anchoredPosition.y);
            image2.anchoredPosition = new Vector2(1440, image2.anchoredPosition.y);

            LeanTween.moveX(image1, -480, traslationDuration).setEase(LeanTweenType.easeInOutQuad);
            LeanTween.moveX(image2, 480, traslationDuration).setEase(LeanTweenType.easeInOutQuad);
        }
    }

    public void TranslateOut()
    {
        if (image1 != null && image2 != null)
        {
            // Mueve los paneles desde el centro (-480, 480) hacia fuera (-1440, 1440)
            LeanTween.moveX(image1, -1440, traslationDuration).setEase(LeanTweenType.easeInOutQuad);
            LeanTween.moveX(image2, 1440, traslationDuration).setEase(LeanTweenType.easeInOutQuad);
        }
    }

    #endregion

}
