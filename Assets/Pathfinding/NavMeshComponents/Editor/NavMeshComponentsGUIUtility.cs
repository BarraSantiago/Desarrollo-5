using UnityEditor;
using UnityEditor.AI;
using UnityEngine;
using UnityEngine.AI;

namespace NavMeshPlus.Editors.Components
{
    public static class NavMeshComponentsGUIUtility
    {
        public static void AreaPopup(Rect rect, string labelName, SerializedProperty areaProperty)
        {
            int areaIndex = -1;
            string[] areaNames = GameObjectUtility.GetNavMeshAreaNames();
            for (int i = 0; i < areaNames.Length; i++)
            {
                int areaValue = GameObjectUtility.GetNavMeshAreaFromName(areaNames[i]);
                if (areaValue == areaProperty.intValue)
                    areaIndex = i;
            }
            ArrayUtility.Add(ref areaNames, "");
            ArrayUtility.Add(ref areaNames, "Open Area Settings...");

            EditorGUI.BeginProperty(rect, GUIContent.none, areaProperty);

            EditorGUI.BeginChangeCheck();
            areaIndex = EditorGUI.Popup(rect, labelName, areaIndex, areaNames);

            if (EditorGUI.EndChangeCheck())
            {
                if (areaIndex >= 0 && areaIndex < areaNames.Length - 2)
                    areaProperty.intValue = GameObjectUtility.GetNavMeshAreaFromName(areaNames[areaIndex]);
                else if (areaIndex == areaNames.Length - 1)
                    NavMeshEditorHelpers.OpenAreaSettings();
            }

            EditorGUI.EndProperty();
        }

        public static bool IsAgentSelectionValid(SerializedProperty agentTypeID)
        {
            int count = NavMesh.GetSettingsCount();
            for (int i = 0; i < count; i++)
            {
                int id = NavMesh.GetSettingsByIndex(i).agentTypeID;
                string name = NavMesh.GetSettingsNameFromID(id);
                if (id == agentTypeID.intValue)
                    return true;
            }
            return false;
        }

        public static void AgentTypePopup(Rect rect, string labelName, SerializedProperty agentTypeID)
        {
            int index = -1;
            int count = NavMesh.GetSettingsCount();
            string[] agentTypeNames = new string[count + 2];
            for (int i = 0; i < count; i++)
            {
                int id = NavMesh.GetSettingsByIndex(i).agentTypeID;
                string name = NavMesh.GetSettingsNameFromID(id);
                agentTypeNames[i] = name;
                if (id == agentTypeID.intValue)
                    index = i;
            }
            agentTypeNames[count] = "";
            agentTypeNames[count + 1] = "Open Agent Settings...";

            bool validAgentType = index != -1;
            if (!validAgentType)
            {
                Rect warningRect = rect;
                warningRect.height *= .5f;
                warningRect.y += warningRect.height;
                EditorGUI.HelpBox(warningRect, "Agent Type invalid.", MessageType.Warning);

                rect.height *= .5f;
            }

            EditorGUI.BeginProperty(rect, GUIContent.none, agentTypeID);

            EditorGUI.BeginChangeCheck();
            index = EditorGUI.Popup(rect, labelName, index, agentTypeNames);
            if (EditorGUI.EndChangeCheck())
            {
                if (index >= 0 && index < count)
                {
                    int id = NavMesh.GetSettingsByIndex(index).agentTypeID;
                    agentTypeID.intValue = id;
                }
                else if (index == count + 1)
                {
                    NavMeshEditorHelpers.OpenAgentSettings(-1);
                }
            }

            EditorGUI.EndProperty();
        }

        // Agent mask is a set (internally array/list) of agentTypeIDs.
        // It is used to describe which agents modifiers apply to.
        // There is a special case of "None" which is an empty array.
        // There is a special case of "All" which is an array of length 1, and value of -1.
        public static void AgentMaskPopup(string labelName, SerializedProperty agentMask)
        {
            // Contents of the dropdown box.
            string popupContent = "";

            if (agentMask.hasMultipleDifferentValues)
                popupContent = "\u2014";
            else
                popupContent = GetAgentMaskLabelName(agentMask);

            GUIContent content = new GUIContent(popupContent);
            Rect popupRect = GUILayoutUtility.GetRect(content, EditorStyles.popup);

            EditorGUI.BeginProperty(popupRect, GUIContent.none, agentMask);
            popupRect = EditorGUI.PrefixLabel(popupRect, 0, new GUIContent(labelName));
            bool pressed = GUI.Button(popupRect, content, EditorStyles.popup);

            if (pressed)
            {
                bool show = !agentMask.hasMultipleDifferentValues;
                bool showNone = show && agentMask.arraySize == 0;
                bool showAll = show && IsAll(agentMask);

                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("None"), showNone, SetAgentMaskNone, agentMask);
                menu.AddItem(new GUIContent("All"), showAll, SetAgentMaskAll, agentMask);
                menu.AddSeparator("");

                int count = NavMesh.GetSettingsCount();
                for (int i = 0; i < count; i++)
                {
                    int id = NavMesh.GetSettingsByIndex(i).agentTypeID;
                    string sname = NavMesh.GetSettingsNameFromID(id);

                    bool showSelected = show && AgentMaskHasSelectedAgentTypeID(agentMask, id);
                    object[] userData = new object[] { agentMask, id, !showSelected };
                    menu.AddItem(new GUIContent(sname), showSelected, ToggleAgentMaskItem, userData);
                }

                menu.DropDown(popupRect);
            }

