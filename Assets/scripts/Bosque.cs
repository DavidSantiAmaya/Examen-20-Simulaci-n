using UnityEngine;

public class Bosque : MonoBehaviour
{
    [Header("Bosque")]
    public int maxArboles = 20;
    public float intervaloAparicion = 4f;
    public float RangoDeAparicion = 10f;
    public GameObject Prefab;

    private float timer;

    public void Simulate(float h)
    {
        timer += h;

        if (timer < intervaloAparicion)
            return;

        timer = 0f;
        CrearArboles();
    }

    private void CrearArboles()
    {
        Arbol[] arbolesActivos = FindObjectsByType<Arbol>(FindObjectsSortMode.InstanceID);
        if (arbolesActivos.Length >= maxArboles)
            return;

        if (Prefab == null)
            return;

        Vector2 offset = Random.insideUnitCircle * RangoDeAparicion;
        Vector3 pos = transform.position + new Vector3(offset.x, offset.y, 0f);
        Instantiate(Prefab, pos, Quaternion.identity);
    }
}
