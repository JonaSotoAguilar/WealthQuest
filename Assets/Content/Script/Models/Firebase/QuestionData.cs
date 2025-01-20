using System.Diagnostics;
using Firebase.Firestore;

[FirestoreData]
public class QuestionData
{
    // Question
    private string question;
    private string[] answers;
    private int indexCorrectAnswer;

    // Content
    private string topic;
    private string subTopic;
    private int level;

    [FirestoreProperty] public string Question { get => question; set => question = value; }
    [FirestoreProperty] public string[] Answers { get => answers; set => answers = value; }
    [FirestoreProperty] public int IndexCorrectAnswer { get => indexCorrectAnswer; set => indexCorrectAnswer = value; }
    [FirestoreProperty] public string Topic { get => topic; set => topic = value; }
    [FirestoreProperty] public string SubTopic { get => subTopic; set => subTopic = value; }
    [FirestoreProperty] public int Level { get => level; set => level = value; }

    public QuestionData() { }

    public QuestionData(Question question)
    {
        Question = question.question;
        Answers = question.answers;
        IndexCorrectAnswer = question.indexCorrectAnswer;
        Topic = question.topic;
        SubTopic = question.subTopic;
        Level = question.level;
    }

}
