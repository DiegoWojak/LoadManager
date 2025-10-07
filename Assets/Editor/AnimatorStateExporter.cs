using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.IO;

public class AnimatorStateExporter : EditorWindow
{
    public AnimatorController animatorController;
    public string exportPath = "Assets/Output/AnimatorStatesExport.json";

    [MenuItem("Tools/Export Animator States to JSON")]
    public static void ShowWindow()
    {
        GetWindow<AnimatorStateExporter>("Animator State Exporter");
    }

    void OnGUI()
    {
        animatorController = (AnimatorController)EditorGUILayout.ObjectField("Animator Controller", animatorController, typeof(AnimatorController), false);
        exportPath = EditorGUILayout.TextField("Export Path", exportPath);
        if (GUILayout.Button("Export to JSON"))
        {
            if (animatorController != null)
            {
                ExportAnimatorStates(animatorController, exportPath);
                EditorUtility.DisplayDialog("Export Complete", "Animator states exported to JSON.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please assign an AnimatorController.", "OK");
            }
        }
    }

    [System.Serializable]
    public class AnimatorStateJson
    {
        public string name;
        public string motion;
        public float speed;
        public List<string> behaviours = new List<string>();
        public List<string> transitions = new List<string>();
    }
    [System.Serializable]
    public class AnimatorStateMachineJson
    {
        public string name;
        public List<AnimatorStateJson> states = new List<AnimatorStateJson>();
        public List<AnimatorStateMachineJson> subStateMachines = new List<AnimatorStateMachineJson>();
    }
    [System.Serializable]
    public class AnimatorLayerJson
    {
        public string name;
        public List<AnimatorStateJson> states = new List<AnimatorStateJson>();
        public List<AnimatorStateMachineJson> subStateMachines = new List<AnimatorStateMachineJson>();
    }
    [System.Serializable]
    public class AnimatorControllerJson
    {
        public List<AnimatorLayerJson> layers = new List<AnimatorLayerJson>();
    }

    void ExportAnimatorStates(AnimatorController controller, string path)
    {
        // Exportar solo la estructura de 'Attacks' como raíz
        AnimatorStateMachineJson attacksRoot = null;
        foreach (var layer in controller.layers)
        {
            // Buscar el subStateMachine 'Attacks' en el root stateMachine
            foreach (var subMachine in layer.stateMachine.stateMachines)
            {
                if (subMachine.stateMachine != null && subMachine.stateMachine.name == "Attacks")
                {
                    attacksRoot = ParseStateMachine(subMachine.stateMachine);
                    break;
                }
            }
            if (attacksRoot != null) break;
        }
        if (attacksRoot == null)
        {
            Debug.LogError("No se encontró el subStateMachine 'Attacks' en el AnimatorController.");
            return;
        }
        var json = JsonUtility.ToJson(attacksRoot, true);
        File.WriteAllText(path, json);
        AssetDatabase.Refresh();
    }

    AnimatorStateMachineJson ParseStateMachine(AnimatorStateMachine stateMachine)
    {
        var smJson = new AnimatorStateMachineJson { name = stateMachine.name };
        // States
        foreach (var state in stateMachine.states)
        {
            smJson.states.Add(ParseState(state.state));
        }
        // Sub-StateMachines
        foreach (var subMachine in stateMachine.stateMachines)
        {
            if (subMachine.stateMachine != null)
                smJson.subStateMachines.Add(ParseStateMachine(subMachine.stateMachine));
        }
        return smJson;

    }

    AnimatorStateJson ParseState(AnimatorState state)
    {
        var stateJson = new AnimatorStateJson
        {
            name = state.name,
            motion = state.motion != null ? state.motion.name : "",
            speed = state.speed,
        };
        foreach (var behaviour in state.behaviours)
        {
            if (behaviour != null)
                stateJson.behaviours.Add(behaviour.GetType().Name);
        }
        foreach (var transition in state.transitions)
        {
            if (transition != null && transition.destinationState != null)
                stateJson.transitions.Add(transition.destinationState.name);
        }
        return stateJson;
    }
}
