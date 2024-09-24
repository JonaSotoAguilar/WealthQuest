using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int turno = 1;      // Control del turno actual
    public int dinero = 100;   // Dinero del jugador
    public int puntaje = 0;    // Puntaje acumulado del jugador

    public void ActualizarTurno()
    {
        turno++;
        //Debug.Log("Turno actualizado: " + turno);
    }

    public void ModificarDinero(int cantidad)
    {
        dinero += cantidad;
        //Debug.Log("Dinero actual: " + dinero);
    }

    public void ModificarPuntaje(int puntos)
    {
        puntaje += puntos;
        //Debug.Log("Puntaje actual: " + puntaje);
    }

    public void MostrarStats()
    {
        Debug.Log("Turno: " + turno + ", Dinero: " + dinero + ", Puntaje: " + puntaje);
    }
}
