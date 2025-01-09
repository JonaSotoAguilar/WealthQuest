using Mirror;

[System.Serializable]
public class QuestionData
{
    // Question
    public string question;
    public string[] answers;
    public int indexCorrectAnswer;

    // Content
    public string topic;
    public string subTopic;
    public int level; // Difficulty and points

    public QuestionData() { }

    public QuestionData(string question, string[] answers, int indexCorrectAnswer, string topic, string subTopic, int level)
    {
        this.question = question;
        this.answers = answers;
        this.indexCorrectAnswer = indexCorrectAnswer;
        this.topic = topic;
        this.subTopic = subTopic;
        this.level = level;
    }

    #region Write and Read

    public static void WriteQuestionData(NetworkWriter writer, QuestionData questionData)
    {
        writer.WriteString(questionData.question);

        writer.WriteInt(questionData.answers.Length);
        foreach (var answer in questionData.answers)
        {
            writer.WriteString(answer);
        }

        writer.WriteInt(questionData.indexCorrectAnswer);
        writer.WriteString(questionData.topic);
        writer.WriteString(questionData.subTopic);
        writer.WriteInt(questionData.level);
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
        string topic = reader.ReadString();
        string subTopic = reader.ReadString();
        int level = reader.ReadInt();

        return new QuestionData(question, answers, indexCorrectAnswer, topic, subTopic, level);
    }


    #endregion

}
