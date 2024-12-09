using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Character
{
    public int characterID;
    public string characterName;
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

    private void OnValidate()
    {
        if (characters == null) return;

        for (int i = 0; i < characters.Count; i++)
        {
            characters[i].characterID = i; // Asignar el índice como ID

            // Configurar el nombre del personaje igual al prefab si está asignado
            if (characters[i].characterPrefabs != null)
            {
                characters[i].characterName = characters[i].characterPrefabs.name;
            }
            else
            {
                characters[i].characterName = "Unnamed Character"; // Nombre predeterminado si no hay prefab
            }
        }
    }

}
