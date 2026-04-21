using UnityEngine;

public class EnemyInfo : MonoBehaviour
{
    public string EnemyType { get; private set; }
    public bool IsElite { get; private set; }

    public void Initialize(string typeName, bool isElite)
    {
        EnemyType = typeName;
        IsElite = isElite;
    }
}