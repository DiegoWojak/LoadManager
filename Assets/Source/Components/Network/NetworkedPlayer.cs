using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Transform))]
public class NetworkedPlayer : MonoBehaviour
{
    [Tooltip("Mark this on the local controlled character.")]
    public bool isLocal;

    private Vector3 targetPosition;
    private float lerpSpeed = 10f;
    private float lastTimestamp;
    public string networkname = "";
    private NavMeshAgent navAgent;
    private NGMvMeleePointClickInput meleeInput;
    private float positionThreshold = 0.5f; // Distance threshold to start moving

    public static Action<NetworkedPlayer> OnRegisteredLocalPlayer;

    void Start()
    {
        targetPosition = transform.position;
        navAgent = GetComponent<NavMeshAgent>();
        meleeInput = GetComponent<NGMvMeleePointClickInput>();
        RegisterLocal();
    }

    public void RegisterLocal()
    {
        if (isLocal)
        {
            OnRegisteredLocalPlayer?.Invoke(this);
        }
    }

    internal void ApplyNetworkPosition(Vector3 position, float timestamp)
    {
        if (isLocal) return; // No aplicar a jugador local

        // Evitar aplicar datos antiguos
        if (timestamp > lastTimestamp)
        {
            targetPosition = position;
            lastTimestamp = timestamp;

            // Si hay NavMeshAgent, usarlo para movimiento natural
            if (navAgent != null && navAgent.enabled)
            {
                float distance = Vector3.Distance(transform.position, position);

                // Solo mover si la distancia es significativa
                if (distance > positionThreshold)
                {
                    navAgent.SetDestination(position);
                }
                else
                {
                    // Si está cerca, detener el agente
                    navAgent.ResetPath();
                }
            }
        }
    }

    internal void ResetForRemote()
    {
        //throw new NotImplementedException();
    }

    internal void DeActiveInputManager()
    {
        if(meleeInput != null)
        {
            meleeInput.enabled = false;
        }
    }

    void Update()
    {
        if (!isLocal && navAgent == null)
        {
            // Fallback: Si no hay NavMeshAgent, usar lerp simple
            // Esto es solo por si el prefab no tiene NavMeshAgent configurado
            transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);
        }
    }
}