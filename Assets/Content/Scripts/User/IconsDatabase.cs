using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Icons", menuName = "User/Icons")]
public class IconsDatabase : ScriptableObject
{
    public List<Texture> icons;

    public int Length
    {
        get
        {
            return icons.Count;
        }
    }

    public Texture GetIcon(int index)
    {
        return icons[index];
    }
}
