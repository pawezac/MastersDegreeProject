using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Planet))]
public class PlanetEditor : Editor
{
    Planet planet;

    Editor shapeEditor;
    Editor colourEditor;

    public override void OnInspectorGUI()
    {
        using (var check = new EditorGUI.ChangeCheckScope())
        { 
            base.OnInspectorGUI();
            if (check.changed)
            {
                planet.GeneratePlanet();
            }
        }

        if (GUILayout.Button(nameof(planet.GeneratePlanet)))
        {
            planet.GeneratePlanet();
        }

        DrawSettingEditor(planet.shapeSettings, planet.OnShapeSettingsUpdated, ref planet.shapeSettingsFoldout, ref shapeEditor);
        DrawSettingEditor(planet.colourSettings, planet.OnColourSettingsUpdated, ref planet.colourSettingsFoldout, ref colourEditor);
    }

    void DrawSettingEditor(Object settings, System.Action onSettingsUpdate, ref bool foldOut, ref Editor editor)
    {
        if (settings == null) return;

        foldOut = EditorGUILayout.InspectorTitlebar(foldOut, settings);
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            if (!foldOut) return;

            CreateCachedEditor(settings, null, ref editor);
            editor.OnInspectorGUI();

            if (check.changed)
            {
                onSettingsUpdate?.Invoke();
            }
        }
    }

    private void OnEnable()
    {
        planet = (Planet)target;
    }
}
