using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class QuestionLoader : MonoBehaviour
{
    [SerializeField] private GameData gameData;
    [SerializeField] private TextAsset jsonFile;

    public void Awake()
    {
        LoadQuestionList();
    }

    private void LoadQuestionList()
    {
        // Ruta completa al archivo JSON en tu proyecto
        QuestionList questionList = JsonUtility.FromJson<QuestionList>(jsonFile.text);

        // Convertir el array de preguntas a una lista
        List<QuestionData> questions = new List<QuestionData>(questionList.questions);

        // Asignar la lista de preguntas a GameData
        gameData.QuestionList = questions;
    }
}
