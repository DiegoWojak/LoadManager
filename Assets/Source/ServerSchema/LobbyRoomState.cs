// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 3.0.61
// 

using Colyseus.Schema;
#if UNITY_5_3_OR_NEWER
using UnityEngine.Scripting;
#endif

public partial class LobbyRoomState : Schema {
#if UNITY_5_3_OR_NEWER
[Preserve]
#endif
public LobbyRoomState() { }
	[Type(0, "string")]
	public string roomName = default(string);

	[Type(1, "map", typeof(MapSchema<Player>))]
	public MapSchema<Player> players = null;
}

