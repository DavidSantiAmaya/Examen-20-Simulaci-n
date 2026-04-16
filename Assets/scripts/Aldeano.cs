using UnityEngine;

public class Aldeano : MonoBehaviour
{
    [Header("Aldeano")]
    public float energy = 20f;
    public float age = 0f;
    public float maxAge = 40f;
    public float speed = 1f;
    public float visionRange = 6f;

    [Header("Madera")]
    public float carryingWood = 0f;
    public float minWoodToLeaveVillage = 25f;

    private float villageStopDistance = 0.2f;
    private float treeHarvestDistance = 0.45f;


    [Header("Estado")]
    public bool isAlive = true;
    public AldeanoStates currentState = AldeanoStates.EnAldea;

    private Vector3 destination;
    private float h;
    private Aldea aldea;
    private Arbol targetTree;

    private bool wolfDetectedThisFrame = false;
    private bool lastWolfDetected = false;

    private int wolfMask;
    private int treeMask;
    internal bool estaVivo;

    private void Start()
    {
        wolfMask = LayerMask.GetMask("Lobos");
        treeMask = LayerMask.GetMask("Arboles");
        aldea = BuscarAldeaPorLayer();
        destination = aldea != null ? aldea.GetRandomPointInsideVillage() : transform.position;
    }

    public void Simulate(float h)
    {
        if (!isAlive) return;

        this.h = h;

        if (aldea == null)
            aldea = BuscarAldeaPorLayer();

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
            case AldeanoStates.EnAldea:
                ExplorarAldea();
                break;

            case AldeanoStates.YendoAlBosque:
                IrAlArbol();
                break;

            case AldeanoStates.Recolectando:
                RecolectarMadera();
                break;

            case AldeanoStates.Regresando:
                RegresarALaAldea();
                break;

            case AldeanoStates.Huyendo:
                HuirALCentroDeLaAldea();
                break;
        }
    }

    private Aldea BuscarAldeaPorLayer()
    {
        Aldea[] aldeas = FindObjectsByType<Aldea>(FindObjectsSortMode.InstanceID);
        int layerAldea = LayerMask.NameToLayer("Aldea");

        foreach (Aldea a in aldeas)
        {
            if (a != null && a.gameObject.layer == layerAldea)
                return a;
        }

        return null;
    }

    private void EvaluarEstado()
    {
        if (RevisarLoboYHuir())
            return;

        if (carryingWood > 0f)
        {
            currentState = AldeanoStates.Regresando;
            IrAlCentroDeLaAldea();
            return;
        }

        if (aldea != null && aldea.storedWood < minWoodToLeaveVillage)
        {
            if (targetTree == null || !targetTree.isAlive)
                targetTree = BuscarArbolMasCercano();

            if (targetTree != null)
            {
                float dist = Vector3.Distance(transform.position, targetTree.transform.position);

                if (dist <= treeHarvestDistance)
                {
                    currentState = AldeanoStates.Recolectando;
                }
                else
                {
                    currentState = AldeanoStates.YendoAlBosque;
                    destination = targetTree.transform.position;
                }

                return;
            }
        }

        currentState = AldeanoStates.EnAldea;
    }

    private bool RevisarLoboYHuir()
    {
        bool hayLobo = HayLoboCerca();
        wolfDetectedThisFrame = hayLobo;

        if (hayLobo != lastWolfDetected)
        {
            lastWolfDetected = hayLobo;
        }

        if (hayLobo)
        {
            currentState = AldeanoStates.Huyendo;
            IrAlCentroDeLaAldea();
            return true;
        }

        return false;
    }

    private void IrAlCentroDeLaAldea()
    {
        if (aldea == null) return;
        destination = aldea.transform.position;
    }

    private void ExplorarAldea()
    {
        if (aldea == null) return;

        if (Vector3.Distance(transform.position, destination) <= villageStopDistance)
            destination = aldea.GetRandomPointInsideVillage();
    }

    private void IrAlArbol()
    {
        if (targetTree == null || !targetTree.isAlive)
            targetTree = BuscarArbolMasCercano();

        if (targetTree == null)
        {
            currentState = AldeanoStates.EnAldea;
            destination = aldea != null ? aldea.GetRandomPointInsideVillage() : transform.position;
            return;
        }

        destination = targetTree.transform.position;

        float dist = Vector3.Distance(transform.position, targetTree.transform.position);
        if (dist <= treeHarvestDistance)
            currentState = AldeanoStates.Recolectando;
    }

    private void RecolectarMadera()
    {
        if (targetTree == null || !targetTree.isAlive)
        {
            VolverAEstadoAldea();
            return;
        }

        float dist = Vector3.Distance(transform.position, targetTree.transform.position);
        if (dist > treeHarvestDistance)
        {
            currentState = AldeanoStates.YendoAlBosque;
            destination = targetTree.transform.position;
            return;
        }

        float woodObtained = targetTree.Harvest();

        if (woodObtained <= 0f)
        {
            VolverAEstadoAldea();
            return;
        }

        carryingWood += woodObtained;
        targetTree = null;

        if (aldea != null)
        {
            currentState = AldeanoStates.Regresando;
            destination = aldea.transform.position;
        }
    }

    private void RegresarALaAldea()
    {
        if (aldea == null) return;

        IrAlCentroDeLaAldea();

        if (Vector3.Distance(transform.position, aldea.transform.position) <= 0.6f)
        {
            aldea.DepositWood(carryingWood);
            carryingWood = 0f;
            currentState = AldeanoStates.EnAldea;
            destination = aldea.GetRandomPointInsideVillage();
        }
    }

    private void HuirALCentroDeLaAldea()
    {
        if (aldea == null) return;

        IrAlCentroDeLaAldea();

        if (!wolfDetectedThisFrame && Vector3.Distance(transform.position, aldea.transform.position) <= 0.8f)
        {
            currentState = AldeanoStates.EnAldea;
            destination = aldea.GetRandomPointInsideVillage();
        }
    }

    private void VolverAEstadoAldea()
    {
        currentState = AldeanoStates.EnAldea;
        targetTree = null;
        destination = aldea != null ? aldea.GetRandomPointInsideVillage() : transform.position;
    }

    private void Mover()
    {
        if (currentState == AldeanoStates.Recolectando)
            return;

        float moveSpeed = speed;

        if (currentState == AldeanoStates.Huyendo)
            moveSpeed *= 1.2f;

        if (currentState == AldeanoStates.EnAldea)
            moveSpeed *= 0.45f;

        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            moveSpeed * h
        );

        energy -= speed * 0.2f * h;
    }

    private void Envejecer()
    {
        age += h;
    }

    private void RevisarEstadoFinal()
    {
        if (age > maxAge || energy <= 0f)
            Morir();
    }

    public void Morir()
    {
        if (!isAlive) return;

        isAlive = false;
        currentState = AldeanoStates.Muerto;
        Destroy(gameObject);
    }

    private bool HayLoboCerca()
    {
        if (wolfMask == 0)
            return false;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            visionRange,
            wolfMask
        );

        return hits.Length > 0;
    }

    private Arbol BuscarArbolMasCercano()
    {
        if (treeMask == 0)
            return null;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            visionRange,
            treeMask
        );

        Arbol arbolMasCercano = null;
        float distanciaMinima = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            Arbol arbol = hit.GetComponentInParent<Arbol>();
            if (arbol == null || !arbol.isAlive) continue;

            float dist = Vector3.Distance(transform.position, arbol.transform.position);

            if (dist < distanciaMinima)
            {
                distanciaMinima = dist;
                arbolMasCercano = arbol;
            }
        }

        return arbolMasCercano;
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