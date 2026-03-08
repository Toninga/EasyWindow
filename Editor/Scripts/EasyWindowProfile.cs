using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomWindowProfile", menuName = "Scriptable Objects/CustomWindowProfile")]
public class EasyWindowProfile : ScriptableObject
{
    public List<ScriptableObject> MenuTargets = new();
}