            EditorGUI.EndProperty();
        }

        public static GameObject CreateAndSelectGameObject(string suggestedName, GameObject parent)
        {
            Transform parentTransform = parent != null ? parent.transform : null;
            string uniqueName = GameObjectUtility.GetUniqueNameForSibling(parentTransform, suggestedName);
            GameObject child = new GameObject(uniqueName);

            Undo.RegisterCreatedObjectUndo(child, "Create " + uniqueName);
            if (parentTransform != null)
                Undo.SetTransformParent(child.transform, parentTransform, "Parent " + uniqueName);

            Selection.activeGameObject = child;

            return child;
        }

        static bool IsAll(SerializedProperty agentMask)
        {
            return agentMask.arraySize == 1 && agentMask.GetArrayElementAtIndex(0).intValue == -1;
        }

        static void ToggleAgentMaskItem(object userData)
        {
            object[] args = (object[])userData;
            SerializedProperty agentMask = (SerializedProperty)args[0];
            int agentTypeID = (int)args[1];
            bool value = (bool)args[2];

            ToggleAgentMaskItem(agentMask, agentTypeID, value);
        }

        static void ToggleAgentMaskItem(SerializedProperty agentMask, int agentTypeID, bool value)
        {
            if (agentMask.hasMultipleDifferentValues)
            {
                agentMask.ClearArray();
                agentMask.serializedObject.ApplyModifiedProperties();
            }

            // Find which index this agent type is in the agentMask array.
            int idx = -1;
            for (int j = 0; j < agentMask.arraySize; j++)
            {
                SerializedProperty elem = agentMask.GetArrayElementAtIndex(j);
                if (elem.intValue == agentTypeID)
                    idx = j;
            }

            // Handle "All" special case.
            if (IsAll(agentMask))
            {
                agentMask.DeleteArrayElementAtIndex(0);
            }

            // Toggle value.
            if (value)
            {
                if (idx == -1)
                {
                    agentMask.InsertArrayElementAtIndex(agentMask.arraySize);
                    agentMask.GetArrayElementAtIndex(agentMask.arraySize - 1).intValue = agentTypeID;
                }
            }
            else
            {
                if (idx != -1)
                {
                    agentMask.DeleteArrayElementAtIndex(idx);
                }
            }

            agentMask.serializedObject.ApplyModifiedProperties();
        }

        static void SetAgentMaskNone(object data)
        {
            SerializedProperty agentMask = (SerializedProperty)data;
            agentMask.ClearArray();
            agentMask.serializedObject.ApplyModifiedProperties();
        }

        static void SetAgentMaskAll(object data)
        {
            SerializedProperty agentMask = (SerializedProperty)data;
            agentMask.ClearArray();
            agentMask.InsertArrayElementAtIndex(0);
            agentMask.GetArrayElementAtIndex(0).intValue = -1;
            agentMask.serializedObject.ApplyModifiedProperties();
        }

        static string GetAgentMaskLabelName(SerializedProperty agentMask)
        {
            if (agentMask.arraySize == 0)
                return "None";

            if (IsAll(agentMask))
                return "All";

            if (agentMask.arraySize <= 3)
            {
                string labelName = "";
                for (int j = 0; j < agentMask.arraySize; j++)
                {
                    SerializedProperty elem = agentMask.GetArrayElementAtIndex(j);
                    string settingsName = NavMesh.GetSettingsNameFromID(elem.intValue);
                    if (string.IsNullOrEmpty(settingsName))
                        continue;

                    if (labelName.Length > 0)
                        labelName += ", ";
                    labelName += settingsName;
                }
                return labelName;
            }

            return "Mixed...";
        }

        static bool AgentMaskHasSelectedAgentTypeID(SerializedProperty agentMask, int agentTypeID)
        {
            for (int j = 0; j < agentMask.arraySize; j++)
            {
                SerializedProperty elem = agentMask.GetArrayElementAtIndex(j);
                if (elem.intValue == agentTypeID)
                    return true;
            }
            return false;
        }
    }
}
