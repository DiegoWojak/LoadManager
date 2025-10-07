using Invector.vCharacterController.PointClick;
using UnityEngine;

public class NGMvMeleePointClickInput : vMeleePointClickInput
{
    protected override void Update()
    {
        base.Update();

        // Ataque personalizado con tecla F o botón Fire1
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (meleeManager != null)
            {
                TriggerAttack();
            }
        }
    }


}
