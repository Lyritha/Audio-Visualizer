using UnityEditor;
using UnityEngine;

public class AudioReactive_Graph_Circle : AudioReactiveComponent
{
    [Header("Visuals")]
    [SerializeField] private Material material;
    [SerializeField] private PrimitiveType shape;

    [Header("Reactive Parameters")]
    [SerializeField] private Vector3 graphObjectScale = Vector3.one;
    [SerializeField] private float radius = 10;
    [SerializeField] private float maxHeight = 5;
    [SerializeField] private float reactionStrength = 1000;


    private GameObject[] graphObjects;

    // put any audio reactive effects in this, no need to worry about execution order
    protected override void AudioReaction()
    {
        // if values don't match up, regenerate the circle
        if (graphObjects.Length != audioData.Length)
        {
            foreach (GameObject obj in graphObjects) Destroy(obj);
            GenerateCubes();
        }

        // apply the effect
        for (int i = 0; i < graphObjects.Length; i++)
        {
            float value = audioData[i] * reactionStrength;
            graphObjects[i].transform.localScale = graphObjectScale + (Vector3.up * Mathf.Clamp(value, 0, maxHeight));
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() => GenerateCubes();

    void GenerateCubes()
    {
        int count = audioData.Length;
        float rotStep = 360f / count;

        graphObjects = new GameObject[count];

        for (int i = 0; i < graphObjects.Length; i++)
        {
            // Create a rotation for this spawn point (rotating around the Y-axis)
            Quaternion spawnRotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y + (rotStep * i), transform.rotation.z);

            // Scale the direction to the desired distance
            Vector3 spawnPoint = spawnRotation * Vector3.forward * radius;

            graphObjects[i] = CreateObject(i, spawnPoint, spawnRotation);
        }
    }

    private GameObject CreateObject(int index, Vector3 pos, Quaternion rot)
    {
        // create primitive cube
        GameObject obj = GameObject.CreatePrimitive(shape);

        // set transform information
        obj.transform.SetPositionAndRotation(pos, rot);
        obj.transform.parent = transform;

        // assign the material
        obj.GetComponent<Renderer>().material = material;

        // remove collider, as it won't be needed
        Destroy(obj.GetComponent<Collider>());

        // give it a name
        obj.name = "SampleCube_" + index;

        //return the cube
        return obj;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Create a slider for adjusting the radius
        Handles.color = material.GetColor("_EmissionColor");
        Handles.DrawWireArc(transform.position, transform.up, transform.forward, 360, radius);
    }
#endif
}