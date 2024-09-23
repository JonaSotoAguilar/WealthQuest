using System.Collections;
using UnityEngine;

public class DeterministicDice : MonoBehaviour
{
    public bool mode2D;
    public bool diceSleeping;
    public bool deterministicRoll;
    public int diceRoll;
    public int diceRollDeterministic;
    public diceTypeList diceType;
    private int diceSides;
    private float topSide;
    [Range(0, 10000)]
    public float edgelandingforce = 1000f;
    public bool edgelandinginverter = false;
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
        // Lanza el dado al presionar la tecla Espacio
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LanzarDado();
        }

        // Solo ejecuta cuando el dado ha dejado de rodar
        if (myRigidbody.IsSleeping())
        {
            // Solo ejecuta si aún no se ha determinado el resultado
            if (!diceSleeping)
            {
                // Reinicia la variable que contiene la cara superior del dado
                topSide = mode2D ? 50000 : -50000;

                // Recorre los lados del dado y encuentra cuál tiene la mayor posición en Y (o Z para 2D)
                for (int index = 0; index < (int)diceType; index++)
                {
                    var getChild = gameObject.transform.GetChild(index);

                    // Si estamos en 3D, usa el eje Y para el cálculo
                    if (!mode2D)
                    {
                        if (getChild.position.y > topSide)
                        {
                            topSide = getChild.position.y;
                            diceRoll = index + 1;
                        }
                    }
                    // Si estamos en 2D, usa el eje Z
                    else
                    {
                        if (getChild.position.z < topSide)
                        {
                            topSide = getChild.position.z;
                            diceRoll = index + 1;
                        }
                    }
                }

                // Imprime el resultado del dado en la consola
                Debug.Log("Resultado del dado: " + diceRoll);

                // Indica que el dado ha dejado de moverse y se ha registrado el resultado
                diceSleeping = true;
            }
        }
        else
        {
            diceSleeping = false; // El dado está rodando de nuevo
        }
    }

    // Método para lanzar el dado con fuerza y torque aleatorios
    public void LanzarDado()
    {
        // Aplicar una fuerza y rotación aleatoria al dado
        Vector3 fuerzaAleatoria = new Vector3(Random.Range(-5f, 5f), 10f, Random.Range(-5f, 5f));
        Vector3 torqueAleatorio = new Vector3(Random.Range(-500f, 500f), Random.Range(-500f, 500f), Random.Range(-500f, 500f));

        myRigidbody.AddForce(fuerzaAleatoria, ForceMode.Impulse);
        myRigidbody.AddTorque(torqueAleatorio, ForceMode.Impulse);

        // Reiniciar el estado del dado al lanzarlo
        diceSleeping = false;
    }
}
