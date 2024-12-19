using UnityEditor;

[CustomEditor(typeof(AudioReactive_Transform), true)]
public class AudioSettingsTransformEditor : AudioSettingsEditor
{
    protected SerializedProperty targetTransformProp;

    protected SerializedProperty defaultScaleProp;
    protected SerializedProperty rangeXProp;
    protected SerializedProperty rangeYProp;
    protected SerializedProperty rangeZProp;

    protected override void OnEnable()
    {
        base.OnEnable();

        targetTransformProp = serializedObject.FindProperty("targetTransform");

        defaultScaleProp = serializedObject.FindProperty("defaultScale");
        rangeXProp = serializedObject.FindProperty("rangeX");
        rangeYProp = serializedObject.FindProperty("rangeY");
        rangeZProp = serializedObject.FindProperty("rangeZ");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        base.OnInspectorGUI();

        // Get the DataTarget value from the targetDataProp
        AudioReactive_Transform.TargetTransform targetTransform = (AudioReactive_Transform.TargetTransform)targetTransformProp.enumValueIndex;

        // Draw the 'targetData' enum field
        EditorGUILayout.PropertyField(targetTransformProp);

        // If the targetData is 'rawSamples', show the dataResolution field
        if (targetTransform == AudioReactive_Transform.TargetTransform.scale)
        {
            EditorGUILayout.PropertyField(defaultScaleProp);
            EditorGUILayout.PropertyField(rangeXProp);
            EditorGUILayout.PropertyField(rangeYProp);
            EditorGUILayout.PropertyField(rangeZProp);
        }


        // Apply changes to the serialized object
        serializedObject.ApplyModifiedProperties();
    }
}
