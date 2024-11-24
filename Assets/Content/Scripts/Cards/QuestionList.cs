using FishNet.CodeGenerating;
using FishNet.Serializing;

[System.Serializable]
public class QuestionList
{
    public QuestionData[] questions;
}

[System.Serializable]
public class QuestionData
{
    public string question;
    public string[] answers;
    public int indexCorrectAnswer;
    public int scoreForCorrectAnswer;
}

// public static class QuestionDataSerializer
// {
//     // Escribir QuestionData
//     public static void WriteQuestionData(this Writer writer, QuestionData value)
//     {
//         writer.WriteString(value.question);
//         writer.WriteInt32(value.answers.Length);
//         foreach (var answer in value.answers)
//         {
//             writer.WriteString(answer);
//         }
//         writer.WriteInt32(value.indexCorrectAnswer);
//         writer.WriteInt32(value.scoreForCorrectAnswer);
//     }

//     public static QuestionData ReadQuestionData(this Reader reader)
//     {
//         var questionData = new QuestionData
//         {
//             question = reader.ReadString()
//         };

//         int answerCount = reader.ReadInt32();
//         questionData.answers = new string[answerCount];
//         for (int i = 0; i < answerCount; i++)
//         {
//             questionData.answers[i] = reader.ReadString();
//         }
//         questionData.indexCorrectAnswer = reader.ReadInt32();
//         questionData.scoreForCorrectAnswer = reader.ReadInt32();

//         return questionData;
//     }

// }