using UnityEngine;

public class Bosque : MonoBehaviour
{
    [Header("Bosque")]
    public int maxTrees = 20;
    public float spawnInterval = 4f;
    public float spawnRadius = 10f;
    public GameObject treePrefab;

    private float timer;

    public void Simulate(float h)
    {
        timer += h;

        if (timer < spawnInterval)
            return;

        timer = 0f;
        CrearArbolSiEsPosible();
    }

    private void CrearArbolSiEsPosible()
    {
        Arbol[] arbolesActivos = FindObjectsByType<Arbol>(FindObjectsSortMode.InstanceID);
        if (arbolesActivos.Length >= maxTrees)
            return;

        if (treePrefab == null)
            return;

        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        Vector3 pos = transform.position + new Vector3(offset.x, offset.y, 0f);
        Instantiate(treePrefab, pos, Quaternion.identity);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }

}
