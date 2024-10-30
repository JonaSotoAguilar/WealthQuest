using System.Collections.Generic;

[System.Serializable]
public class BGamesAttributes
{

    public int id_attributes;
    public string name;
    public string description;
    public string data_type;
    public string initiated_date;
    public string last_modified;
}

[System.Serializable]
public class BGamesAttributesList
{
    public List<BGamesAttributes> attributes;
}