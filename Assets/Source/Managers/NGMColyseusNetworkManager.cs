using UnityEngine;
using Colyseus;
using Assets.Source;
using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine.SocialPlatforms;
using Invector.vCharacterController;

public class NGMColyseusNetworkManager : LoaderBase<NGMColyseusNetworkManager>
{
    [Header("Connection")]
    public string websocketUrl = "ws://localhost:2567";
    public string roomName = "lobby_room";
    public float sendRate = 10f; // sends per second
    [SerializeField]
    private GameObject InputPlayerPrefab;
    [SerializeField]
    private GameObject remoteNavMeshPlayerPrefab;
    private ColyseusClient client;
    private ColyseusRoom<LobbyRoomState> room;
    public ColyseusRoom<LobbyRoomState> Room { get { return room; } }
    private Dictionary<string, NetworkedPlayer> remotePlayers = new Dictionary<string, NetworkedPlayer>();

    public NetworkedPlayer localPlayer;

    private Awaitable SendFunc;

    public string SessionId { get; private set; }

    protected CancellationTokenSource sendLoopCTS;

    public override void Init()
    {
        base.Init();
        NetworkedPlayer.OnRegisteredLocalPlayer += ConfigurateNewGameObject;
        isLoaded = true;
    }

    private async void Start()
    {
        client = new ColyseusClient(websocketUrl);
        var options = new Dictionary<string, object>()
        {
            ["YOUR_ROOM_OPTION_1"] = "option 1",
            ["YOUR_ROOM_OPTION_2"] = "option 2"
        };
        room = await client.JoinOrCreate<LobbyRoomState>(roomName, options);
        Debug.Log("Joined room: " + room.Name);

        try
        {
            SessionId = room.SessionId;
        }
        catch
        {
            SessionId = null;
            Debug.LogError("Not session Id Found for Room entity");
        }

        room.OnMessage<string>("init", (message) =>
            {
                // message could contain assigned sessionId if needed
                Debug.Log("[Colyseus] room init: " + message.ToString());
            });

        // Recibir posición de spawn asignada por el servidor para ESTE cliente
        room.OnMessage<SpawnData>("spawn_position", (data) =>
        {
            if (data != null && data.id == SessionId)
            {
                Debug.Log($"[Client] Server assigned spawn position: ({data.x}, {data.y}, {data.z})");

                // Instanciar jugador local en la posición asignada por el servidor
                if (localPlayer == null)
                {
                    localPlayer = Instantiate(InputPlayerPrefab, new Vector3(data.x, data.y, data.z), Quaternion.identity).GetComponent<NetworkedPlayer>();
                    localPlayer.isLocal = true;
                    localPlayer.networkname = data.id;
                    localPlayer.RegisterLocal();
                    remotePlayers.Add(data.id, localPlayer);
                    Debug.Log($"[Client] Instantiated as Local {localPlayer.networkname}");

                    // Iniciar envío de posiciones
                    StartSendLoopAwaitable();
                }
            }
        });

        // Recibir notificación de otros jugadores (existentes o nuevos)
        room.OnMessage<SpawnData>("player_joined", (data) =>
        {
            if (data != null && data.id != SessionId && !remotePlayers.ContainsKey(data.id))
            {
                Debug.Log($"[Client] Remote player joined: {data.id} at ({data.x}, {data.y}, {data.z})");
                SpawnRemotePlayer(data.id, new Vector3(data.x, data.y, data.z));
            }
        });

        room.OnMessage<SpawnData>("despawn", (msg) =>
        {
            if (msg != null)
            {
                DespawnRemotePlayer(msg.id);
            }
        });

        room.OnMessage<PositionData>("position", (p) =>
        {
            if (p == null) return;
            if (p.id == SessionId) return; // ignore our own broadcast

            if (remotePlayers.TryGetValue(p.id, out var np))
            {
                np.ApplyNetworkPosition(new Vector3(p.x, p.y, p.z), p.timestamp);
            }
            // Ya no auto-spawneamos aquí - el servidor envía "player_joined" primero
        });

        room.OnMessage<AnimationData>("animation", (data) =>
        {
            if (data == null || data.id == SessionId) return;

            if (remotePlayers.TryGetValue(data.id, out var np))
            {
                //Call animate
            }
        });

        room.OnMessage<AttackData>("attack", (data) =>
        {
            if (data == null || data.id == SessionId) return;

            if (remotePlayers.TryGetValue(data.id, out NetworkedPlayer np))
            {
                var meleeInput = np.GetComponent<NGMvMeleePointClickInput>();
                if (meleeInput != null)
                {
                    //trigger attack from that 
                    //meleeInput.(data.attackID, data.powerType, data.weaponType);
                }
            }
        });

        room.OnStateChange += UpdateState;

        // El jugador local se creará cuando el servidor envíe "spawn_position"
        // Ya no enviamos "spawn" al servidor - el servidor decide la posición
    }

