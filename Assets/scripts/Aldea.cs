using UnityEngine;

public class Aldea : MonoBehaviour
{
    [Header("Aldea")]
    public float storedWood = 0f;
    public float woodConsumptionPerSecond = 0.25f;
    public float villageRadius = 3f;
    public bool safeZoneActive = true;

    public void Simulate(float h)
    {
        if (h <= 0f) return;

        storedWood -= woodConsumptionPerSecond * h;

        if (storedWood <= 0f)
        {
            storedWood = 0f;
            safeZoneActive = false;
        }
        else
        {
            safeZoneActive = true;
        }
    }

    public void DepositWood(float amount)
    {
        if (amount <= 0f) return;

        storedWood += amount;
        safeZoneActive = storedWood > 0f;
    }

    public Vector3 GetRandomPointInsideVillage()
    {
        Vector2 offset = Random.insideUnitCircle * villageRadius;
        return transform.position + new Vector3(offset.x, offset.y, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, villageRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
