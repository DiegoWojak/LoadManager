using System;
using System.Collections.Generic;
using Invector.vCharacterController.PointClick;
using UnityEngine;
using UnityEngine.AI;

public class NGMvMeleePointClickInput : vMeleePointClickInput
{
    private NGMCombatAnimationManager combatAnimationManager;

    protected override void Start()
    {
        base.Start();
        // Busca el manager en la escena (puedes cambiar esto por inyección directa si lo prefieres)
        combatAnimationManager = FindFirstObjectByType<NGMCombatAnimationManager>();
    }

    protected override void Update()
    {
        base.Update();

        // Ataque personalizado con tecla F o botón Fire1
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (meleeManager != null)
            {
                // Obtener el nombre del motion actual (ejemplo:  SwordAttack, WeakAttack_SwordA, etc.)
                var motions = GetCurrentAttackMotion();
                if (combatAnimationManager != null)
                {
                    combatAnimationManager.SaveMotionInLastUsed(motions);
                    // Asume que  el Animator tiene un parámetro "AttackSpeed" y el AnimatorState lo usa

                }
                SendAttackToNetwork(motions);
                TriggerAttack();
            }
        }
    }

    protected override void PointAndClickMovement()
    {
        try
        {
            base.PointAndClickMovement();
        }
        catch
        {
            Debug.Log($"object name: {gameObject.name} has {meleeManager.isActiveAndEnabled}");
            Debug.Log($"and network id: { GetComponent<NetworkedPlayer>().networkname}");
        }
    }

    private async void SendAttackToNetwork(Dictionary<string, StateData> motions)
    {
        var networkManager = NGMColyseusNetworkManager.Instance;
        if (networkManager?.Room != null)
        {
            var attackData = new AttackData
            {
                id = networkManager.SessionId,
                attackID = meleeManager.GetAttackID(),
                powerType = GetPowerType(),
                weaponType = GetAttackType(),
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            await networkManager.Room.Send("attack", attackData);
        }
    }

    // Método para obtener el nombre del motion actual (debes adaptar esto a tu lógica de ataque)
    private Dictionary<string, StateData> GetCurrentAttackMotion()
    {
        if (meleeManager != null && combatAnimationManager != null)
        {
            int attackID = meleeManager.GetAttackID(); //unarmed 0 sword 1 random 2 twohander 4 para weakattacks
            string attackPowerType = GetPowerType(); //WeakAttack // StrongAttack // etc.
            string attackWeaponType = GetAttackType(); // ShortKatana // LongKatana // TwoHander // Unarmed // etc.

            return combatAnimationManager.GetMotionForAttack(attackID, attackPowerType, attackWeaponType);
        }
        return null;
    }

    // Ejemplo de método para obtener el tipo de ataque (debes adaptar según tu lógica)
    private string GetAttackType()
    {
        int weaponID = meleeManager.GetAttackID(); // Ejemplo

        switch (weaponID)
        {
            case 0: return "Unarmed";
            case 1: return "SwordAttack";
            case 2: return "SwordRandomAttack";
            case 4: return "2HandWeaponAttack";
            default: return "Unarmed";
        }
    }

    private string GetPowerType()
    {
        return "WeakAttacks";
    }
}


