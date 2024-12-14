using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AudioReactiveComponent), true)]
public class AudioSettingsEditor : Editor
{
    // Reference to the target component
    private SerializedProperty targetDataProp;
    private SerializedProperty sampleResolutionProp;
    private SerializedProperty sampleRangeProp;

    // Reference to the actual script instance
    private AudioReactiveComponent reactiveComponent;

    private void OnEnable()
    {
        // Cache references to the properties using the exact field names
        targetDataProp = serializedObject.FindProperty("targetData");
        sampleResolutionProp = serializedObject.FindProperty("sampleResolution");
        sampleRangeProp = serializedObject.FindProperty("sampleRange");

        // Get the actual script reference
        reactiveComponent = (AudioReactiveComponent)target;
    }

    public override void OnInspectorGUI()
    {
        // Update the serialized object to reflect the latest data
        serializedObject.Update();

        // Draw default fields (includes fields defined in the base class AudioReactiveComponent)
        base.OnInspectorGUI();

        // Get the DataTarget value from the targetDataProp
        AudioReactiveComponent.DataTarget targetData = (AudioReactiveComponent.DataTarget)targetDataProp.enumValueIndex;

        // Draw the 'targetData' enum field
        EditorGUILayout.PropertyField(targetDataProp);

        // If the targetData is 'rawSamples', show the dataResolution field
        if (targetData == AudioReactiveComponent.DataTarget.rawSamples)
        {
            EditorGUILayout.PropertyField(sampleResolutionProp);
            MinMaxSlider();
        }



        // Apply changes to the serialized object
        serializedObject.ApplyModifiedProperties();
    }

    private void MinMaxSlider()
    {
        // Add a slider for sampleRange
        Vector2Int sampleRange = sampleRangeProp.vector2IntValue; // Get the current value
        float min = sampleRange.x;
        float max = sampleRange.y;

        // vector2Int range slider
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Sample Range", GUILayout.ExpandWidth(true));
        min = EditorGUILayout.FloatField(min, GUILayout.Width(50));
        float value = reactiveComponent.AudioAnalyzer != null ? reactiveComponent.AudioAnalyzer.SampleCount : 0;
        EditorGUILayout.MinMaxSlider(ref min, ref max, 0f, value, GUILayout.ExpandWidth(true));
        max = EditorGUILayout.FloatField(max, GUILayout.Width(50));
        EditorGUILayout.EndHorizontal();

        // Update the property with integer values
        sampleRangeProp.vector2IntValue = new Vector2Int(Mathf.RoundToInt(min), Mathf.RoundToInt(max));
    }
}
