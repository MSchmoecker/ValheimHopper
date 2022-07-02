using System;
using UnityEditor;

[CustomEditor(typeof(Container))]
public class ChestInspector : Editor {
    private void Awake() {
        EditorApplication.update += Repaint;
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        EditorGUILayout.Separator();
        Container container = (Container)target;
        Inventory inventory = container.GetInventory();

        if (inventory == null) {
            EditorGUILayout.LabelField("Inventory is null");
            return;
        }

        for (int y = 0; y < inventory.GetHeight(); y++) {
            for (int x = 0; x < inventory.GetWidth(); x++) {
                ItemDrop.ItemData item = inventory.GetItemAt(x, y);

                EditorGUILayout.LabelField($"Item ({x}, {y}):");
                EditorGUI.BeginDisabledGroup(true);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical();

                if (item != null) {
                    EditorGUILayout.TextField("Name", item.m_shared.m_name);
                    EditorGUILayout.IntField("Stack", item.m_stack);
                    EditorGUILayout.IntField("Max Stack", item.m_shared.m_maxStackSize);
                } else {
                    EditorGUILayout.LabelField($"Null");
                }


                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();
                // EditorGUILayout.ObjectField(item, typeof(ItemDrop.ItemData), false)
            }
        }
    }
}
