using System.Diagnostics;
using Firebase.Firestore;
using Mirror;

[System.Serializable]
public class Question
{
    // Question
    public string question;
    public string[] answers;
    public int indexCorrectAnswer;

    // Topic
    public string topic;
    public string subTopic;
    public int level;

    public Question() { }

    public Question(string question, string[] answers, int indexCorrectAnswer, string topic, string subTopic, int level)
    {
        this.question = question;
        this.answers = answers;
        this.indexCorrectAnswer = indexCorrectAnswer;
        this.topic = topic;
        this.subTopic = subTopic;
        this.level = level;
    }

    #region Write and Read

    public static void WriteQuestionData(NetworkWriter writer, Question questionData)
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

    public static Question ReadQuestionData(NetworkReader reader)
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

        return new Question(question, answers, indexCorrectAnswer, topic, subTopic, level);
    }


    #endregion

}
