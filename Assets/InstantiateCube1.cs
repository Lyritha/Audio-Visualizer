using UnityEngine;
using UnityEngine.Rendering.Universal;

public class InstantiateCubeSamples : MonoBehaviour
{
    [SerializeField] AudioAnalyzer audioAnalyzer;
    [SerializeField] GameObject cubePrefab;
    [SerializeField] float cubeDistance = 10;
    [SerializeField] float maxHeight = 5;
    [SerializeField] float rotOffset = 0;
    [SerializeField] int resolutionReduction = 2;
    [SerializeField] Vector3 cubeSize = Vector3.one;

    private GameObject[] sampleCubes;

    //enforce values in the inspector
    private void OnValidate()
    {

        // clamp frequency band to even numbers, to make sample assignment easier
        resolutionReduction += resolutionReduction % 2 != 0 ? 1 : 0;
        resolutionReduction = Mathf.Clamp(resolutionReduction, 1, 64);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateCubes();
    }

    void GenerateCubes()
    {
        int bandCount = audioAnalyzer.Samples.Length / resolutionReduction;
        float rotStep = 360f / bandCount;

        sampleCubes = new GameObject[bandCount];

        for (int i = 0; i < sampleCubes.Length; i++)
        {
            // Create a rotation for this spawn point (rotating around the Y-axis)
            Quaternion spawnRotation = Quaternion.Euler(0, (rotStep * i) + rotOffset, 0);

            // Rotate the forward vector (Z direction) by the rotation
            Vector3 forwardDirection = spawnRotation * Vector3.forward;

            // Scale the direction to the desired distance
            Vector3 spawnPoint = forwardDirection * cubeDistance;


            GameObject obj = Instantiate(cubePrefab, spawnPoint, spawnRotation);
            obj.name = "SampleCube_" + i;
            obj.transform.parent = transform;

            sampleCubes[i] = obj;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if there are more frequency bands then cubes
        if (sampleCubes.Length != audioAnalyzer.Samples.Length / resolutionReduction)
        {
            foreach (GameObject obj in sampleCubes) Destroy(obj);
            GenerateCubes();
        }

        for(int i = 0; i < sampleCubes.Length; i++)
        {
            float value = audioAnalyzer.Samples[i * resolutionReduction] * 1000;

            sampleCubes[i].transform.localScale = new(cubeSize.x, Mathf.Clamp(value + cubeSize.y, 0, maxHeight), cubeSize.z); 
        }
    }
}
