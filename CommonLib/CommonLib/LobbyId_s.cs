using System;
using Facepunch.Steamworks;

namespace CommonLib
{
	public struct LobbyId_s : IEquatable<LobbyId_s>
	{
		[Tag(1, null, true)]
		public ulong innerId;

		private LobbyList.Lobby steamLobby;

		public bool IsNull => innerId == 0;

		private LobbyList.Lobby SteamLobby
		{
			get
			{
				if (steamLobby == null || steamLobby.LobbyID != innerId)
				{
					foreach (LobbyList.Lobby lobby in Client.Instance.LobbyList.Lobbies)
					{
						if (lobby.LobbyID == innerId)
						{
							steamLobby = lobby;
							break;
						}
					}
				}
				return steamLobby;
			}
		}

		public string Name
		{
			get
			{
				LobbyList.Lobby lobby = SteamLobby;
				if (lobby == null)
				{
					return string.Empty;
				}
				return lobby.Name;
			}
		}

		public int MaxMembers => SteamLobby?.MemberLimit ?? 0;

		public int MemberCount
		{
			get
			{
				LobbyList.Lobby lobby = SteamLobby;
				if (lobby == null)
				{
					return 0;
				}
				return SteamLobby.NumMembers;
			}
		}

		public override string ToString()
		{
			return innerId.ToString();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is LobbyId_s))
			{
				return false;
			}
			LobbyId_s lobbyId_s = (LobbyId_s)obj;
			return innerId == lobbyId_s.innerId;
		}

		public bool Equals(LobbyId_s other)
		{
			return innerId == other.innerId;
		}

		public override int GetHashCode()
		{
			return innerId.GetHashCode();
		}

		public static bool operator ==(LobbyId_s value1, LobbyId_s value2)
		{
			if ((object)value1 == null || (object)value2 == null)
			{
				return (object)value1 == (object)value2;
			}
			return value1.innerId == value2.innerId;
		}

		public static bool operator !=(LobbyId_s value1, LobbyId_s value2)
		{
			if ((object)value1 == null || (object)value2 == null)
			{
				return (object)value1 != (object)value2;
			}
			return value1.innerId != value2.innerId;
		}

		internal LobbyId_s(ulong steamLobbyId)
		{
			innerId = steamLobbyId;
			steamLobby = null;
		}

		public string get_metadata(string key)
		{
			if (Client.Instance == null)
			{
				return string.Empty;
			}
			if (platform.lobbyDataByLobbyId.TryGetValue(innerId, out var value) && value.metadataDict.TryGetValue(key, out var value2))
			{
				return value2;
			}
			return string.Empty;
		}
	}
}
