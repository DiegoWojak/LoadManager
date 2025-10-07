using UnityEngine;
using UnityEditor;

public class AnimationClipLister : EditorWindow
{
    [MenuItem("Tools/List Animation Clips of Selected GameObject")]
    public static void ShowWindow()
    {
        GetWindow<AnimationClipLister>("Animation Clip Lister");
    }

    private void OnGUI()
    {
        if (Selection.activeGameObject == null)
        {
            EditorGUILayout.LabelField("Selecciona un GameObject en la escena.");
            return;
        }

        Animator animator = Selection.activeGameObject.GetComponent<Animator>();
        if (animator == null)
        {
            EditorGUILayout.LabelField("El objeto seleccionado no tiene un componente Animator.");
            return;
        }

        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        if (controller == null)
        {
            EditorGUILayout.LabelField("El Animator no tiene un controlador asignado.");
            return;
        }


        GUILayout.Space(10);
        if (GUILayout.Button("Exportar animaciones y parámetros a JSON (ruta fija)"))
        {
            ExportToJson(animator, controller, "Assets/AnimationExport.json");
        }
        if (GUILayout.Button("Exportar animaciones y parámetros a JSON (elegir ruta)"))
        {
            string path = EditorUtility.SaveFilePanel("Guardar JSON de animaciones", Application.dataPath, "AnimationExport", "json");
            if (!string.IsNullOrEmpty(path))
            {
                ExportToJson(animator, controller, path);
            }
        }
        
        EditorGUILayout.LabelField("Clips de animación:");
        foreach (var clip in controller.animationClips)
        {
            EditorGUILayout.LabelField("- " + clip.name, GUILayout.ExpandHeight(true));
        }
    }

    private void ExportToJson(Animator animator, RuntimeAnimatorController controller, string path)
    {
        var export = new AnimationExportData();
        foreach (var param in animator.parameters)
        {
            export.parameters.Add(new AnimationParameterData
            {
                name = param.name,
                type = param.type.ToString()
            });
        }
        foreach (var clip in controller.animationClips)
        {
            export.clips.Add(new AnimationClipData
            {
                name = clip.name,
                length = clip.length,
                frameRate = clip.frameRate,
                segments = new System.Collections.Generic.List<AnimationSegmentData> {
                    // Ejemplo de segmento, puedes editarlo luego en el JSON
                    new AnimationSegmentData { startFrame = 0, endFrame = (int)(clip.length * clip.frameRate), duration = clip.length }
                }
            });
        }
        string json = JsonUtility.ToJson(export, true);
        System.IO.File.WriteAllText(path, json);
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Exportación completa", $"Animaciones exportadas a:\n{path}", "OK");
    }

    [System.Serializable]
    public class AnimationExportData
    {
        public System.Collections.Generic.List<AnimationParameterData> parameters = new System.Collections.Generic.List<AnimationParameterData>();
        public System.Collections.Generic.List<AnimationClipData> clips = new System.Collections.Generic.List<AnimationClipData>();
    }
    [System.Serializable]
    public class AnimationParameterData
    {
        public string name;
        public string type;
    }
    [System.Serializable]
    public class AnimationClipData
    {
        public string name;
        public float length;
        public float frameRate;
        public System.Collections.Generic.List<AnimationSegmentData> segments;
    }
    [System.Serializable]
    public class AnimationSegmentData
    {
        public int startFrame;
        public int endFrame;
        public float duration;
    }
}
