using UnityEngine;

public class AudioReactive_Graph_Circle : AudioReactiveComponent
{
    [Header("Circle Data")]
    [SerializeField] private GameObject graphObjectPrefab;
    [SerializeField] private Vector3 graphObjectScale = Vector3.one;
    [SerializeField] private float cubeDistance = 10;
    [SerializeField] private float maxHeight = 5;
    [SerializeField] private float rotationOffset = 0;
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
            Quaternion spawnRotation = Quaternion.Euler(0, (rotStep * i) + rotationOffset, 0);

            // Rotate the forward vector (Z direction) by the rotation
            Vector3 forwardDirection = spawnRotation * Vector3.forward;

            // Scale the direction to the desired distance
            Vector3 spawnPoint = forwardDirection * cubeDistance;


            GameObject obj = Instantiate(graphObjectPrefab, spawnPoint, spawnRotation);
            obj.name = "SampleCube_" + i;
            obj.transform.parent = transform;

            graphObjects[i] = obj;
        }
    }
}