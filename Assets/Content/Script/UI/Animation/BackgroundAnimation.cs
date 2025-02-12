using System.Collections;
using UnityEngine;

public class BackgroundAnimation : MonoBehaviour
{
    [Header("Background Elements")]
    [SerializeField] private RectTransform cloudParent1;
    [SerializeField] private RectTransform cloudParent2;
    [SerializeField] private RectTransform wave;

    [Header("Animation Wave")]
    public float waveMoveAmount = 20f; // Cuánto sube y baja
    public float waveSpeed = 2f; // Velocidad del movimiento de wave

    [Header("Animation Cloud")]
    public float cloudSpeed = 5f; // Velocidad del desplazamiento horizontal de la nube
    public float cloudFloatAmount = 10f; // Cuánto sube y baja la nube
    public float cloudFloatSpeed = 3f; // Velocidad de la animación de flotación

    private void Awake()
    {
        if (wave != null)
            StartCoroutine(WaveAnimation());

        if (cloudParent1 != null)
        {
            StartCoroutine(CloudAnimation(cloudParent1, 0f));
            StartCoroutine(CloudFloatAnimation(cloudParent1));
        }

        if (cloudParent2 != null)
        {
            StartCoroutine(CloudAnimation(cloudParent2, cloudSpeed / 2));
            StartCoroutine(CloudFloatAnimation(cloudParent2));
        }
    }

    private IEnumerator WaveAnimation()
    {
        while (true)
        {
            // Sube
            LeanTween.moveY(wave, wave.anchoredPosition.y + waveMoveAmount, waveSpeed)
                .setEase(LeanTweenType.easeInOutSine);

            yield return new WaitForSeconds(waveSpeed);

            // Baja
            LeanTween.moveY(wave, wave.anchoredPosition.y - waveMoveAmount, waveSpeed)
                .setEase(LeanTweenType.easeInOutSine);

            yield return new WaitForSeconds(waveSpeed);
        }
    }

    private IEnumerator CloudAnimation(RectTransform cloud, float delay)
    {
        yield return new WaitForSeconds(delay); // Espera el tiempo de retraso antes de empezar

        while (true)
        {
            // Inicia la nube desde la posición inicial
            cloud.anchoredPosition = new Vector2(-1920, cloud.anchoredPosition.y);

            // Mueve la nube de -1920 a 1920 en X
            LeanTween.moveX(cloud, 1920, cloudSpeed)
                .setEase(LeanTweenType.linear)
                .setOnComplete(() =>
                {
                    // Cuando llega a 1920, teletransportarla a -1920 y repetir
                    cloud.anchoredPosition = new Vector2(-1920, cloud.anchoredPosition.y);
                });

            yield return new WaitForSeconds(cloudSpeed); // Esperar hasta que termine la animación
        }
    }

    private IEnumerator CloudFloatAnimation(RectTransform cloud)
    {
        while (true)
        {
            // Sube un poco
            LeanTween.moveY(cloud, cloud.anchoredPosition.y + cloudFloatAmount, cloudFloatSpeed)
                .setEase(LeanTweenType.easeInOutSine);

            yield return new WaitForSeconds(cloudFloatSpeed);

            // Baja un poco
            LeanTween.moveY(cloud, cloud.anchoredPosition.y - cloudFloatAmount, cloudFloatSpeed)
                .setEase(LeanTweenType.easeInOutSine);

            yield return new WaitForSeconds(cloudFloatSpeed);
        }
    }
}
