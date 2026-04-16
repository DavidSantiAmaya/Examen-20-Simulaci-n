using System.Collections.Generic;
using UnityEngine;

public class Simulate : MonoBehaviour
{
    public float secondsPerIteration = 1f;
    private float time = 0f;

    public List<Aldeano> aldeanos = new List<Aldeano>();
    public List<Lodo> lodos = new List<Lodo>();

    public Aldea aldea;
    public Bosque bosque;


    private void Start()
    {
        RefreshReferences();
    }

    private void Update()
    {
            time += Time.deltaTime;

            if (time >= secondsPerIteration)
            {
                time = 0f;
                SimulateStep(secondsPerIteration);
            }
    }

    private void RefreshReferences()
    {
        aldeanos = new List<Aldeano>(FindObjectsByType<Aldeano>(FindObjectsSortMode.InstanceID));
        lodos = new List<Lodo>(FindObjectsByType<Lodo>(FindObjectsSortMode.InstanceID));

        aldea = FindFirstObjectByType<Aldea>();
        bosque = FindFirstObjectByType<Bosque>();
    }

    private void SimulateStep(float step)
    {
        foreach (Lodo lobo in lodos)
            if (lobo != null && lobo.isAlive)
                lobo.Simulate(step);

        if (aldea != null)
            aldea.Simulate(step);

        if (bosque != null)
            bosque.Simulate(step);

        foreach (Aldeano aldeano in aldeanos)
            if (aldeano != null && aldeano.isAlive)
                aldeano.Simulate(step);
    }
}