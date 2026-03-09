using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EasyWindowProfile", menuName = "Scriptable Objects/Easy Window Profile")]
public class EasyWindowProfile : ScriptableObject
{
    public List<ScriptableObject> MenuTargets = new();
}
