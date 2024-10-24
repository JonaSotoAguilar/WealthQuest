using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Character
{
    public string characterName;
    public Texture characterIcon;
    public GameObject characterPrefabs;
}


[CreateAssetMenu(fileName = "Characters", menuName = "User/Characters")]
public class CharactersDatabase : ScriptableObject
{
    public List<Character> characters;

    public int Length
    {
        get
        {
            return characters.Count;
        }
    }

    public Character GetCharacter(int index)
    {
        return characters[index];
    }

    public GameObject GetModel(int index)
    {
        return characters[index].characterPrefabs;
    }
}
