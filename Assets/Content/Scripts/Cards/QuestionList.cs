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

