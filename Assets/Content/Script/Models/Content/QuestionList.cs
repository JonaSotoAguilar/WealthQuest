using System.Collections.Generic;

[System.Serializable]
public class QuestionList
{
    public List<Question> questions = new List<Question>();

    public QuestionList() { }

    public QuestionList(List<Question> questions)
    {
        this.questions = questions;
    }
}
