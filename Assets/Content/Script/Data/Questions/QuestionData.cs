using Mirror;

[System.Serializable]
public class QuestionData
{
    // Question
    public string question;
    public string[] answers;
    public int indexCorrectAnswer;

    // Topic
    public string topic; // De 1 a 3.
    public string subTopic;
    public int level; // Es el puntaje que se le asigna a la pregunta

    public QuestionData() { }

    public QuestionData(string question, string[] answers, int indexCorrectAnswer, int scoreForCorrectAnswer)
    {
        this.question = question;
        this.answers = answers;
        this.indexCorrectAnswer = indexCorrectAnswer;
    }

    #region Write and Read

    // FIXME: Agregar el resto de variables
    public static void WriteQuestionData(NetworkWriter writer, QuestionData questionData)
    {
        writer.WriteString(questionData.question);
        writer.WriteInt(questionData.answers.Length);
        foreach (var answer in questionData.answers)
        {
            writer.WriteString(answer);
        }
        writer.WriteInt(questionData.indexCorrectAnswer);
    }

    public static QuestionData ReadQuestionData(NetworkReader reader)
    {
        string question = reader.ReadString();
        int answersCount = reader.ReadInt();
        string[] answers = new string[answersCount];
        for (int i = 0; i < answersCount; i++)
        {
            answers[i] = reader.ReadString();
        }

        int indexCorrectAnswer = reader.ReadInt();
        int scoreForCorrectAnswer = reader.ReadInt();
        return new QuestionData(question, answers, indexCorrectAnswer, scoreForCorrectAnswer);
    }

    #endregion

}
