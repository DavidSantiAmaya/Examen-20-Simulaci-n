using UnityEngine;

public class Lodo : MonoBehaviour
{
    [Header("Lobo")]
    public float energia = 15f;
    public float maxEnergia = 15f;
    public float age = 0f;
    public float maxAge = 30f;
    public float speed = 1.5f;
    public float rangoVision = 6f;
    private float rangoAtaque = 0.35f;

    [Header("Descanso")]
    public float Recuperacion = 3f;
    public float topeDeEnergia = 10f;

    [Header("Estado")]
    public bool isAlive = true;
    public LoboStates estadoActual = LoboStates.Patrullando;

    private Vector3 destinp;
    private float h;
    private Aldea aldea;
    private int aldeanoMask;

    private void Start()
    {
        destinp = transform.position;
        aldea = BuscarAldea();
        aldeanoMask = LayerMask.GetMask("Aldeanos");
    }

    public void Simulate(float h)
    {
        if (!isAlive) return;

        this.h = h;

        if (aldea == null)
            aldea = BuscarAldea();

        EvaluarEstado();
        EjecutarEstado();
        Mover();
        Envejecer();
        RevisarEstadoFinal();
    }

    private void EjecutarEstado()
    {
        switch (estadoActual)
        {
            case LoboStates.Patrullando:
                Patrullar();
                break;

            case LoboStates.Cazando:
                Cazar();
                break;

            case LoboStates.Descansando:
                Descansar();
                break;
        }
    }

    private void EvaluarEstado()
    {
        if (energia <= 0f)
        {
            estadoActual = LoboStates.Descansando;
            return;
        }

        if (estadoActual == LoboStates.Descansando && energia < topeDeEnergia)
            return;

        if (estadoActual == LoboStates.Descansando && energia >= topeDeEnergia)
            estadoActual = LoboStates.Patrullando;

        if (aldea != null && aldea.ActivarZona)
        {
            float distanciaAldea = Vector3.Distance(transform.position, aldea.transform.position);

            if (distanciaAldea < aldea.RangoAldea)
            {
                Vector3 alejarse = (transform.position - aldea.transform.position).normalized;
                if (alejarse == Vector3.zero) alejarse = Vector3.right;

                destinp = transform.position + alejarse * rangoVision;
                estadoActual = LoboStates.Patrullando;
                return;
            }
        }

        Aldeano aldeanoCercano = BuscarAldeanoMasCercano();
        if (aldeanoCercano != null)
        {
            estadoActual = LoboStates.Cazando;
            destinp = aldeanoCercano.transform.position;
            return;
        }

        if (estadoActual != LoboStates.Descansando)
            estadoActual = LoboStates.Patrullando;
    }

    private void Patrullar()
    {
        if (Vector3.Distance(transform.position, destinp) < 0.15f)
        {
            Vector2 direccion = Random.insideUnitCircle.normalized;
            if (direccion == Vector2.zero) direccion = Vector2.right;

            destinp = transform.position + new Vector3(direccion.x, direccion.y, 0f) * rangoVision;
        }
    }

    private void Cazar()
    {
        Aldeano objetivo = BuscarAldeanoMasCercano();

        if (objetivo == null)
        {
            estadoActual = LoboStates.Patrullando;
            return;
        }

        if (aldea != null && aldea.ActivarZona)
        {
            float distanciaAlCentro = Vector3.Distance(objetivo.transform.position, aldea.transform.position);
            if (distanciaAlCentro < aldea.RangoAldea)
            {
                estadoActual = LoboStates.Patrullando;
                destinp = transform.position;
                return;
            }
        }

        destinp = objetivo.transform.position;

        if (Vector3.Distance(transform.position, objetivo.transform.position) <= attackRange)
        {
            objetivo.Morir();
            energia = Mathf.Min(energia + 5f, maxEnergia);
            estadoActual = LoboStates.Patrullando;
        }
    }

    private void Descansar()
    {
        energia += Recuperacion * h;

        if (energia >= topeDeEnergia)
        {
            energia = Mathf.Min(energia, maxEnergia);
            estadoActual = LoboStates.Patrullando;
        }
    }

    private void Mover()
    {
        if (estadoActual == LoboStates.Descansando)
            return;

        transform.position = Vector3.MoveTowards(transform.position, destinp, speed * h);
        energia -= speed * 0.2f * h;

        if (energia <= 0f)
        {
            energia = 0f;
            estadoActual = LoboStates.Descansando;
        }
    }

    private void Envejecer()
    {
        age += h;
    }

    private void RevisarEstadoFinal()
    {
        if (age > maxAge)
        {
            isAlive = false;
            estadoActual = LoboStates.Muerto;
            Destroy(gameObject);
        }
    }

    private Aldeano BuscarAldeanoMasCercano()
    {
        if (aldeanoMask == 0)
            return null;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            rangoVision,
            aldeanoMask
        );

        Aldeano masCercano = null;
        float distanciaMinima = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            Aldeano a = hit.GetComponentInParent<Aldeano>();
            if (a == null || !a.isAlive) continue;

            if (aldea != null && aldea.ActivarZona)
            {
                float distanciaAlCentro = Vector3.Distance(a.transform.position, aldea.transform.position);
                if (distanciaAlCentro < aldea.RangoAldea)
                    continue;
            }

            float dist = Vector3.Distance(transform.position, a.transform.position);
            if (dist < distanciaMinima)
            {
                distanciaMinima = dist;
                masCercano = a;
            }
        }

        return masCercano;
    }

    private Aldea BuscarAldea()
    {
        Aldea[] aldeas = FindObjectsByType<Aldea>(FindObjectsSortMode.InstanceID);

        int layerAldea = LayerMask.NameToLayer("Aldea");
        if (layerAldea == -1)
        {
            return null;
        }

        foreach (Aldea a in aldeas)
        {
            if (a != null && a.gameObject.layer == layerAldea)
                return a;
        }

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, rangoVision);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(destinp, 0.2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, destinp);
    }
}