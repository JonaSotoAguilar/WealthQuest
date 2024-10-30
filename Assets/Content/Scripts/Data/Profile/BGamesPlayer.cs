using System.Collections.Generic;


[System.Serializable]
public class BGamesPlayer
{
    public string name;
    public string password;
    public string email;
    public int age;
    public string external_type;
    public int external_id;
    public int id_players;
}

[System.Serializable]
public class BGamesPlayerList
{
    public List<BGamesPlayer> players;
}