    public void UpdateState(LobbyRoomState state, bool isFirst)
    {

    }

    public async void SendAnimation(string animationName)
    {
        if (room != null)
        {
            var data = new AnimationData
            {
                id = SessionId,
                animation = animationName,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            await room.Send("animation", data);
        }
    }

    public async void StartSendLoopAwaitable()
    {
        // Cancelar loop anterior si existe
        sendLoopCTS?.Cancel();
        sendLoopCTS?.Dispose();

        // Crear nuevo token de cancelación
        sendLoopCTS = new CancellationTokenSource();

        // Iniciar el loop
        await SendLoopAwaitable(sendLoopCTS.Token);
    }

    public void StopSendLoopAwaitable()
    {
        sendLoopCTS?.Cancel();
    }

    private async Awaitable SendLoopAwaitable(CancellationToken ct)
    {
        float waitTime = 1f / sendRate;

        try
        {
            while (true)
            {
                // Verificar cancelación antes de continuar
                ct.ThrowIfCancellationRequested();

                // Esperar el tiempo especificado
                await Awaitable.WaitForSecondsAsync(waitTime, ct);

                // Si room es null, salir del loop
                if (room == null)
                {
                    Debug.Log("Room is null, stopping send loop");
                    break;
                }

                // Si localPlayer es null, continuar sin enviar
                if (localPlayer == null)
                {
                    continue;
                }

                // Enviar posición
                var pos = localPlayer.transform.position;
                var msg = new PositionData
                {
                    id = SessionId ?? "local",
                    x = pos.x,
                    y = pos.y,
                    z = pos.z,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                await room.Send("position", msg);
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("SendLoop cancelled");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in SendLoop: {ex.Message}");
        }
    }

    private void DespawnRemotePlayer(string id)
    {
        if (remotePlayers.TryGetValue(id, out var player))
        {
            remotePlayers.Remove(id);
            Destroy(player.gameObject);
            Debug.Log($"[Network] Despawned remote player {id}");
        }
    }

    private void SpawnRemotePlayer(string id, Vector3 vector3)
    {

        GameObject instance = Instantiate(remoteNavMeshPlayerPrefab, vector3, Quaternion.identity);

        var np = instance.GetComponent<NetworkedPlayer>();
        if (np != null)
        {
            np.isLocal = false;
            np.ResetForRemote();
            np.networkname = id;
            remotePlayers.Add(id, np);
            Debug.Log($"[Network] Spawned remote player {id} at {vector3}");
        }
        else
        {
            Debug.LogError($"[Network] Remote prefab missing NetworkedPlayer component!");
        }
    }

    private void OnDestroy()
    {
        // Cancelar y limpiar el loop
        sendLoopCTS?.Cancel();
        sendLoopCTS?.Dispose();
    }

    private void OnDisable()
    {
        // También puedes cancelar cuando se desactiva el componente
        StopSendLoopAwaitable();
    }

    private void OnApplicationQuit()
    {
        sendLoopCTS?.Cancel();
        sendLoopCTS?.Dispose();
        StopSendLoopAwaitable();

        try
        {
            room?.Leave();
        }
        catch
        {
            // Ignorar errores al cerrar    
        }
    }
    
    private void ConfigurateNewGameObject(NetworkedPlayer _nObject)
    {
        if (_nObject != localPlayer)
        {
            var _ = _nObject.GetComponent<NGMvMeleePointClickInput>();
            if (_){ _.enabled = false;}
        }
    }
}

[SerializeField]
public class SpawnData
{
    public string id;
    public float x;
    public float y;
    public float z;
}

[SerializeField]
public class PositionData
{
    public string id;
    public float x;
    public float y;
    public float z;
    public float timestamp;
}

[SerializeField]
public class AnimationData
{
    public string id;
    public string animation;
    public float timestamp;
}

[SerializeField]
public class AttackData
{
    public string id;
    public int attackID;
    public string powerType;
    public string weaponType;
    public float timestamp;
}