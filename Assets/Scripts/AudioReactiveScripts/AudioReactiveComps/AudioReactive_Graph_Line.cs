using UnityEditor;
using UnityEngine;

public class AudioReactive_Graph_Line : AudioReactiveComponent
{
    [Header("Visuals")]
    [SerializeField] private Material material;
    [SerializeField] private PrimitiveType shape;
    [SerializeField] private bool customShapeBool;
    [SerializeField] private GameObject customShape;

    [Header("Reactive Parameters")]
    [SerializeField] private Vector3 graphObjectScale = Vector3.one;
    [SerializeField] private float length = 10;
    [SerializeField] private float maxHeight = 5;
    [SerializeField] private float reactionStrength = 1000;


    private GameObject[] graphObjects;

    protected override void OnValidate()
    {
        if (audioAnalyzer == null) audioAnalyzer = FindFirstObjectByType<AudioAnalyser>();
        base.OnValidate();
    }

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
        float step = length / count;

        graphObjects = new GameObject[count];

        for (int i = 0; i < graphObjects.Length; i++)
        {
            float offsetStart = -(length / 2);



            // Convert local spawn point to world space
            Vector3 spawnPoint = new((transform.position.x + offsetStart) + (step * i), transform.position.y, transform.position.z) ;

            graphObjects[i] = CreateObject(i, spawnPoint);
        }
    }

    private GameObject CreateObject(int index, Vector3 pos)
    {
        // create primitive cube
        GameObject obj = customShapeBool ? Instantiate(customShape) : GameObject.CreatePrimitive(shape);

        // set transform information
        obj.transform.position = pos;
        obj.transform.parent = transform;
        obj.layer = gameObject.layer;

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
        Handles.DrawLine(transform.position - new Vector3(length / 2, 0,0), transform.position + new Vector3(length / 2,0,0));
    }
#endif
}