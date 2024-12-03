using UnityEngine;

public class InstantiateCube : MonoBehaviour
{
    [SerializeField] AudioAnalyzer audioAnalyzer;
    [SerializeField] GameObject cubePrefab;

    private GameObject[] sampleCubes;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateCubes();
    }

    void GenerateCubes()
    {
        int bandCount = audioAnalyzer.FrequencyBands.Length;
        float rotStep = 360f / bandCount;

        sampleCubes = new GameObject[bandCount];

        for (int i = 0; i < sampleCubes.Length; i++)
        {
            // Create a rotation for this spawn point (rotating around the Y-axis)
            Quaternion spawnRotation = Quaternion.Euler(0, rotStep * i, 0);

            // Rotate the forward vector (Z direction) by the rotation
            Vector3 forwardDirection = spawnRotation * Vector3.forward;

            // Scale the direction to the desired distance
            Vector3 spawnPoint = forwardDirection * 10;


            GameObject obj = Instantiate(cubePrefab, spawnPoint, spawnRotation);
            obj.name = "SampleCube_" + i;

            sampleCubes[i] = obj;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if there are more frequency bands then cubes
        if (sampleCubes.Length != audioAnalyzer.FrequencyBands.Length)
        {
            foreach (GameObject obj in sampleCubes) Destroy(obj);
            GenerateCubes();
        }

        for(int i = 0; i < sampleCubes.Length; i++)
        {
            sampleCubes[i].transform.localScale = new(1f, (audioAnalyzer.FrequencyBands[i] * 1000) + 1, 1f); 
        }
    }
}
