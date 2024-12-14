using System.Collections.Generic;

[System.Serializable]
public class QuestionList
{
    public List<QuestionData> questions = new List<QuestionData>();

    public QuestionList() { }

    public QuestionList(List<QuestionData> questions)
    {
        this.questions = questions;
    }
}
