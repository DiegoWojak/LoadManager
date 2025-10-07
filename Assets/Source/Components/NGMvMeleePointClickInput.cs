using System;
using Invector.vCharacterController.PointClick;
using UnityEngine;

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
                // Obtener el nombre del motion actual (ejemplo: SwordAttack, WeakAttack_SwordA, etc.)
                string motionName = GetCurrentAttackMotion();
                if (combatAnimationManager != null && !string.IsNullOrEmpty(motionName))
                {
                    float speed = GetSpeedForAttack();
                    // Asume que el Animator tiene un parámetro "AttackSpeed" y el AnimatorState lo usa
                    animator.SetFloat("AttackSpeed", speed);
                }
                TriggerAttack();
            }
        }
    }

    // Método para obtener el nombre del motion actual (debes adaptar esto a tu lógica de ataque)
    private string GetCurrentAttackMotion()
    {
        if (meleeManager != null && combatAnimationManager != null)
        {
            int attackID = meleeManager.GetAttackID(); //unarmed 0 sword 1 random 2 twohander 4 para weakattacks
            string attackPowerType = GetPowerType(); //WeakAttack // StrongAttack // etc.
            string attackWeaponType = GetAttackType(); // ShortKatana // LongKatana // TwoHander // Unarmed // etc.
            string motion = combatAnimationManager.GetMotionForAttack(attackID, attackPowerType, attackWeaponType);

            return motion;
        }
        return "";
    }

    private float GetSpeedForAttack()
    {
        if (meleeManager != null && combatAnimationManager != null)
        {

            int attackID = meleeManager.GetAttackID(); //unarmed 0 sword 1 random 2 twohander 4 para weakattacks
            string attackPowerType = GetPowerType(); //WeakAttack // StrongAttack // etc.
            string attackWeaponType = GetAttackType(); // ShortKatana // LongKatana // TwoHander // Unarmed // etc.
            
            float speed = combatAnimationManager.GetSpeedForAttack(attackID, attackPowerType, attackWeaponType);

            return speed;
        }
        return 1.0f;
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


