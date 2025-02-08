using System;
using System.Collections.Generic;
using System.Diagnostics;

[System.Serializable]
public class Content
{
    public string uid;
    public string uidAuthor;
    public string name;
    public int version;
    public int downloadCount;
    public List<Question> questions = new List<Question>();

    public Content() { }

    public Content(string nameContent, List<Question> questions, int version = 1)
    {
        uid = GenerateUID();
        uidAuthor = ProfileUser.uid;
        name = nameContent;
        this.version = version;
        downloadCount = 0;
        this.questions = questions;
    }

    public string GenerateUID()
    {
        return System.Guid.NewGuid().ToString();
    }
}
