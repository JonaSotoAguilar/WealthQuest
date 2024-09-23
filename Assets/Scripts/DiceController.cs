using System.Collections;
using UnityEngine;

public class DiceController : MonoBehaviour
{
    public bool mode2D;
    public bool diceSleeping;
    public bool isDiceLaunched = false; // Bandera para controlar si el dado ha sido lanzado
    public int diceRoll;
    public diceTypeList diceType;
    private float topSide;
    public Rigidbody myRigidbody;

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

    void Update()
    {
        // Lanzar dado al presionar espacio
        if (Input.GetKeyDown(KeyCode.Space) && !isDiceLaunched)
        {
            LaunchDice();
            isDiceLaunched = true;
            StartCoroutine(WaitAndCheckResult()); // Comienza la espera antes de comprobar el resultado
        }
    }

    IEnumerator WaitAndCheckResult()
    {
        yield return new WaitForSeconds(1); // Espera 3 segundos antes de verificar el resultado
        while (!myRigidbody.IsSleeping())
        {
            yield return null; // Espera hasta que el dado se detenga completamente
        }
        CheckResult();
    }

    void CheckResult()
    {
        topSide = mode2D ? 50000 : -50000;

        for (int index = 0; index < (int)diceType; index++)
        {
            var getChild = gameObject.transform.GetChild(index);

            if (!mode2D)
            {
                if (getChild.position.y > topSide)
                {
                    topSide = getChild.position.y;
                    diceRoll = index + 1;
                }
            }
            else
            {
                if (getChild.position.z < topSide)
                {
                    topSide = getChild.position.z;
                    diceRoll = index + 1;
                }
            }
        }

        diceSleeping = true;
        isDiceLaunched = false; // Restablecer para permitir futuros lanzamientos
        GameManager.instance.FinishDiceRoll(diceRoll); // Notificar al GameManager del resultado
    }

    public void LaunchDice()
    {
        Vector3 fuerzaAleatoria = new Vector3(Random.Range(-5f, 5f), 10f, Random.Range(-5f, 5f));
        Vector3 torqueAleatorio = new Vector3(Random.Range(-500f, 500f), Random.Range(-500f, 500f), Random.Range(-500f, 500f));
        myRigidbody.AddForce(fuerzaAleatoria, ForceMode.Impulse);
        myRigidbody.AddTorque(torqueAleatorio, ForceMode.Impulse);
        diceSleeping = false;
    }
}
