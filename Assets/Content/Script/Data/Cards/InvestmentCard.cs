using UnityEngine;
using System.Collections.Generic;
using System.Globalization;
using Mirror;

[CreateAssetMenu(fileName = "InvestmentCard", menuName = "Cards/InvestmentCard")]
public class InvestmentCard : Card
{
    [Range(2, 15), Tooltip("Duración de pago.")] public int duration;
    [Min(2000), Tooltip("Año de inicio.")] public int startYear;
    [Range(-1, 110), Tooltip("Porcentaje de cambio previos.")] public List<float> pctChangePrevious;
    [Range(-1, 100), Tooltip("Porcentaje de cambio.")] public List<float> pctChange;
    [Range(0, 1), Tooltip("Porcentaje de dividendos.")] public List<float> pctDividend;

    public override string GetFormattedText(int scoreKFP)
    {
        string description = $"Invertir en el año <color=blue>{startYear}</color> durante <color=blue>{duration}</color> años. ";

        if (DistributeDividends())
        {
            description += "\n(Dividendos promedio de <color=green>" + (CalculateDividends() * 100) + "%</color>).";
        } else {
            description += "\n<color=red>(Sin dividendos).</color>";
        }
        return description;
    }

    public override void ApplyEffect(int capital, bool isLocalGame = true)
    {
        if (capital <= 0) return;

        Investment investment = new Investment(title, duration, capital, new List<float>(pctChange), new List<float>(pctDividend));
        if (isLocalGame)
            GameLocalManager.CurrentPlayer.Data.AddInvestment(investment);
        else
            GameNetManager.CurrentPlayer.Data.AddInvestment(investment);
    }

    public void GenerateGraphSprite()
    {
        int width = 256; // Ancho de la textura
        int height = 128; // Alto de la textura
        Texture2D texture = new Texture2D(width, height);

        // Colores
        Color backgroundColor = Color.black;
        Color lineColor = Color.green;

        // Limpiar la textura con el color de fondo
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                texture.SetPixel(x, y, backgroundColor);
            }
        }

        // Cálculo de puntos a partir de los porcentajes previos
        List<float> points = new List<float>();
        float currentValue = 100f; // Valor base inicial
        points.Add(currentValue); // Añadimos el valor base como el primer punto

        foreach (float pct in pctChangePrevious)
        {
            currentValue += currentValue * pct; // Aplica el porcentaje
            points.Add(currentValue);
        }

        // Normalizar los puntos en el rango de la textura
        float minValue = Mathf.Min(points.ToArray());
        float maxValue = Mathf.Max(points.ToArray());
        float range = maxValue - minValue;
        if (range == 0) range = 1; // Evitar división por 0

        // Calcular coordenadas en la textura
        List<Vector2> graphPoints = new List<Vector2>();
        float stepX = width / (float)(points.Count - 1);

        for (int i = 0; i < points.Count; i++)
        {
            float x = i * stepX;
            float y = Mathf.Lerp(0, height, (points[i] - minValue) / range);
            graphPoints.Add(new Vector2(x, y));
        }

        // Dibujar las líneas entre los puntos
        for (int i = 0; i < graphPoints.Count - 1; i++)
        {
            Vector2 start = graphPoints[i];
            Vector2 end = graphPoints[i + 1];
            DrawLine(texture, (int)start.x, (int)start.y, (int)end.x, (int)end.y, lineColor);
        }

        // Aplicar cambios a la textura
        texture.Apply();

        // Convertir la textura en un Sprite
        Rect rect = new Rect(0, 0, texture.width, texture.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        image = Sprite.Create(texture, rect, pivot);
    }

    // Dibuja una línea entre dos puntos en una textura
    private void DrawLine(Texture2D texture, int x1, int y1, int x2, int y2, Color color)
    {
        int dx = Mathf.Abs(x2 - x1);
        int dy = Mathf.Abs(y2 - y1);
        int sx = x1 < x2 ? 1 : -1;
        int sy = y1 < y2 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            texture.SetPixel(x1, y1, color);

            if (x1 == x2 && y1 == y2) break;
            int e2 = err * 2;
            if (e2 > -dy)
            {
                err -= dy;
                x1 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y1 += sy;
            }
        }
    }

    public float CalculateCompoundPercentage()
    {
        int initialCapital = 100;
        float finalCapital = initialCapital;

        foreach (float percentage in pctChangePrevious)
        {
            finalCapital += finalCapital * percentage;
        }

        // Calculamos el rendimiento compuesto como porcentaje
        float compoundPercentage = ((finalCapital / initialCapital) - 1) * 100;

        // Truncar a 2 decimales
        compoundPercentage = Mathf.Floor(compoundPercentage * 100) / 100;

        return compoundPercentage;
    }

    private bool DistributeDividends(){
        return pctDividend[0] > 0;
    }

    private float CalculateDividends(){
        float dividendAverage = 0;
        foreach (float pct in pctDividend)
        {
            dividendAverage += pct;
        }

        dividendAverage /= duration;
        dividendAverage = Mathf.Floor(dividendAverage * 10000) / 10000;

        return dividendAverage;
    }

    private void OnValidate()
    {
        AdjustListSize(ref pctChange, duration, 0);
        AdjustListSize(ref pctDividend, duration, 0);

        GenerateGraphSprite();
    }

    private void AdjustListSize<T>(ref List<T> list, int newSize, T defaultValue)
    {
        if (list == null) list = new List<T>();
        while (list.Count < newSize) list.Add(defaultValue);
        while (list.Count > newSize) list.RemoveAt(list.Count - 1);
    }
}
