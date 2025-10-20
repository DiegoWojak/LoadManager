using System.Collections.Generic;
using UnityEngine;

using System;
using Assets.Source;

public class NGMCombatAnimationManager : LoaderBase<NGMCombatAnimationManager>
{
    // Última configuración de ataque usada
    public string LastPowerType { get; private set; }
    public string LastWeaponType { get; private set; }

    [Header("JSON Configuration")]
    [SerializeField] private TextAsset animatorJsonFile;

    private AnimatorData animatorData;
    private Dictionary<string, Dictionary<string, Dictionary<string, StateData>>> animationCache;

    public override void Init()
    {
        base.Init();
        LoadAnimatorData();
        BuildAnimationCache();
        isLoaded = true;
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

    public Dictionary<string, StateData> GetMotionForAttack(int attackID, string powerType, string weaponType)
    {
        var state = GetStateData(attackID, powerType, weaponType);
        return state != null ? state : null;
    }


    public Dictionary<string, StateData> GetStateData(int attackID, string powerType, string weaponType)
    {
        if (animationCache == null)
        {
            Debug.LogError("Animation cache not initialized!");
            return null;
        }

        if (animationCache.ContainsKey(powerType) &&
            animationCache[powerType].ContainsKey(weaponType))
        {
            return animationCache[powerType][weaponType];
        }

        Debug.LogWarning($"State not found: {powerType}/{weaponType}");
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

    public float GetAttackSpeed(AnimatorStateInfo stateInfo)
    {
        int index = stateInfo.IsName("B") ? 1 :
                    stateInfo.IsName("A") ? 0 :
                    stateInfo.IsName("C") ? 2 : -1;

        Debug.Log($"Attack speed for {index}");

        if (LastUsedMotions == null || index == -1) return 1.0f;
        if (LastUsedMotions.ContainsKey(index))
            return LastUsedMotions[index].speed;
        return 1.0f;
    }

    Dictionary<int, StateData> LastUsedMotions = null;
    public void SaveMotionInLastUsed(Dictionary<string, StateData> states)
    {
        if (states == null || states.Count == 0)
        {
            LastUsedMotions = null;
            return;
        }

        LastUsedMotions = new Dictionary<int, StateData>();
        foreach (var kv in states)
        {
            int idx = StateNameToIndex(kv.Key);
            LastUsedMotions[idx] = kv.Value;
        }
    }

    private int StateNameToIndex(string name)
    {
        if (string.IsNullOrEmpty(name)) return 0;
        switch (name)
        {
            case "A": return 0;
            case "B": return 1;
            case "C": return 2;
            default:
                if (name.StartsWith("A")) return 0;
                if (name.StartsWith("B")) return 1;
                if (name.StartsWith("C")) return 2;
                return 0;
        }
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