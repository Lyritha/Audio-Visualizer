using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AudioReactiveComponent), true)]
public class AudioSettingsEditor : Editor
{
    // Reference to the target component
    private SerializedProperty targetDataProp;
    private SerializedProperty dataResolutionProp;

    private void OnEnable()
    {
        // Cache references to the properties using the exact field names
        targetDataProp = serializedObject.FindProperty("targetData");
        dataResolutionProp = serializedObject.FindProperty("dataResolution");
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
            EditorGUILayout.PropertyField(dataResolutionProp);
        }

        // Apply changes to the serialized object
        serializedObject.ApplyModifiedProperties();
    }
}
