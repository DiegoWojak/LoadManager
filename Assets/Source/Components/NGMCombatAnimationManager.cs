using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class NGMCombatAnimationManager : MonoBehaviour
{
    [Header("JSON Configuration")]
    [SerializeField] private TextAsset animatorJsonFile;
    
    private AnimatorData animatorData;
    private Dictionary<string, Dictionary<string, Dictionary<string, StateData>>> animationCache;


    void Awake()
    {
        LoadAnimatorData();
        BuildAnimationCache();
    }

    private void BuildAnimationCache()
    {
        animationCache = new Dictionary<string, Dictionary<string, Dictionary<string, StateData>>>();

        if (animatorData == null) return;

        foreach (var powerTypeSubMachine in animatorData.subStateMachines)
        {
            string powerType = powerTypeSubMachine.name; // "StrongAttacks", "WeakAttacks", "Null"
            
            if (!animationCache.ContainsKey(powerType))
            {
                animationCache[powerType] = new Dictionary<string, Dictionary<string, StateData>>();
            }

            foreach (var weaponTypeSubMachine in powerTypeSubMachine.subStateMachines)
            {
                string weaponType = weaponTypeSubMachine.name; // "Unarmed", "SwordAttack", etc.
                
                if (!animationCache[powerType].ContainsKey(weaponType))
                {
                    animationCache[powerType][weaponType] = new Dictionary<string, StateData>();
                }

                foreach (var state in weaponTypeSubMachine.states)
                {
                    animationCache[powerType][weaponType][state.name] = state;
                }
            }
        }

        Debug.Log($"Animation cache built with {animationCache.Count} power types");
        
        // Log para debug - ver qué se cargó
        foreach (var powerType in animationCache.Keys)
        {
            foreach (var weaponType in animationCache[powerType].Keys)
            {
                Debug.Log($"Loaded: {powerType}/{weaponType} with {animationCache[powerType][weaponType].Count} states");
            }
        }
    }

    private void LoadAnimatorData()
    {
        if (animatorJsonFile != null)
        {
            animatorData = JsonUtility.FromJson<AnimatorData>(animatorJsonFile.text);
            Debug.Log($"Animator data loaded: {animatorData.name}");
        }
        else
        {
            Debug.LogError("Animator JSON file not assigned!");
        }
    }

    public string GetMotionForAttack(int attackID, string powerType, string weaponType)
    {
        StateData state = GetStateData(attackID, powerType, weaponType);
        return state != null ? state.motion : "";
    }

    /// <summary>
    /// Obtiene la velocidad de un ataque específico
    /// </summary>
    public float GetSpeedForAttack(int attackID, string powerType, string weaponType)
    {
        StateData state = GetStateData(attackID, powerType, weaponType);
        return state != null ? state.speed : 1.0f;
    }

    /// <summary>
    /// Obtiene los behaviours de un ataque específico
    /// </summary>
    public List<string> GetBehavioursForAttack(int attackID, string powerType, string weaponType)
    {
        StateData state = GetStateData(attackID, powerType, weaponType);
        return state != null ? state.behaviours : new List<string>();
    }

    /// <summary>
    /// Obtiene las transiciones disponibles de un ataque
    /// </summary>
    public List<string> GetTransitionsForAttack(int attackID, string powerType, string weaponType)
    {
        StateData state = GetStateData(attackID, powerType, weaponType);
        return state != null ? state.transitions : new List<string>();
    }

    /// <summary>
    /// Verifica si un ataque tiene transición al siguiente
    /// </summary>
    public bool HasNextAttack(int attackID, string powerType, string weaponType)
    {
        StateData state = GetStateData(attackID, powerType, weaponType);
        return state != null && state.transitions.Count > 0;
    }

    /// <summary>
    /// Obtiene el StateData completo
    /// </summary>
    public StateData GetStateData(int attackID, string powerType, string weaponType)
    {
        // Convierte attackID a nombre de estado (0=A, 1=B, 2=C)
        string stateName = GetStateNameFromID(attackID);

        if (animationCache == null)
        {
            Debug.LogError("Animation cache not initialized!");
            return null;
        }

        if (animationCache.ContainsKey(powerType) &&
            animationCache[powerType].ContainsKey(weaponType) &&
            animationCache[powerType][weaponType].ContainsKey(stateName))
        {
            return animationCache[powerType][weaponType][stateName];
        }

        Debug.LogWarning($"State not found: {powerType}/{weaponType}/{stateName}");
        return null;
    }

    /// <summary>
    /// Obtiene todos los estados de un tipo de arma específico
    /// </summary>
    public Dictionary<string, StateData> GetAllStatesForWeapon(string powerType, string weaponType)
    {
        if (animationCache.ContainsKey(powerType) && 
            animationCache[powerType].ContainsKey(weaponType))
        {
            return animationCache[powerType][weaponType];
        }
        return new Dictionary<string, StateData>();
    }

    /// <summary>
    /// Convierte un ID de ataque a nombre de estado
    /// </summary>
    private string GetStateNameFromID(int attackID)
    {
        switch (attackID)
        {
            case 0: return "A";
            case 1: return "B";
            case 2: return "C";
            default: return "A";
        }
    }

    /// <summary>
    /// Obtiene información completa de un ataque en formato legible
    /// </summary>
    public string GetAttackInfo(int attackID, string powerType, string weaponType)
    {
        StateData state = GetStateData(attackID, powerType, weaponType);
        if (state == null) return "Attack not found";

        return $"Attack: {powerType}/{weaponType}/{state.name}\n" +
               $"Motion: {state.motion}\n" +
               $"Speed: {state.speed}\n" +
               $"Behaviours: {string.Join(", ", state.behaviours)}\n" +
               $"Transitions: {string.Join(", ", state.transitions)}";
    }
}

[Serializable]
public class AnimatorData
{
    public string name;
    public List<StateData> states = new List<StateData>();
    public List<SubStateMachineData> subStateMachines = new List<SubStateMachineData>();
}

[Serializable]
public class SubStateMachineData
{
    public string name;
    public List<StateData> states = new List<StateData>();
    public List<SubStateMachineData> subStateMachines = new List<SubStateMachineData>();
}

[Serializable]
public class StateData
{
    public string name;
    public string motion;
    public float speed;
    public List<string> behaviours = new List<string>();
    public List<string> transitions = new List<string>();
}