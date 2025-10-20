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

public partial class Player : Schema {
#if UNITY_5_3_OR_NEWER
[Preserve]
#endif
public Player() { }
	[Type(0, "string")]
	public string sessionId = default(string);

	[Type(1, "string")]
	public string username = default(string);

	[Type(2, "boolean")]
	public bool isTyping = default(bool);

	[Type(3, "number")]
	public float joinedAt = default(float);

	[Type(4, "string")]
	public string lastMessage = default(string);

	[Type(5, "number")]
	public float lastMessageTime = default(float);

	[Type(6, "number")]
	public float x = default(float);

	[Type(7, "number")]
	public float y = default(float);

	[Type(8, "number")]
	public float z = default(float);

	[Type(9, "number")]
	public float rotationY = default(float);

	[Type(10, "string")]
	public string animation = default(string);

	[Type(11, "int64")]
	public long timestamp = default(long);
}

