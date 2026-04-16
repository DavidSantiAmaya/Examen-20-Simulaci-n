using UnityEngine;

public class Lodo : MonoBehaviour
{
    [Header("Lobo")]
    public float energy = 15f;
    public float maxEnergy = 15f;
    public float age = 0f;
    public float maxAge = 30f;
    public float speed = 1.5f;
    public float visionRange = 6f;
    public float attackRange = 0.35f;

    [Header("Descanso")]
    public float restRecoveryRate = 3f;
    public float energyToWakeUp = 10f;

    [Header("Depuración")]
    public bool debugWolf = true;

    [Header("Estado")]
    public bool isAlive = true;
    public LoboStates currentState = LoboStates.Patrullando;

    private Vector3 destination;
    private float h;
    private Aldea aldea;
    private int aldeanoMask;

    private void Start()
    {
        destination = transform.position;
        aldea = BuscarAldea();
        aldeanoMask = LayerMask.GetMask("Aldeanos");

        if (aldeanoMask == 0 && debugWolf)
            Debug.LogError($"[{name}] No existe la layer 'Aldeanos'.");
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
        switch (currentState)
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
        if (energy <= 0f)
        {
            currentState = LoboStates.Descansando;
            return;
        }

        if (currentState == LoboStates.Descansando && energy < energyToWakeUp)
            return;

        if (currentState == LoboStates.Descansando && energy >= energyToWakeUp)
            currentState = LoboStates.Patrullando;

        if (aldea != null && aldea.safeZoneActive)
        {
            float distanciaAldea = Vector3.Distance(transform.position, aldea.transform.position);

            if (distanciaAldea < aldea.villageRadius)
            {
                Vector3 alejarse = (transform.position - aldea.transform.position).normalized;
                if (alejarse == Vector3.zero) alejarse = Vector3.right;

                destination = transform.position + alejarse * visionRange;
                currentState = LoboStates.Patrullando;
                return;
            }
        }

        Aldeano aldeanoCercano = BuscarAldeanoMasCercano();
        if (aldeanoCercano != null)
        {
            currentState = LoboStates.Cazando;
            destination = aldeanoCercano.transform.position;
            return;
        }

        if (currentState != LoboStates.Descansando)
            currentState = LoboStates.Patrullando;
    }

    private void Patrullar()
    {
        if (Vector3.Distance(transform.position, destination) < 0.15f)
        {
            Vector2 direccion = Random.insideUnitCircle.normalized;
            if (direccion == Vector2.zero) direccion = Vector2.right;

            destination = transform.position + new Vector3(direccion.x, direccion.y, 0f) * visionRange;
        }
    }

    private void Cazar()
    {
        Aldeano objetivo = BuscarAldeanoMasCercano();

        if (objetivo == null)
        {
            currentState = LoboStates.Patrullando;
            return;
        }

        if (aldea != null && aldea.safeZoneActive)
        {
            float distanciaAlCentro = Vector3.Distance(objetivo.transform.position, aldea.transform.position);
            if (distanciaAlCentro < aldea.villageRadius)
            {
                currentState = LoboStates.Patrullando;
                destination = transform.position;
                return;
            }
        }

        destination = objetivo.transform.position;

        if (Vector3.Distance(transform.position, objetivo.transform.position) <= attackRange)
        {
            objetivo.Morir();
            energy = Mathf.Min(energy + 5f, maxEnergy);
            currentState = LoboStates.Patrullando;
        }
    }

    private void Descansar()
    {
        energy += restRecoveryRate * h;

        if (energy >= energyToWakeUp)
        {
            energy = Mathf.Min(energy, maxEnergy);
            currentState = LoboStates.Patrullando;
        }
    }

    private void Mover()
    {
        if (currentState == LoboStates.Descansando)
            return;

        transform.position = Vector3.MoveTowards(transform.position, destination, speed * h);
        energy -= speed * 0.2f * h;

        if (energy <= 0f)
        {
            energy = 0f;
            currentState = LoboStates.Descansando;
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
            currentState = LoboStates.Muerto;
            Destroy(gameObject);
        }
    }

    private Aldeano BuscarAldeanoMasCercano()
    {
        if (aldeanoMask == 0)
            return null;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            visionRange,
            aldeanoMask
        );

        Aldeano masCercano = null;
        float distanciaMinima = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            Aldeano a = hit.GetComponentInParent<Aldeano>();
            if (a == null || !a.isAlive) continue;

            if (aldea != null && aldea.safeZoneActive)
            {
                float distanciaAlCentro = Vector3.Distance(a.transform.position, aldea.transform.position);
                if (distanciaAlCentro < aldea.villageRadius)
                    continue;
            }

            float dist = Vector3.Distance(transform.position, a.transform.position);
            if (dist < distanciaMinima)
            {
                distanciaMinima = dist;
                masCercano = a;
            }
        }

        if (debugWolf)
            Debug.Log($"[{name}] Aldeanos detectados en rango: {(masCercano != null ? 1 : 0)}");

        return masCercano;
    }

    private Aldea BuscarAldea()
    {
        Aldea[] aldeas = FindObjectsByType<Aldea>(FindObjectsSortMode.InstanceID);

        int layerAldea = LayerMask.NameToLayer("Aldea");
        if (layerAldea == -1)
        {
            if (debugWolf)
                Debug.LogError($"[{name}] No existe la layer 'Aldea'.");
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
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(destination, 0.2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, destination);
    }
}