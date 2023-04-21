﻿namespace TypeReferences.Demo.Editor
{
    using System;
    using System.Linq;
    using System.Reflection;
    using SolidUtilities;
    using SolidUtilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using Utils;

    [CustomEditor(typeof(TypeReferenceExample), true)]
    public class TypeReferenceExampleEditor : Editor
    {
        private Type targetType;

        private void OnEnable()
        {
            targetType = target.GetType();
        }

        public override void OnInspectorGUI()
        {
            SaveChanges(() =>
            {
                var childProperties = new ChildProperties(serializedObject, true);
                foreach (var property in childProperties)
                {
                    if (property.IsBuiltIn())
                        continue;

                    FieldInfo field = targetType.GetFieldAtPath(property.propertyPath);

                    if (field == null)
                        continue;

                    DrawInfoBoxIfAttributeFound(field);

                    if (field.FieldType != typeof(TypeReference))
                        continue;

                    var typeRefField = new TypeReferenceField(property, field);
                    typeRefField.Draw();
                }
            });

            DrawButtonsIfRequested();
        }

        private void SaveChanges(Action drawFields)
        {
            serializedObject.Update();
            drawFields();
            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawInfoBoxIfAttributeFound(MemberInfo field)
        {
            var infoBoxAttribute = field.GetCustomAttribute<InfoBoxAttribute>();
            if (infoBoxAttribute != null)
                InfoBox.Draw(infoBoxAttribute.Text);
        }

        private void DrawButtonsIfRequested()
        {
            // Loop through all methods with no parameters
            var methods = target.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetParameters().Length == 0);

            foreach (var method in methods)
            {
                DrawInfoBoxIfAttributeFound(method);

                var ba = method.GetCustomAttribute<ButtonAttribute>();

                if (ba == null)
                    continue;

                string buttonName = ObjectNames.NicifyVariableName(method.Name);

                if (GUILayout.Button(buttonName))
                    method.Invoke(target, null);
            }
        }
    }
}