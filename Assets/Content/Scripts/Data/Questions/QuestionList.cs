[System.Serializable]
public class QuestionList
{
    public QuestionData[] questions;

    public QuestionList() { }

    public QuestionList(QuestionData[] questions)
    {
        this.questions = questions;
    }
}
