using UnityEngine;

[CreateAssetMenu(fileName = "GlobalParameters", menuName = "Scriptable Objects/GlobalParameters")]
public class GlobalParameters : ScriptableObject
{
    public Language Language;
    public string SaveFilePath;
    public bool RecordStatistics;


}

public enum Language
{
    English,
    French,
    Italian,
    German,
    Spanish,
    Mandarin,
    Japanese
}