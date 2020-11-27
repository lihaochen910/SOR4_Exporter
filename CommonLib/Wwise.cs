using System.Collections.Generic;
using System.Runtime.InteropServices;
using CommonLib;

public static class Wwise
{
	public class SoundBankDummyAsset
	{
	}

	public enum MusicEventType
	{
		MusicSyncBeat = 0,
		MusicSyncBar = 1,
		MusicSyncEntry = 2,
		MusicSyncExit = 3,
		MusicSyncGrid = 4,
		MusicSyncUserCue = 5,
		MusicSyncPoint = 6,
		MusicSyncCount = 7
	}

	public struct AkVector
	{
		public float x;

		public float y;

		public float z;

		public AkVector(Vec3 source)
		{
			x = source.x;
			y = source.y;
			z = source.z * -1f;
		}
	}

	public struct MusicEvent
	{
		public ulong gameObjectId;

		public MusicEventType eventType;

		public int userCueId;

		public MusicEvent(MusicEventType evtType)
		{
			gameObjectId = 0uL;
			eventType = evtType;
			userCueId = 0;
		}
	}

	private const string nativeLibName = "Wwise";

	public static Dictionary<string, string> stateNameByGroupName = new Dictionary<string, string>();

	public static Dictionary<string, float> rtpcById = new Dictionary<string, float>();

	[DllImport("Wwise", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool native_wwise_init(string soundbankPath);

	[DllImport("Wwise", CallingConvention = CallingConvention.Cdecl)]
	private static extern void native_wwise_destroy();

	[DllImport("Wwise", CallingConvention = CallingConvention.Cdecl)]
	private static extern void native_wwise_update();

	[DllImport("Wwise", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool native_wwise_loadbank(string bankName);

	[DllImport("Wwise", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool native_wwise_unloadbank(string bankName);

	[DllImport("Wwise", CallingConvention = CallingConvention.Cdecl)]
	private static extern void native_wwise_register_gameobject_with_id(ulong gameObjectId);

	[DllImport("Wwise", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	private static extern void native_wwise_register_gameobject_with_id_and_name(ulong gameObjectId, string objectName);

	[DllImport("Wwise", CallingConvention = CallingConvention.Cdecl)]
	private static extern void native_wwise_set_switch(string switchGroupName, string switchName, ulong gameObjectId);

	[DllImport("Wwise", CallingConvention = CallingConvention.Cdecl)]
	private static extern void native_wwise_set_state(string stateGroupName, string stateName);

	[DllImport("Wwise", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool native_wwise_post_event(string eventName);

	[DllImport("Wwise", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool native_wwise_post_event_with_id(string eventName, ulong gameObjectId);

	[DllImport("Wwise", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool native_wwise_post_trigger_with_id(string triggerName, ulong gameObjectId);

	[DllImport("Wwise", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool native_wwise_post_trigger(string triggerName);

	[DllImport("Wwise", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool native_wwise_set_listener_position(AkVector position);

	[DllImport("Wwise", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool native_wwise_set_gameobject_position(ulong gameObjectId, AkVector position);

	[DllImport("Wwise", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool native_wwise_set_rtpc_value(string rtpcName, float value);

	[DllImport("Wwise", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool native_wwise_set_rtpc_value_with_id(string rtpcName, float value, ulong gameObjectId);

	[DllImport("Wwise", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool native_wwise_get_music_event(out MusicEvent musicEvent);

	[DllImport("Wwise", CallingConvention = CallingConvention.Cdecl)]
	public static extern int native_wwise_get_total_memory();

	[DllImport("Wwise", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool native_wwise_is_game_object_active(ulong gameObjectId);

	[DllImport("Wwise", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern void native_wwise_unregister_inactive_game_objects();

	public static bool init(string soundbankPath)
	{
		return native_wwise_init(soundbankPath);
	}

	public static void destroy()
	{
		native_wwise_destroy();
	}

	public static void update()
	{
		native_wwise_update();
	}

	public static void load_bank(string bankName)
	{
		native_wwise_loadbank(bankName);
	}

	public static void unloadbank(string bankName)
	{
		native_wwise_unloadbank(bankName);
	}

	public static void register_gameobject(ulong gameObjectId, string objectName)
	{
		native_wwise_register_gameobject_with_id_and_name(gameObjectId, objectName);
	}

	public static void set_switch(string switchGroupName, string switchName, ulong gameObjectId)
	{
		if (!switchGroupName.is_null_or_empty())
		{
			native_wwise_set_switch(switchGroupName, switchName, gameObjectId);
		}
	}

	public static void set_state(string stateGroupName, string stateName)
	{
		if (!stateGroupName.is_null_or_empty() && (!stateNameByGroupName.ContainsKey(stateGroupName) || !(stateNameByGroupName[stateGroupName] == stateName)))
		{
			stateNameByGroupName[stateGroupName] = stateName;
			native_wwise_set_state(stateGroupName, stateName);
		}
	}

	public static void post_event(string eventName, ulong gameObjectId)
	{
		if (!eventName.is_null_or_empty())
		{
			native_wwise_post_event_with_id(eventName, gameObjectId);
		}
	}

	public static void post_trigger(string triggerName)
	{
		if (!triggerName.is_null_or_empty())
		{
			native_wwise_post_trigger(triggerName);
		}
	}

	public static void post_trigger(string triggerName, ulong gameObjectId)
	{
		if (!triggerName.is_null_or_empty())
		{
			native_wwise_post_trigger_with_id(triggerName, gameObjectId);
		}
	}

	public static void set_listener_position(AkVector position)
	{
		native_wwise_set_listener_position(position);
	}

	public static void set_gameobject_position(ulong gameObjectId, AkVector position)
	{
		native_wwise_set_gameobject_position(gameObjectId, position);
	}

	public static void wwise_set_rtpc_value(string rtpcName, float value)
	{
		if (!rtpcName.is_null_or_empty())
		{
			rtpcById[rtpcName] = value;
			native_wwise_set_rtpc_value(rtpcName, value);
		}
	}

	public static void set_rtpc_value(string rtpcName, float value, ulong gameObjectId)
	{
		if (!rtpcName.is_null_or_empty())
		{
			native_wwise_set_rtpc_value_with_id(rtpcName, value, gameObjectId);
		}
	}

	public static bool get_music_event(out MusicEvent musicEvent)
	{
		return native_wwise_get_music_event(out musicEvent);
	}

	public static bool is_game_object_active(uint id)
	{
		return native_wwise_is_game_object_active(id);
	}

	public static void unregister_inactive_game_objects()
	{
		native_wwise_unregister_inactive_game_objects();
	}
}
