using Mirror;

[System.Serializable]
public class QuestionData
{
    public string question;
    public string[] answers;
    public int indexCorrectAnswer;
    public int scoreForCorrectAnswer;

    public QuestionData() { }

    public QuestionData(string question, string[] answers, int indexCorrectAnswer, int scoreForCorrectAnswer)
    {
        this.question = question;
        this.answers = answers;
        this.indexCorrectAnswer = indexCorrectAnswer;
        this.scoreForCorrectAnswer = scoreForCorrectAnswer;
    }

    #region Write and Read

    public static void WriteQuestionData(NetworkWriter writer, QuestionData questionData)
    {
        // Escribir la pregunta
        writer.WriteString(questionData.question);

        // Escribir la cantidad de respuestas y cada respuesta
        writer.WriteInt(questionData.answers.Length);
        foreach (var answer in questionData.answers)
        {
            writer.WriteString(answer);
        }

        // Escribir el índice de la respuesta correcta
        writer.WriteInt(questionData.indexCorrectAnswer);

        // Escribir el puntaje por la respuesta correcta
        writer.WriteInt(questionData.scoreForCorrectAnswer);
    }

    public static QuestionData ReadQuestionData(NetworkReader reader)
    {
        // Leer la pregunta
        string question = reader.ReadString();

        // Leer la cantidad de respuestas y luego cada respuesta
        int answersCount = reader.ReadInt();
        string[] answers = new string[answersCount];
        for (int i = 0; i < answersCount; i++)
        {
            answers[i] = reader.ReadString();
        }

        // Leer el índice de la respuesta correcta
        int indexCorrectAnswer = reader.ReadInt();

        // Leer el puntaje por la respuesta correcta
        int scoreForCorrectAnswer = reader.ReadInt();

        // Crear y devolver la instancia de QuestionData
        return new QuestionData(question, answers, indexCorrectAnswer, scoreForCorrectAnswer);
    }

    #endregion

}
