using IgorTime.PoolableScrollView.Scrolls;
using UnityEditor;
using UnityEditor.UI;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(BasePoolableScrollView), true)]
public class PoolableScrollViewInspector : ScrollRectEditor
{
    // public override void OnInspectorGUI()
    // {
    //     EditorGUILayout.PropertyField(serializedObject.FindProperty("itemViewProvider"));
    //     EditorGUILayout.PropertyField(serializedObject.FindProperty("interactable"));
    //     EditorGUILayout.Space();
    //     base.OnInspectorGUI();
    // }

    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        root.Add(new PropertyField(serializedObject.FindProperty("itemViewProvider")));
        root.Add(new PropertyField(serializedObject.FindProperty("interactable")));
        root.Add(new IMGUIContainer(() =>
        {
            EditorGUILayout.Space();
            base.OnInspectorGUI();
        }));

        return root;
    }
}