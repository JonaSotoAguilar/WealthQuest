using System.Collections;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

public class PlayerNetUI : NetworkBehaviour
{
    [SerializeField] private UIPlayer ui;
    [SerializeField] private GameData data;

    // Questions
    private readonly SyncVar<int> questionIndex = new SyncVar<int>(-1);
    private readonly SyncVar<bool> questionAnswered = new SyncVar<bool>(false);
    private readonly SyncVar<bool> wasAnswerCorrect = new SyncVar<bool>(false);

    [SerializeField] TextMeshProUGUI questionText;

    private void Awake()
    {
        SuscribeEvents();
    }

    private void SuscribeEvents()
    {
        Debug.Log("Suscribiendo eventos");
        questionIndex.OnChange += OnQuestionChanged;
    }

    // 1. Crear pregunta
    [Server]
    public void CreateQuestion()
    {
        // FIXME: Revisar
        Test("Antes");

        int index = Random.Range(0, 10);
        Debug.Log("Pregunta index: " + index);

        Test("Despues" + index);

        questionIndex.Value = index;
    }

    [ObserversRpc]
    private void Test(string value)
    {
        Debug.Log("Test Question:" + value);
    }

    private void OnQuestionChanged(int oldQuestion, int newQuestion, bool asServer)
    {
        if (asServer) return;

        Debug.Log("Pregunta cambiada: " + newQuestion);
        QuestionData questionData = data.GetQuestionData(newQuestion);
        questionText.text = questionData.question;
        Debug.Log("Pregunta: " + questionData.question);

        //StartCoroutine(SetupQuestion());
    }

    private IEnumerator SetupQuestion()
    {
        //ui.SetupQuestion(question.Value, IsOwner);
        if (IsOwner) ui.OnQuestionAnswered += OnAnswerQuestion;

        yield return new WaitUntil(() => questionAnswered.Value);
        ui.ShowQuestion(false);

        if (wasAnswerCorrect.Value)
        {
            CmdSubmitAnswer(false, false);
            //EnableDice();
        }
        else
        {
            CmdSubmitAnswer(false, false);
            //FinishTurn();
        }
    }

    private void OnAnswerQuestion(bool isCorrect)
    {
        Debug.Log("Respuesta seleccionada: " + isCorrect);
        ui.OnQuestionAnswered -= OnAnswerQuestion;
        CmdSubmitAnswer(true, isCorrect);
    }

    [ServerRpc]
    private void CmdSubmitAnswer(bool answered, bool isCorrect)
    {
        wasAnswerCorrect.Value = isCorrect;
        questionAnswered.Value = answered;
        if (answered) ui.ShowQuestion(false);
    }
}
