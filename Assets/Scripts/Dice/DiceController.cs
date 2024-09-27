using System.Collections;
using UnityEngine;

public class DiceController : MonoBehaviour
{
    public bool mode2D;
    public int diceRoll;
    public diceTypeList diceType;
    private float topSide;
    public Rigidbody myRigidbody;

    // Flag
    public bool diceSleeping;

    public enum diceTypeList
    {
        D4 = 4,
        D6 = 6,
        D8 = 8,
        D10 = 10,
        D12 = 12,
        D16 = 16,
        D20 = 20,
    }

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();
    }

    // Lanzar el dado
    public void LaunchDice()
    {
        // Aplicar fuerza y torque aleatorio al dado
        diceSleeping = false;
        Vector3 fuerzaAleatoria = new Vector3(Random.Range(-5f, 5f), 10f, Random.Range(-5f, 5f));
        Vector3 torqueAleatorio = new Vector3(Random.Range(-500f, 500f), Random.Range(-500f, 500f), Random.Range(-500f, 500f));
        myRigidbody.AddForce(fuerzaAleatoria, ForceMode.Impulse);
        myRigidbody.AddTorque(torqueAleatorio, ForceMode.Impulse);

        // Comenzar la rutina para verificar el resultado una vez se detenga el dado
        StartCoroutine(WaitAndCheckResult());
    }

    // Corutina para esperar hasta que el dado se detenga
    IEnumerator WaitAndCheckResult()
    {
        yield return new WaitForSeconds(1); // Espera un segundo antes de verificar si el dado está en reposo

        // Espera hasta que el dado se detenga completamente
        while (!myRigidbody.IsSleeping())
        {
            yield return null;
        }

        

        // Una vez que el dado se haya detenido, verificamos el resultado
        CheckResult();
    }

    // Verificar el resultado del lanzamiento del dado
    void CheckResult()
    {
        topSide = mode2D ? 50000 : -50000;

        // Recorremos los hijos del dado (las caras) para determinar cuál está arriba
        for (int index = 0; index < (int)diceType; index++)
        {
            var getChild = gameObject.transform.GetChild(index);

            if (!mode2D)
            {
                // Modo 3D: Comparamos la posición Y para determinar la cara superior
                if (getChild.position.y > topSide)
                {
                    topSide = getChild.position.y;
                    diceRoll = index + 1;
                }
            }
            else
            {
                // Modo 2D: Comparamos la posición Z para determinar la cara frontal
                if (getChild.position.z < topSide)
                {
                    topSide = getChild.position.z;
                    diceRoll = index + 1;
                }
            }
        }
        diceSleeping = true;
    }

    // Método para verificar si el dado ha dejado de moverse
    public bool IsDiceStopped()
    {
        return diceSleeping;
    }
}
