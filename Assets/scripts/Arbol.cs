using UnityEngine;

public class Arbol : MonoBehaviour
{
    [Header("Árbol")]
    public bool isAlive = true;
    public float woodYield = 1f;

    public float Harvest()
    {
        if (!isAlive) return 0f;

        isAlive = false;
        Destroy(gameObject);
        return woodYield;
    }
}