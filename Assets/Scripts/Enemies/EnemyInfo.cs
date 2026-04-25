using UnityEngine;

public class EnemyInfo : MonoBehaviour
{
    public string EnemyType { get; private set; }
    public EnemyCategory Category { get; private set; }
    public bool IsElite { get; private set; }

    public void Initialize(string typeName, EnemyCategory category, bool isElite)
    {
        EnemyType = typeName;
        Category = category;
        IsElite = isElite;
    }
}