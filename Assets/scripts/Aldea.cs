using UnityEngine;

public class Aldea : MonoBehaviour
{
    [Header("Aldea")]
    public float maderaAlmacenada = 0f;
    public float ConsumoMaderaPorSegundp = 0.25f;
    public float RangoAldea = 3f;
    public bool ActivarZona = true;

    public void Simulate(float h)
    {
        if (h <= 0f) return;

        maderaAlmacenada -= ConsumoMaderaPorSegundp * h;

        if (maderaAlmacenada <= 0f)
        {
            maderaAlmacenada = 0f;
            ActivarZona = false;
        }
        else
        {
            ActivarZona = true;
        }
    }

    public void DepositoMadera(float amount)
    {
        if (amount <= 0f) return;

        maderaAlmacenada += amount;
        ActivarZona = maderaAlmacenada > 0f;
    }

    public Vector3 ObtenerPuntoAleatorioAldea()
    {
        Vector2 offset = Random.insideUnitCircle * RangoAldea;
        return transform.position + new Vector3(offset.x, offset.y, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, RangoAldea);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
