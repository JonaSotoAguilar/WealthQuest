using System.Collections.Generic;
using Firebase.Firestore;

[FirestoreData]
public class ContentData
{
    private string uidAuthor;
    private string name;
    private int version;
    private int downloadCount;
    private bool isPublic;
    private List<QuestionData> questions = new List<QuestionData>();

    [FirestoreProperty] public string UIDAuthor { get => uidAuthor; set => uidAuthor = value; }
    [FirestoreProperty] public string Name { get => name; set => name = value; }
    [FirestoreProperty] public int Version { get => version; set => version = value; }
    [FirestoreProperty] public int DownloadCount { get => downloadCount; set => downloadCount = value; }
    [FirestoreProperty] public bool IsPublic { get => isPublic; set => isPublic = value; }
    [FirestoreProperty] public List<QuestionData> Questions { get => questions; set => questions = value; }

    public ContentData() { }

    public ContentData(Content content)
    {
        uidAuthor = content.uidAuthor;
        name = content.name;
        version = content.version;
        downloadCount = content.downloadCount;
        isPublic = true;

        // Convertir la lista de Questions a QuestionData
        questions = new List<QuestionData>();
        foreach (var question in content.questions)
        {
            questions.Add(new QuestionData(question));
        }
    }
}
