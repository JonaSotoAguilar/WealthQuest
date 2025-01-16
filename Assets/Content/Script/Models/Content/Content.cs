using System.Collections.Generic;
using System.Diagnostics;

[System.Serializable]
public class Content
{
    public string uid;
    public string uidAuthor;
    public string name;
    public int version;
    public List<QuestionData> questions = new List<QuestionData>();

    public Content() { }

    public Content(string nameContent, List<QuestionData> questions)
    {
        uid = GenerateUID();
        uidAuthor = ProfileUser.uid;
        name = nameContent;
        version = 1;
        this.questions = questions;
    }

    public string GenerateUID()
    {
        return System.Guid.NewGuid().ToString();
    }
}
