using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using Facepunch.Steamworks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SDL2;
using SharpFont;
using SharpFont.HarfBuzz;

namespace CommonLib
{
	public static class platform
	{
		public struct LeaderboardEntry_s
		{
			public MachineId_s machineId;

			public int score;

			public int rank;
		}

		public struct LeaderboardContent_s
		{
			public LeaderboardEntry_s[] worldBest;

			public LeaderboardEntry_s[] worldAroundLocal;

			public LeaderboardEntry_s[] friendBest;
		}

		public enum LeaderboardTypeEnum
		{
			global = 0,
			aroundGlobal = 1,
			friends = 2
		}

		public enum SendDataTypeEnum
		{
			reliable = 0,
			unreliable = 1
		}

		public enum LobbyTypeEnum
		{
			publicLobby = 0,
			privateLobby = 1
		}

		private class SteamLeaderboardRequest
		{
			public string leaderboardId;

			public float timeoutInSeconds;

			public RequestStatusEnum requestedLeaderBoardStatus;

			public Leaderboard worldBestLeaderboard;

			public Leaderboard worldAroundLocalLeaderboard;

			public Leaderboard friendBestLeaderboard;

			public Leaderboard friendAroundLocalLeaderboard;

			public int worldBestCount;

			public int worldAroundLocalCount;

			public int friendBestCount;

			public int friendAroundLocalCount;

			public bool worldBestRequested;

			public bool worldAroundLocalRequested;

			public bool friendBestRequested;

			public LeaderboardContent_s leaderboardContentCache;
		}

		private enum SimulatePlayTogetherEnum
		{
			none = 0,
			createLobbyCalled = 1,
			friendInviteSent = 2,
			lobbyIsFull = 3
		}

		internal struct LobbyData_s
		{
			public Dictionary<string, string> metadataDict;
		}

		private class UserProfileRequest
		{
			public MachineId_s machineId;

			public RequestStatusEnum requestStatus;

			public Image avatarImage;

			public UserProfile userProfile;
		}

		public struct FontDefinition_s
		{
			public string filePath;

			public int subFont;

			public override int GetHashCode()
			{
				return 17 * subFont + filePath.GetHashCode();
			}

			public bool Equals(FontDefinition_s other)
			{
				if (subFont == other.subFont)
				{
					return filePath == other.filePath;
				}
				return false;
			}

			public override bool Equals(object obj)
			{
				if (obj is FontDefinition_s)
				{
					return Equals((FontDefinition_s)obj);
				}
				return false;
			}

			public static bool operator ==(FontDefinition_s a, FontDefinition_s b)
			{
				return a.Equals(b);
			}

			public static bool operator !=(FontDefinition_s a, FontDefinition_s b)
			{
				return !a.Equals(b);
			}
		}

		private static object tempSaveGame;

		private static long targetFrameDuration = Stopwatch.Frequency / 60;

		private static long previousFrameStartTime;

		private static long frameAccumulator;

		private static ContentManager videoContentManager;

		private static VideoPlayer videoPlayer;

		private static SpriteBatch videoSpriteBatch;

		private static int managedMemoryDisplayTimer;

		private static DateTime mostRecentSaveBackupTime;

		private static string _dataFolder;

		private static bool _isRetailLaunchedByVisualStudio;

		private static bool _isInNoGcRegion;

		private static string[] achievementNameArray = new string[37]
		{
			"campaign_axel",
			"campaign_blaze",
			"campaign_cherry",
			"campaign_floyd",
			"campaign_adam",
			"campaign_sor1",
			"campaign_sor2",
			"campaign_sor3",
			"retro_level",
			"meet_adam",
			"beat_shiva",
			"beat_max",
			"beat_final_boss",
			"s_rank",
			"no_damage",
			"s_rank_all",
			"mania",
			"combo_1",
			"combo_2",
			"combo_3",
			"arcade",
			"last_pack",
			"hit_ally",
			"catch_weapon",
			"life_pickup",
			"hole_kill",
			"grab_struggle",
			"elevator_break",
			"golden_chicken",
			"call_police",
			"car_destroyed",
			"break_bottle",
			"break_motorcycles",
			"ball_kill",
			"explosion_3",
			"chandelier_kill",
			"halberd_break"
		};

		private static bool didSetVibrationThrowExceptionOnce;

		public static bool pauseGameWhenWindowsLoseFocus = true;

		private static int runCallbacksCounter;

		private static bool isLoggedOn;

		private static int isLoggedOnCheckCounter;

		public const uint gameAppId = 985890u;

		private static float defaultLeaderboardRequestTimeout = 30f;

		private static Dictionary<string, SteamLeaderboardRequest> leaderboardRequestDict = new Dictionary<string, SteamLeaderboardRequest>();

		private static GlobalLobbyStateEnum globalLobbyState;

		private static Dictionary<string, string> hostLobbyMetadata;

		internal static Dictionary<ulong, LobbyData_s> lobbyDataByLobbyId = new Dictionary<ulong, LobbyData_s>();

		private static bool didJoinStartupLobby;

		private static SimulatePlayTogetherEnum debugSimulatePlayTogether;

		private static List<NetworkData_s> receivedNetworkDataArray = new List<NetworkData_s>();

		private static int _byteSentPerFrame;

		private static int maxUserProfileRequestCount = 128;

		private static Dictionary<MachineId_s, UserProfileRequest> userProfileRequestByMachine = new Dictionary<MachineId_s, UserProfileRequest>();

		private static Library fontLib = null;

		private static Dictionary<FontDefinition_s, Face> faceByFontDef = new Dictionary<FontDefinition_s, Face>();

		private static byte[] _textBitmapBuffer = new byte[0];

		private static List<IntVec2> _spanList = new List<IntVec2>();

		private const string endOfLine = "$(£¥·'\"〈《「『【〔〖〝﹙﹛＄（．［｛￡￥([{£¥'\"‵〈《「『〔〝\ufe34﹙﹛（｛︵︷︹︻︽︿﹁﹃\ufe4f([｛〔〈《「『【〘〖〝'\"｟«$([\\{£¥'\"々〇〉》」〔＄（［｛｠￥￦ #";

		private const string startOfLine = "!%),.:;?]}¢°·'\"\"†‡›℃∶、。〃〆〕〗〞﹚﹜！＂％＇），．：；？！］｝～!),.:;?]}¢·–— '\"•\" 、。〆〞〕〉》」︰︱︲\ufe33﹐﹑﹒\ufe53﹔﹕﹖﹘﹚﹜！），．：；？︶︸︺︼︾﹀﹂﹗］｜｝､)]｝〕〉》」』】〙〗〟'\"｠»ヽヾーァィゥェォッャュョヮヵヶぁぃぅぇぉっゃゅょゎゕゖㇰㇱㇲㇳㇴㇵㇶㇷㇸㇹㇺㇻㇼㇽㇾㇿ々〻‐゠–〜? ! ‼ ⁇ ⁈ ⁉・、:;,。.!%),.:;?]}¢°'\"†‡℃〆〈《「『〕！％），．：；？］｝";

		public static bool ShouldResetToTitleScreenAndFlushSave => false;

		public static bool SinglePlayerAllControllersActive => true;

		public static bool MenusAllControllersActive => true;

		public static GamePadDisplayTypeEnum DefaultGamePadDisplayType => GamePadDisplayTypeEnum.XboxOne;

		public static bool DefaultJapaneseStyleMenuButtons => false;

		public static int MaxControllers => 5;

		public static MachineId_s LocalMachine
		{
			get
			{
				MachineId_s result = default(MachineId_s);
				if (!online_service_is_connected())
				{
					return result;
				}
				result.machineId = Client.Instance.SteamId;
				return result;
			}
		}

		public static bool ShouldPause
		{
			get
			{
				if (pauseGameWhenWindowsLoseFocus && !xna.MainWindowIsActive)
				{
					return true;
				}
				_ = Client.Instance;
				return false;
			}
		}

		public static bool CanShowGamerProfile => true;

		public static bool LobbyIsConnecting
		{
			get
			{
				if (GlobalLobbyState != GlobalLobbyStateEnum.connectingAsHost)
				{
					return GlobalLobbyState == GlobalLobbyStateEnum.connectingAsClient;
				}
				return true;
			}
		}

		public static GlobalLobbyStateEnum GlobalLobbyState
		{
			get
			{
				if (!online_service_is_connected())
				{
					return GlobalLobbyStateEnum.offline;
				}
				return globalLobbyState;
			}
		}

		public static int ByteSentPerFrame => _byteSentPerFrame;

		public static void finish_startup()
		{
		}

		public static void gc_free(ref byte[] buffer)
		{
			buffer = null;
		}

		public static string get_save_file_path(string fileName)
		{
			string text = SDL.SDL_GetPlatform();
			if (text.Equals("Windows"))
			{
				return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Streets of Rage 4 Save and Config", fileName);
			}
			if (text.Equals("Mac OS X"))
			{
				string environmentVariable = Environment.GetEnvironmentVariable("HOME");
				if (string.IsNullOrEmpty(environmentVariable))
				{
					return ".";
				}
				environmentVariable += "/Library/Application Support";
				return Path.Combine(environmentVariable, "Streets of Rage 4 Save and Config", fileName);
			}
			if (text.Equals("Linux"))
			{
				string text2 = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
				if (string.IsNullOrEmpty(text2))
				{
					text2 = Environment.GetEnvironmentVariable("HOME");
					if (string.IsNullOrEmpty(text2))
					{
						return ".";
					}
					text2 += "/.local/share";
				}
				return Path.Combine(text2, "Streets of Rage 4 Save and Config", fileName);
			}
			throw new Exception("SDL platform unhandled: " + text);
		}

		public static RumblePadTypeEnum get_rumble_pad_type(int controllerId)
		{
			return RumblePadTypeEnum.xBox;
		}

		private static int file_path_most_recent_first_comparer(string f1, string f2)
		{
			DateTime lastWriteTime = File.GetLastWriteTime(f1);
			DateTime lastWriteTime2 = File.GetLastWriteTime(f2);
			return -lastWriteTime.CompareTo(lastWriteTime2);
		}

		private static void try_load_save_backups(object saveGameObject, Exception e)
		{
			string text = get_save_file_path("Save.bin");
			string[] files = Directory.GetFiles(get_save_file_path(""), "*.savebackup.bin");
			Array.Sort(files, file_path_most_recent_first_comparer);
			int num = 0;
			while (true)
			{
				string text2 = "";
				if (num < files.Length)
				{
					text2 = $"A backup exists from {File.GetLastWriteTime(files[num])}.";
				}
				string message = "An error occured while reading the save data file \"" + text + "\", it may be corrupted.\r\n\r\n" + text2 + "\r\n\r\n" + utils.get_build_svn_revision() + "\r\nException type: " + e.GetType().Name + "\r\nException message: " + e.Message + "\r\nException stack trace: \r\n" + e.StackTrace + "\r\n";
				string button = "No Backup.";
				if (num < files.Length)
				{
					button = "Load Backup";
				}
				switch (xna.show_runtime_error("Error reading game save", message, "Retry", button, "Exit"))
				{
				case 0:
					try
					{
						reflection.bin_deserialize_save_game_from_file_to_object(text, saveGameObject);
						return;
					}
					catch (Exception ex2)
					{
						e = ex2;
					}
					break;
				case 1:
					if (num < files.Length)
					{
						text = files[num];
						num++;
						try
						{
							reflection.bin_deserialize_save_game_from_file_to_object(text, saveGameObject);
							return;
						}
						catch (Exception ex)
						{
							e = ex;
						}
					}
					break;
				case 2:
					Environment.Exit(0);
					break;
				}
			}
		}

		public static void initial_load_config_and_optionaly_save(object saveGameObject, object configObject)
		{
			simple_serializer.deserialize(configObject, get_save_file_path("Config.txt"));
			string text = get_save_file_path("Save.bin");
			mostRecentSaveBackupTime = DateTime.MinValue;
			if (!File.Exists(text))
			{
				utils.log_write_line("Save file does not exist: " + text, LogImportanceEnum.warning);
				return;
			}
			string[] files = Directory.GetFiles(get_save_file_path(""), "*.savebackup.bin");
			foreach (string path in files)
			{
				DateTime lastWriteTime = File.GetLastWriteTime(path);
				if (lastWriteTime > mostRecentSaveBackupTime)
				{
					mostRecentSaveBackupTime = lastWriteTime;
				}
			}
			try
			{
				reflection.bin_deserialize_save_game_from_file_to_object(text, saveGameObject);
			}
			catch (Exception ex)
			{
				utils.log_write_line($"Save game could not be loaded: {text}\n{ex.GetType()}\n{ex.Message}\n{ex.StackTrace}", LogImportanceEnum.error);
				try_load_save_backups(saveGameObject, ex);
			}
		}

		public static void start_load_save_and_config(object saveGameObject, object configObject)
		{
			throw new InvalidOperationException();
		}

		public static bool load_save_and_config_is_finished()
		{
			throw new InvalidOperationException();
		}

		public static void save_config(object configObject, bool canThrow)
		{
			simple_serializer.serialize(configObject, get_save_file_path("Config.txt"), canThrow);
		}

		public static void save_save_game(object saveGameObject)
		{
			if (saveGameObject == null)
			{
				throw new ArgumentNullException("saveGameObject");
			}
			reflection.bin_serialize_to_file(get_save_file_path("Save.temp.bin"), saveGameObject);
			File.Copy(get_save_file_path("Save.temp.bin"), get_save_file_path("Save.bin"), overwrite: true);
			if (DateTime.Now - mostRecentSaveBackupTime > new TimeSpan(1, 0, 0, 0))
			{
				string text = get_save_file_path($"{DateTime.Now.ToFileTime()}.savebackup.bin");
				File.Copy(get_save_file_path("Save.temp.bin"), text);
				utils.log_write_line("Created a save backup: " + text);
				mostRecentSaveBackupTime = DateTime.Now;
			}
		}

		public static string get_default_language()
		{
			string text = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
			if (text == "pt")
			{
				text = "br";
			}
			if (text == "zh" && CultureInfo.InstalledUICulture.Name == "zh-HANT")
			{
				text = "ztpc";
			}
			return text;
		}

		public static bool is_language_forbidden(string lang)
		{
			return lang == "zt";
		}

		public static long get_process_memory()
		{
			return 0L;
		}

		private static void init_computer()
		{
		}

		public static Stream get_decompressor(Stream stream)
		{
			return new DeflateStream(stream, CompressionMode.Decompress, leaveOpen: false);
		}

		public static void main_loop_update(Action doUpdate)
		{
			asset_cache.update_on_main_thread();
			long timestamp = Stopwatch.GetTimestamp();
			frameAccumulator += timestamp - previousFrameStartTime;
			previousFrameStartTime = timestamp;
			if (frameAccumulator < targetFrameDuration)
			{
				while (frameAccumulator < targetFrameDuration)
				{
					long timestamp2 = Stopwatch.GetTimestamp();
					frameAccumulator += timestamp2 - previousFrameStartTime;
					previousFrameStartTime = timestamp2;
				}
			}
			if (_isRetailLaunchedByVisualStudio)
			{
				managedMemoryDisplayTimer++;
				if (managedMemoryDisplayTimer > 300)
				{
					managedMemoryDisplayTimer = 0;
					Console.WriteLine($"Total Managed Memory: {GC.GetTotalMemory(forceFullCollection: false) / 1000000}Mb");
				}
			}
			frameAccumulator -= targetFrameDuration;
			doUpdate();
			if (frameAccumulator >= targetFrameDuration)
			{
				frameAccumulator -= targetFrameDuration;
				doUpdate();
			}
			if (frameAccumulator >= targetFrameDuration)
			{
				frameAccumulator = 0L;
				long num = Stopwatch.GetTimestamp() - previousFrameStartTime;
				if (num <= targetFrameDuration)
				{
					doUpdate();
				}
			}
		}

		public static string get_video_folder_path()
		{
			return Path.Combine(get_data_folder(), "videos");
		}

		public static bool video_exists(string videoName)
		{
			string path = Path.Combine(get_video_folder_path(), videoName + ".ogv");
			return File.Exists(path);
		}

		public static void video_start(string videoName)
		{
			if (videoPlayer != null)
			{
				video_end();
			}
			string video_folder_path = get_video_folder_path();
			videoContentManager = new ContentManager(graphics.serviceProvider, video_folder_path);
			Video video = videoContentManager.Load<Video>(videoName);
			videoPlayer = new VideoPlayer();
			videoPlayer.Play(video);
			videoPlayer.Pause();
			videoSpriteBatch = new SpriteBatch(graphics.Device);
		}

		public static void video_draw()
		{
			graphics.Device.Textures[0] = null;
			if (videoPlayer.State == MediaState.Paused)
			{
				videoPlayer.Resume();
			}
			Texture2D texture = videoPlayer.GetTexture();
			videoSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
			videoSpriteBatch.Draw(texture, graphics.currentRenderRect, Microsoft.Xna.Framework.Color.White);
			videoSpriteBatch.End();
		}

		public static bool video_is_finished()
		{
			if (!(videoPlayer.PlayPosition >= videoPlayer.Video.Duration))
			{
				return videoPlayer.State == MediaState.Stopped;
			}
			return true;
		}

		public static TimeSpan video_get_time()
		{
			return videoPlayer.PlayPosition;
		}

		public static void video_end()
		{
			videoPlayer.Dispose();
			videoContentManager.Dispose();
			videoSpriteBatch.Dispose();
			videoPlayer = null;
			videoContentManager = null;
			videoSpriteBatch = null;
		}

		public static string get_system_info()
		{
			try
			{
				string text = get_save_file_path("dxdiag.txt");
				ProcessStartInfo processStartInfo = new ProcessStartInfo();
				processStartInfo.FileName = "dxdiag.exe";
				processStartInfo.Arguments = "/dontskip /whql:off /64bit /t \"" + text + "\"";
				Process process = Process.Start(processStartInfo);
				process.WaitForExit();
				return File.ReadAllText(text);
			}
			catch (Exception ex)
			{
				return $"Could not do dxdiag: {ex.GetType()} {ex.Message}";
			}
		}

		private static int get_achievement_id(string achievementInternalId)
		{
			for (int i = 0; i < achievementNameArray.Length; i++)
			{
				if (achievementNameArray[i] == achievementInternalId)
				{
					return i;
				}
			}
			return -1;
		}

		public static PlatformEnum get_platform()
		{
			return PlatformEnum.Computer;
		}

		public static string get_data_folder()
		{
			if (_dataFolder == null)
			{
				_dataFolder = "data";
				if (!Directory.Exists(_dataFolder))
				{
					throw new Exception("Could not find data folder: " + _dataFolder);
				}
			}
			return _dataFolder;
		}


		public static void set_data_folder (string dataFolder) {
			_dataFolder = dataFolder;
		}

		private static string convert_to_ms(long ticks)
		{
			ticks *= 100000;
			string text = (ticks / Stopwatch.Frequency).ToString("D8");
			text = text.Insert(text.Length - 2, ".");
			return text + "ms";
		}

		public static void full_garbage_collection()
		{
			long timestamp = Stopwatch.GetTimestamp();
			long totalMemory = GC.GetTotalMemory(forceFullCollection: false);
			GC.Collect(99, GCCollectionMode.Forced, blocking: true);
			GC.WaitForPendingFinalizers();
			long totalMemory2 = GC.GetTotalMemory(forceFullCollection: false);
			Console.WriteLine($"Performed Full GC for {convert_to_ms(Stopwatch.GetTimestamp() - timestamp)}, freeing {(totalMemory - totalMemory2) / 1000000}Mb");
			Console.WriteLine($"Total Managed Memory: {totalMemory2 / 1000000}Mb");
		}

		public static void start_no_garbage_collect_region()
		{
		}

		public static void end_no_garbage_collect_region()
		{
		}

		public static void update_game_pad_state_array(GamePadState_s[] gamePadStateArray)
		{
			for (int i = 0; i < 4; i++)
			{
				GamePadState state = GamePad.GetState((PlayerIndex)i, GamePadDeadZone.None);
				gamePadStateArray[i].isConnected = state.IsConnected;
				gamePadStateArray[i].a = state.IsButtonDown(Buttons.A);
				gamePadStateArray[i].b = state.IsButtonDown(Buttons.B);
				gamePadStateArray[i].x = state.IsButtonDown(Buttons.X);
				gamePadStateArray[i].y = state.IsButtonDown(Buttons.Y);
				gamePadStateArray[i].lb = state.IsButtonDown(Buttons.LeftShoulder);
				gamePadStateArray[i].rb = state.IsButtonDown(Buttons.RightShoulder);
				gamePadStateArray[i].lt = state.IsButtonDown(Buttons.LeftTrigger);
				gamePadStateArray[i].rt = state.IsButtonDown(Buttons.RightTrigger);
				gamePadStateArray[i].start = state.IsButtonDown(Buttons.Start);
				gamePadStateArray[i].back = state.IsButtonDown(Buttons.Back);
				Vec2 vec = (Vec2)state.ThumbSticks.Left;
				if (state.DPad.Right == ButtonState.Pressed)
				{
					vec.x = 1f;
				}
				if (state.DPad.Left == ButtonState.Pressed)
				{
					vec.x = -1f;
				}
				if (state.DPad.Up == ButtonState.Pressed)
				{
					vec.y = 1f;
				}
				if (state.DPad.Down == ButtonState.Pressed)
				{
					vec.y = -1f;
				}
				float f = vec.normalize_and_get_magnitude_can_be_zero();
				f = f.clamp_and_get_cursor(0.6f, 0.9f);
				bool flag = vec.x < 0f;
				float num = vec.get_angle();
				if (flag)
				{
					num = ((!(num > 0f)) ? (-(float)Math.PI - num) : ((float)Math.PI - num));
				}
				if (num.abs() < (float)Math.PI / 2f)
				{
					num = num.sign() * num.abs().clamp_and_get_cursor(0.2f, 1.37079632f);
					num *= (float)Math.PI / 2f;
				}
				vec = Vec2.new_direction(num);
				if (vec.x.abs() < 0.0001f)
				{
					vec.x = 0f;
				}
				if (vec.y.abs() < 0.0001f)
				{
					vec.y = 0f;
				}
				if (flag)
				{
					vec.x *= -1f;
				}
				vec *= f;
				gamePadStateArray[i].stickX = vec.x;
				gamePadStateArray[i].stickY = vec.y;
			}
		}

		public static void set_gamepad_vibration(int controllerId, float leftMotor, float rightMotor)
		{
			if (controllerId < 0 || controllerId > 3)
			{
				return;
			}
			try
			{
				GamePad.SetVibration((PlayerIndex)controllerId, leftMotor, rightMotor);
			}
			catch (Exception ex)
			{
				if (!didSetVibrationThrowExceptionOnce)
				{
					didSetVibrationThrowExceptionOnce = true;
					utils.log_write_line($"GamePad.SetVibration threw an Exception {ex.GetType()} {ex.Message}", LogImportanceEnum.error);
				}
			}
		}

		public static void set_gamepad_info(int controllerId, int playerId, Color_s color)
		{
		}

		private static void log_lobby_properties()
		{
			utils.log_write_line("online", $"Lobby Type: {Client.Instance.Lobby.LobbyType}, Owner: {Client.Instance.Lobby.Owner}, Members {Client.Instance.Lobby.NumMembers}/{Client.Instance.Lobby.MaxMembers}");
		}

		public static bool check_drm_and_init()
		{
			init_computer();
			if (Client.RestartIfNecessary(985890u))
			{
				return true;
			}
			new Client(985890u);
			if (Client.Instance != null)
			{
				Client.Instance.Lobby.OnLobbyCreated = delegate(bool success)
				{
					if (success)
					{
						utils.log_write_line("online", "lobby created callback (success): " + Client.Instance.Lobby.CurrentLobby);
						log_lobby_properties();
						if (hostLobbyMetadata != null)
						{
							foreach (KeyValuePair<string, string> hostLobbyMetadatum in hostLobbyMetadata)
							{
								Client.Instance.Lobby.CurrentLobbyData.SetData(hostLobbyMetadatum.Key, hostLobbyMetadatum.Value);
							}
						}
					}
					else
					{
						globalLobbyState = GlobalLobbyStateEnum.offline;
						utils.log_write_line("online", "lobby created callback (failed)");
					}
				};
				LobbyList lobbyList = Client.Instance.LobbyList;
				lobbyList.OnLobbiesUpdated = (Action)Delegate.Combine(lobbyList.OnLobbiesUpdated, (Action)delegate
				{
					utils.log_write_line("online", $"joinable lobbies updated: {Client.Instance.LobbyList.Lobbies.Count} lobbie(s)");
					LobbyData_s value2 = default(LobbyData_s);
					foreach (LobbyList.Lobby lobby7 in Client.Instance.LobbyList.Lobbies)
					{
						value2.metadataDict = new Dictionary<string, string>();
						foreach (KeyValuePair<string, string> allDatum in lobby7.GetAllData())
						{
							value2.metadataDict[allDatum.Key.to_lower()] = allDatum.Value;
						}
						lobbyDataByLobbyId[lobby7.LobbyID] = value2;
					}
				});
				Lobby lobby = Client.Instance.Lobby;
				lobby.OnLobbyJoinRequested = (Action<ulong>)Delegate.Combine(lobby.OnLobbyJoinRequested, (Action<ulong>)delegate(ulong lobbyId)
				{
					utils.log_write_line("online", $"accepted invite to lobby {lobbyId}");
					globalLobbyState = GlobalLobbyStateEnum.offline;
				});
				Lobby lobby2 = Client.Instance.Lobby;
				lobby2.OnLobbyJoined = (Action<bool>)Delegate.Combine(lobby2.OnLobbyJoined, (Action<bool>)delegate(bool succeeded)
				{
					if (succeeded)
					{
						utils.log_write_line("online", $"Lobby joined callback (success): {Client.Instance.Lobby.CurrentLobby}");
						log_lobby_properties();
					}
					else
					{
						utils.log_write_line("online", $"Lobby joined callback (failed): {Client.Instance.Lobby.CurrentLobby}");
						globalLobbyState = GlobalLobbyStateEnum.offline;
					}
				});
				Lobby lobby3 = Client.Instance.Lobby;
				lobby3.OnUserInvitedToLobby = (Action<ulong, ulong>)Delegate.Combine(lobby3.OnUserInvitedToLobby, (Action<ulong, ulong>)delegate(ulong lobbyId, ulong userId)
				{
					utils.log_write_line("online", $"sending invite to user {userId} for lobby {lobbyId}");
				});
				Lobby lobby4 = Client.Instance.Lobby;
				lobby4.OnLobbyStateChanged = (Action<Lobby.MemberStateChange, ulong, ulong>)Delegate.Combine(lobby4.OnLobbyStateChanged, (Action<Lobby.MemberStateChange, ulong, ulong>)delegate(Lobby.MemberStateChange change, ulong source, ulong dest)
				{
					utils.log_write_line("online", $"lobby: user {source} initiated {change}, user {dest} was affected.");
				});
				Lobby lobby5 = Client.Instance.Lobby;
				lobby5.OnLobbyDataUpdated = (Action)Delegate.Combine(lobby5.OnLobbyDataUpdated, (Action)delegate
				{
					Lobby lobby6 = Client.Instance.Lobby;
					LobbyData_s value = default(LobbyData_s);
					value.metadataDict = new Dictionary<string, string>();
					foreach (KeyValuePair<string, string> allDatum2 in lobby6.CurrentLobbyData.GetAllData())
					{
						value.metadataDict[allDatum2.Key.to_lower()] = allDatum2.Value;
					}
					lobbyDataByLobbyId[lobby6.CurrentLobby] = value;
				});
				Client.Instance.Networking.OnP2PData = delegate(ulong steamid, byte[] bytes, int length, int channel)
				{
					byte[] array = new byte[length];
					Array.Copy(bytes, array, length);
					NetworkData_s item = default(NetworkData_s);
					try
					{
						item = (NetworkData_s)reflection.bin_deserialize(array, typeof(NetworkData_s));
					}
					catch (Exception ex)
					{
						utils.log_write_line("online", "Received corrupted NetworkData_s with error: " + ex.Message, LogImportanceEnum.error);
					}
					if (item.is_valid() && item.sentData != null)
					{
						receivedNetworkDataArray.Add(item);
					}
				};
				Client.Instance.Networking.OnIncomingConnection = delegate(ulong steamid)
				{
					utils.log_write_line("online", "incoming connection from: " + steamid);
					return true;
				};
				Client.Instance.Networking.OnConnectionFailed = delegate(ulong steamid, Networking.SessionError error)
				{
					utils.log_write_line("online", "Connection failed with: " + steamid);
				};
				Client.Instance.Networking.SetListenChannel(0, Listen: true);
			}
			return false;
		}

		public static void receive_online_data()
		{
			if (isLoggedOnCheckCounter <= 0)
			{
				isLoggedOnCheckCounter = 60;
				isLoggedOn = Client.Instance != null && Client.Instance.IsLoggedOn;
			}
			else
			{
				isLoggedOnCheckCounter--;
			}
			_byteSentPerFrame = 0;
			if (Client.Instance == null || !Client.Instance.IsValid)
			{
				return;
			}
			if (Client.Instance.IsValid)
			{
				if (runCallbacksCounter <= 0)
				{
					long timestamp = Stopwatch.GetTimestamp();
					Client.Instance.RunCallbacks();
					long num = Stopwatch.GetTimestamp() - timestamp;
					float num2 = (float)num / (float)Stopwatch.Frequency * 1000f;
					if (num2 > 8f)
					{
						utils.log_write_line("Steamworks", $"SteamAPI_RunCallbacks is slow, took {num2}", LogImportanceEnum.error);
						runCallbacksCounter = 60;
					}
					else
					{
						runCallbacksCounter = 10;
					}
				}
				else
				{
					runCallbacksCounter--;
				}
				Client.Instance.Voice.Update();
				Client.Instance.Friends.Cycle();
				Client.Instance.Networking.Update();
				Client.Instance.RunUpdateCallbacks();
			}
			if (debugSimulatePlayTogether != 0)
			{
				switch (debugSimulatePlayTogether)
				{
				case SimulatePlayTogetherEnum.createLobbyCalled:
					if (Client.Instance.Lobby.CurrentLobby != 0L)
					{
						utils.log_write_line("online", $"debug play together host lobby created {Client.Instance.Lobby.CurrentLobby}");
						Client.Instance.Lobby.OpenFriendInviteOverlay();
						debugSimulatePlayTogether = SimulatePlayTogetherEnum.friendInviteSent;
					}
					break;
				case SimulatePlayTogetherEnum.friendInviteSent:
				{
					ulong[] memberIDs = Client.Instance.Lobby.GetMemberIDs();
					if (memberIDs != null && memberIDs.Length >= 2)
					{
						utils.log_write_line("online", $"debug play together lobby full {Client.Instance.Lobby.CurrentLobby}");
						utils.log_write_line("online", "Going directly to globalLobbyState=host to simulate play together.");
						debugSimulatePlayTogether = SimulatePlayTogetherEnum.lobbyIsFull;
						globalLobbyState = GlobalLobbyStateEnum.host;
					}
					break;
				}
				case SimulatePlayTogetherEnum.lobbyIsFull:
					if (Client.Instance.Lobby.CurrentLobby == 0L)
					{
						utils.log_write_line("online", "debug play together no lobby");
						debugSimulatePlayTogether = SimulatePlayTogetherEnum.none;
					}
					break;
				}
			}
			else if (Client.Instance.Lobby.CurrentLobby != 0L && online_service_is_connected())
			{
				switch (globalLobbyState)
				{
				case GlobalLobbyStateEnum.offline:
					utils.log_write_line("online", $"Went directly from offline to being in a client lobby {Client.Instance.Lobby.CurrentLobby}, should be because of an accepted invitation.", LogImportanceEnum.warning);
					globalLobbyState = GlobalLobbyStateEnum.client;
					break;
				case GlobalLobbyStateEnum.connectingAsHost:
					utils.log_write_line("online", $"is in host lobby {Client.Instance.Lobby.CurrentLobby}");
					globalLobbyState = GlobalLobbyStateEnum.host;
					if (Client.Instance.Lobby.LobbyType == Lobby.Type.Private)
					{
						start_friend_invite();
					}
					break;
				case GlobalLobbyStateEnum.connectingAsClient:
					utils.log_write_line("online", $"is in client lobby {Client.Instance.Lobby.CurrentLobby}");
					globalLobbyState = GlobalLobbyStateEnum.client;
					break;
				}
			}
			else
			{
				if (Client.Instance.Lobby.CurrentLobby != 0L)
				{
					Client.Instance.Lobby.Leave();
				}
				switch (globalLobbyState)
				{
				case GlobalLobbyStateEnum.host:
					utils.log_write_line("online", "was host, not in lobby, resetting the global lobby state", LogImportanceEnum.error);
					globalLobbyState = GlobalLobbyStateEnum.offline;
					break;
				case GlobalLobbyStateEnum.client:
					utils.log_write_line("online", "was client, not in lobby, resetting the global lobby state", LogImportanceEnum.error);
					globalLobbyState = GlobalLobbyStateEnum.offline;
					break;
				case GlobalLobbyStateEnum.connectingAsHost:
					if (!online_service_is_connected())
					{
						utils.log_write_line("online", "was connecting as host, online services are disconnected, resetting the global lobby state", LogImportanceEnum.error);
						globalLobbyState = GlobalLobbyStateEnum.offline;
					}
					break;
				case GlobalLobbyStateEnum.connectingAsClient:
					if (!online_service_is_connected())
					{
						utils.log_write_line("online", "was connecting as client, online services are disconnected, resetting the global lobby state", LogImportanceEnum.error);
						globalLobbyState = GlobalLobbyStateEnum.offline;
					}
					break;
				}
			}
			if (!didJoinStartupLobby)
			{
				didJoinStartupLobby = true;
				string[] commandLineArgs = Environment.GetCommandLineArgs();
				for (int i = 0; i < commandLineArgs.Length; i++)
				{
					if (commandLineArgs[i] == "+connect_lobby")
					{
						Client.Instance.Lobby.Join(ulong.Parse(commandLineArgs[i + 1]));
						break;
					}
				}
			}
			update_leaderboard_requests();
		}

		public static void shutdown()
		{
			Client.Instance?.Dispose();
		}

		public static void update()
		{
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static bool online_service_is_connected()
		{
			return isLoggedOn;
		}

		public static string get_machine_display_name(MachineId_s machineId)
		{
			if (!online_service_is_connected())
			{
				return machineId.ToString();
			}
			return Client.Instance.GetSteamIdDisplayName(machineId.machineId);
		}

		public static void set_achievement(string achievementInternalId, int playerId, int controllerId)
		{
			int num = get_achievement_id(achievementInternalId);
			if (num < 0)
			{
				throw new Exception("Invalid achievement internal id");
			}
			Client.Instance?.Achievements?.Trigger((num + 1).to_string());
		}

		public static void show_gamer_profile(MachineId_s machine)
		{
			Client.Instance?.Overlay?.OpenProfile(machine.machineId);
		}

		public static void reset_stats()
		{
			Client.Instance?.Stats?.ResetAll(includeAchievements: true);
		}

		public static void set_rich_presence(string key, string value)
		{
		}

		public static void stat_add(string statId, int addedValue)
		{
		}

		public static void stat_add(string statId, float addedValue)
		{
		}

		public static void stat_max(string statId, int newValue)
		{
		}

		public static void stat_max(string statId, float newValue)
		{
		}

		public static void post_score_to_leaderboard(string leaderboardId, int score)
		{
			if (online_service_is_connected())
			{
				Leaderboard leaderboard = Client.Instance.GetLeaderboard(leaderboardId, Client.LeaderboardSortMethod.Descending, Client.LeaderboardDisplayType.Numeric);
				leaderboard.AddScore(onlyIfBeatsOldScore: true, score);
			}
		}

		private static bool find_leaderboard_request(string leaderboardId, out SteamLeaderboardRequest content)
		{
			return leaderboardRequestDict.TryGetValue(leaderboardId, out content);
		}

		private static void add_leaderboard_request(SteamLeaderboardRequest content)
		{
			if (find_leaderboard_request(content.leaderboardId, out var _))
			{
				remove_leaderboard_request(content.leaderboardId);
			}
			leaderboardRequestDict.Add(content.leaderboardId, content);
		}

		private static void remove_leaderboard_request(string leaderboardId)
		{
			if (leaderboardRequestDict.TryGetValue(leaderboardId, out var _))
			{
				leaderboardRequestDict.Remove(leaderboardId);
			}
		}

		public static void start_get_leaderboard_content(string leaderboardId, int worldBestCount, int worldAroundLocalCount, int friendBestCount)
		{
			if (online_service_is_connected() && !find_leaderboard_request(leaderboardId, out var content))
			{
				content = new SteamLeaderboardRequest();
				content.timeoutInSeconds = defaultLeaderboardRequestTimeout;
				content.leaderboardId = leaderboardId;
				content.worldBestLeaderboard = Client.Instance.GetLeaderboard(leaderboardId, Client.LeaderboardSortMethod.Descending, Client.LeaderboardDisplayType.Numeric);
				content.worldAroundLocalLeaderboard = Client.Instance.GetLeaderboard(leaderboardId, Client.LeaderboardSortMethod.Descending, Client.LeaderboardDisplayType.Numeric);
				content.friendBestLeaderboard = Client.Instance.GetLeaderboard(leaderboardId, Client.LeaderboardSortMethod.Descending, Client.LeaderboardDisplayType.Numeric);
				content.worldBestCount = worldBestCount;
				content.worldAroundLocalCount = worldAroundLocalCount;
				content.friendAroundLocalCount = friendBestCount;
				content.requestedLeaderBoardStatus = RequestStatusEnum.pending;
				add_leaderboard_request(content);
			}
		}

		public static RequestStatusEnum get_requested_leaderBoard_status(string leaderboardId)
		{
			if (!find_leaderboard_request(leaderboardId, out var content))
			{
				return RequestStatusEnum.inactive;
			}
			return content.requestedLeaderBoardStatus;
		}

		private static void update_leaderboard_requests()
		{
			if (!online_service_is_connected())
			{
				foreach (KeyValuePair<string, SteamLeaderboardRequest> item in leaderboardRequestDict)
				{
					item.Value.requestedLeaderBoardStatus = RequestStatusEnum.fail;
				}
				return;
			}
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, SteamLeaderboardRequest> item2 in leaderboardRequestDict)
			{
				SteamLeaderboardRequest value = item2.Value;
				value.timeoutInSeconds -= 0.0166666675f;
				if (value.timeoutInSeconds < 0f)
				{
					list.Add(value.leaderboardId);
					continue;
				}
				bool flag = false;
				if (value.worldBestLeaderboard.IsValid)
				{
					if (!value.worldBestRequested)
					{
						bool flag2 = value.worldBestLeaderboard.FetchScores(Leaderboard.RequestType.Global, 0, value.worldBestCount);
						value.worldBestRequested = true;
						flag = true;
					}
					else
					{
						flag |= value.worldBestLeaderboard.IsQuerying || value.worldBestLeaderboard.Results == null;
					}
				}
				else
				{
					flag = true;
				}
				if (value.worldAroundLocalLeaderboard.IsValid)
				{
					if (!value.worldAroundLocalRequested)
					{
						bool flag3 = value.worldAroundLocalLeaderboard.FetchScores(Leaderboard.RequestType.GlobalAroundUser, -value.worldAroundLocalCount, value.worldAroundLocalCount);
						value.worldAroundLocalRequested = true;
						flag = true;
					}
					else
					{
						flag |= value.worldAroundLocalLeaderboard.IsQuerying || value.worldAroundLocalLeaderboard.Results == null;
					}
				}
				else
				{
					flag = true;
				}
				if (value.friendBestLeaderboard.IsValid)
				{
					if (!value.friendBestRequested)
					{
						bool flag4 = value.friendBestLeaderboard.FetchScores(Leaderboard.RequestType.Friends, 0, value.friendBestCount);
						value.friendBestRequested = true;
						flag = true;
					}
					else
					{
						flag |= value.friendBestLeaderboard.IsQuerying || value.friendBestLeaderboard.Results == null;
					}
				}
				else
				{
					flag = true;
				}
				if (flag)
				{
					value.requestedLeaderBoardStatus = RequestStatusEnum.pending;
					continue;
				}
				if (value.worldBestLeaderboard.IsError || value.worldAroundLocalLeaderboard.IsError || value.friendBestLeaderboard.IsError)
				{
					value.requestedLeaderBoardStatus = RequestStatusEnum.fail;
					continue;
				}
				value.requestedLeaderBoardStatus = RequestStatusEnum.success;
				get_leaderboard_content_result(value.leaderboardId, out var _);
			}
			for (int i = 0; i < list.Count; i++)
			{
				leaderboardRequestDict.Remove(list[i]);
			}
		}

		public static bool get_leaderboard_content_result(string leaderboardId, out LeaderboardContent_s result)
		{
			if (!online_service_is_connected())
			{
				result = default(LeaderboardContent_s);
				return false;
			}
			if (!find_leaderboard_request(leaderboardId, out var content) || content.requestedLeaderBoardStatus != RequestStatusEnum.success)
			{
				result = default(LeaderboardContent_s);
				return false;
			}
			if (content.worldBestLeaderboard != null)
			{
				array.resize(ref content.leaderboardContentCache.worldBest, content.worldBestLeaderboard.Results.Length);
				for (int i = 0; i < content.leaderboardContentCache.worldBest.Length; i++)
				{
					content.leaderboardContentCache.worldBest[i].machineId.machineId = content.worldBestLeaderboard.Results[i].SteamId;
					content.leaderboardContentCache.worldBest[i].rank = content.worldBestLeaderboard.Results[i].GlobalRank;
					content.leaderboardContentCache.worldBest[i].score = content.worldBestLeaderboard.Results[i].Score;
				}
			}
			if (content.worldAroundLocalLeaderboard != null)
			{
				array.resize(ref content.leaderboardContentCache.worldAroundLocal, content.worldAroundLocalLeaderboard.Results.Length);
				for (int j = 0; j < content.leaderboardContentCache.worldAroundLocal.Length; j++)
				{
					content.leaderboardContentCache.worldAroundLocal[j].machineId.machineId = content.worldAroundLocalLeaderboard.Results[j].SteamId;
					content.leaderboardContentCache.worldAroundLocal[j].rank = content.worldAroundLocalLeaderboard.Results[j].GlobalRank;
					content.leaderboardContentCache.worldAroundLocal[j].score = content.worldAroundLocalLeaderboard.Results[j].Score;
				}
			}
			if (content.friendBestLeaderboard != null)
			{
				array.resize(ref content.leaderboardContentCache.friendBest, content.friendBestLeaderboard.Results.Length);
				for (int k = 0; k < content.leaderboardContentCache.friendBest.Length; k++)
				{
					content.leaderboardContentCache.friendBest[k].machineId.machineId = content.friendBestLeaderboard.Results[k].SteamId;
					content.leaderboardContentCache.friendBest[k].rank = content.friendBestLeaderboard.Results[k].GlobalRank;
					content.leaderboardContentCache.friendBest[k].score = content.friendBestLeaderboard.Results[k].Score;
				}
			}
			result = content.leaderboardContentCache;
			return true;
		}

		public static void start_create_host_lobby(int onlineOpenSlotCount, LobbyTypeEnum lobbyType, Dictionary<string, string> metadataDict)
		{
			foreach (KeyValuePair<string, string> item in metadataDict)
			{
			}
			if (online_service_is_connected() && globalLobbyState == GlobalLobbyStateEnum.offline)
			{
				leave_lobby();
				Lobby.Type lobbyType2 = Lobby.Type.Error;
				switch (lobbyType)
				{
				case LobbyTypeEnum.publicLobby:
					lobbyType2 = Lobby.Type.Public;
					break;
				case LobbyTypeEnum.privateLobby:
					lobbyType2 = Lobby.Type.Private;
					break;
				}
				hostLobbyMetadata = metadataDict;
				globalLobbyState = GlobalLobbyStateEnum.connectingAsHost;
				Client.Instance.Lobby.Create(lobbyType2, onlineOpenSlotCount + 1);
			}
		}

		public static void close_host_lobby()
		{
			if (online_service_is_connected() && globalLobbyState == GlobalLobbyStateEnum.host)
			{
				Client.Instance.Lobby.MaxMembers = Client.Instance.Lobby.NumMembers;
			}
		}

		public static bool is_host_lobby_closed()
		{
			if (!online_service_is_connected() || globalLobbyState != GlobalLobbyStateEnum.host)
			{
				return false;
			}
			return Client.Instance.Lobby.MaxMembers == Client.Instance.Lobby.NumMembers;
		}

		public static void leave_lobby()
		{
			globalLobbyState = GlobalLobbyStateEnum.offline;
			if (online_service_is_connected() && Client.Instance != null && Client.Instance.Lobby != null && Client.Instance.Lobby.CurrentLobby != 0L)
			{
				utils.log_write_line("online", "platform leave lobby called");
				Client.Instance.Lobby.Leave();
			}
		}

		public static LobbyId_s get_current_lobby()
		{
			return new LobbyId_s(Client.Instance.Lobby.CurrentLobby);
		}

		public static MachineId_s[] get_host_lobby_members()
		{
			if (!online_service_is_connected())
			{
				return null;
			}
			ulong[] memberIDs = Client.Instance.Lobby.GetMemberIDs();
			int num = 0;
			for (int i = 0; i < memberIDs.Length; i++)
			{
				if (memberIDs[i] != 0L)
				{
					num++;
				}
			}
			MachineId_s[] array = new MachineId_s[num];
			for (int j = 0; j < memberIDs.Length; j++)
			{
				if (memberIDs[j] != 0L)
				{
					array[j] = new MachineId_s(memberIDs[j]);
				}
			}
			return array;
		}

		public static void start_refresh_lobby_list(Dictionary<string, string> stringMetadataFilters, int maxResults)
		{
			if (!online_service_is_connected())
			{
				return;
			}
			LobbyList.Filter filter = new LobbyList.Filter();
			filter.StringFilters.Add("appid", Client.Instance.AppId.ToString());
			filter.MaxResults = maxResults;
			if (stringMetadataFilters != null)
			{
				foreach (KeyValuePair<string, string> stringMetadataFilter in stringMetadataFilters)
				{
					filter.StringFilters.Add(stringMetadataFilter.Key, stringMetadataFilter.Value);
				}
			}
			utils.log_write_line("online", "start refresh lobby list ");
			Client.Instance.LobbyList.Refresh(filter);
		}

		public static bool refresh_lobby_list_is_finished()
		{
			if (!online_service_is_connected())
			{
				return true;
			}
			return Client.Instance.LobbyList.Finished;
		}

		public static int get_joinable_lobby_count()
		{
			if (!online_service_is_connected())
			{
				return 0;
			}
			return Client.Instance.LobbyList.Lobbies.Count;
		}

		public static LobbyId_s get_joinable_lobby(int lobbyIndex)
		{
			if (!online_service_is_connected())
			{
				return new LobbyId_s(0uL);
			}
			LobbyList.Lobby lobby = Client.Instance.LobbyList.Lobbies[lobbyIndex];
			return new LobbyId_s(lobby.LobbyID);
		}

		public static void start_join_lobby(LobbyId_s lobby)
		{
			utils.log_write_line("online", $"start join lobby {lobby}");
			if (LobbyIsConnecting)
			{
				utils.log_write_line("online", $"called start join lobby {lobby} while lobby was connecting", LogImportanceEnum.warning);
				return;
			}
			globalLobbyState = GlobalLobbyStateEnum.connectingAsClient;
			Client.Instance.Lobby.Join(lobby.innerId);
		}

		public static void start_friend_invite()
		{
			if (Client.Instance != null && Client.Instance.Lobby != null)
			{
				Client.Instance.Lobby.OpenFriendInviteOverlay();
			}
		}

		public static int controller_id_that_accepted_lobby_invite()
		{
			for (int i = 0; i < 4; i++)
			{
				if (GamePad.GetState((PlayerIndex)i).IsConnected)
				{
					return i;
				}
			}
			return 4;
		}

		public static void debug_start_simulate_play_together()
		{
			if (debugSimulatePlayTogether == SimulatePlayTogetherEnum.none)
			{
				utils.log_write_line("online", "debug start simulate play together");
				Client.Instance.Lobby.Create(Lobby.Type.Private, 2);
				debugSimulatePlayTogether = SimulatePlayTogetherEnum.createLobbyCalled;
			}
		}

		public static int controller_id_that_started_play_together()
		{
			for (int i = 0; i < 4; i++)
			{
				if (GamePad.GetState((PlayerIndex)i).IsConnected)
				{
					return i;
				}
			}
			return 4;
		}

		public static void send_data(MachineId_s target, byte[] data, int dataLength, SendDataTypeEnum sendType)
		{
			if (online_service_is_connected())
			{
				Networking.SendType eP2PSendType = Networking.SendType.Reliable;
				switch (sendType)
				{
				case SendDataTypeEnum.reliable:
					eP2PSendType = Networking.SendType.Reliable;
					break;
				case SendDataTypeEnum.unreliable:
					eP2PSendType = Networking.SendType.UnreliableNoDelay;
					break;
				}
				DateTime now = DateTime.Now;
				NetworkData_s networkData_s = new NetworkData_s(target, data, dataLength, now.Second, now.Millisecond);
				byte[] array = reflection.bin_serialize_to_memory(networkData_s);
				if (!Client.Instance.Networking.SendP2PPacket(target.machineId, array, array.Length, eP2PSendType))
				{
					utils.log_write_line("Online", $"Failed to send p2p data to: {Client.Instance.Lobby.Owner}", LogImportanceEnum.error);
				}
				else
				{
					_byteSentPerFrame += array.Length;
				}
			}
		}

		public static bool read_data(out NetworkData_s netData)
		{
			if (receivedNetworkDataArray.Count > 0)
			{
				netData = receivedNetworkDataArray[0];
				receivedNetworkDataArray.RemoveAt(0);
				return true;
			}
			netData = default(NetworkData_s);
			return false;
		}

		public static void start_get_user_profile(MachineId_s machineId)
		{
			if (!online_service_is_connected() || find_user_profile_request(machineId, out var _))
			{
				return;
			}
			if (userProfileRequestByMachine.Count >= maxUserProfileRequestCount)
			{
				Random random = new Random();
				MachineId_s key = userProfileRequestByMachine.ElementAt(random.Next(0, userProfileRequestByMachine.Count)).Key;
				userProfileRequestByMachine.Remove(key);
			}
			UserProfileRequest newRequest = new UserProfileRequest();
			newRequest.machineId = machineId;
			newRequest.requestStatus = RequestStatusEnum.pending;
			userProfileRequestByMachine.Add(machineId, newRequest);
			Client.Instance.Friends.GetAvatar(Friends.AvatarSize.Medium, machineId.machineId, delegate(Image avatar)
			{
				ulong machineId2 = newRequest.machineId.machineId;
				UserProfileRequest content2;
				if (avatar == null)
				{
					utils.log_write_line("Online", "No Avatar for steam id: " + machineId2);
				}
				else if (find_user_profile_request(machineId, out content2))
				{
					content2.requestStatus = RequestStatusEnum.success;
					content2.avatarImage = avatar;
					newRequest.userProfile = new UserProfile();
					newRequest.userProfile.machineId = machineId;
					newRequest.userProfile.displayName = get_machine_display_name(machineId);
					newRequest.userProfile.avatarTexture = new Texture2D(graphics.Device, content2.avatarImage.Width, content2.avatarImage.Height);
					newRequest.userProfile.avatarTexture.SetData(content2.avatarImage.Data);
				}
				else
				{
					utils.log_write_line("Online", "User request not found");
				}
			});
		}

		private static bool find_user_profile_request(MachineId_s machineId, out UserProfileRequest content)
		{
			if (!online_service_is_connected())
			{
				content = null;
				return false;
			}
			return userProfileRequestByMachine.TryGetValue(machineId, out content);
		}

		public static RequestStatusEnum get_user_profile_request_status(MachineId_s machineId)
		{
			if (!online_service_is_connected() || !find_user_profile_request(machineId, out var content))
			{
				return RequestStatusEnum.fail;
			}
			return content.requestStatus;
		}

		public static bool get_user_profile(MachineId_s machineId, out UserProfile profile)
		{
			if (!online_service_is_connected() || !find_user_profile_request(machineId, out var content))
			{
				profile = null;
				return false;
			}
			profile = content.userProfile;
			return true;
		}

		public static string get_iso_code(string languageCode)
		{
			return languageCode switch
			{
				"ztpc" => "zh-hant", 
				"zt" => "zh-hant", 
				"zh" => "zh-hans", 
				"br" => "pt", 
				_ => languageCode, 
			};
		}

		private static void shape_text(Face face, string languageCode, string text, out GlyphInfo[] glyphInfos, out GlyphPosition[] glyphPositions)
		{
			using SharpFont.HarfBuzz.Buffer buffer = new SharpFont.HarfBuzz.Buffer();
			buffer.ClusterLevel = ClusterLevel.Characters;
			buffer.Direction = Direction.LeftToRight;
			buffer.Script = languageCode switch
			{
				"zhpc" => Script.Han, 
				"zt" => Script.Han, 
				"zh" => Script.Han, 
				"ru" => Script.Cyrillic, 
				"ko" => Script.Hangul, 
				"ja" => Script.Han, 
				_ => Script.Latin, 
			};
			buffer.Language = get_iso_code(languageCode);
			buffer.AddText(text);
			using (Font font = Font.FromFTFace(face))
			{
				font.Shape(buffer);
			}
			glyphInfos = buffer.GlyphInfo();
			glyphPositions = buffer.GlyphPositions();
		}

		public unsafe static void render_text_to_texture(Texture2D texture, string text, FontDefinition_s fontDef, string languageCode, IntVec2 textureSize, float textSize, TextFlagEnum flags, List<Vec3> imageList)
		{
			array.ensure_capacity(ref _textBitmapBuffer, textureSize.x * textureSize.y);
			Array.Clear(_textBitmapBuffer, 0, textureSize.x * textureSize.y);
			if (fontLib == null)
			{
				fontLib = new Library();
			}
			if (!faceByFontDef.TryGetValue(fontDef, out var value))
			{
				string filePath = fontDef.filePath;
				value = new Face(fontLib, filePath, -1);
				value = new Face(fontLib, filePath, (fontDef.subFont < value.FaceCount) ? fontDef.subFont : 0);
				faceByFontDef.Add(fontDef, value);
			}
			value.SetCharSize(0, textSize, 72u, 0u);
			SizeMetrics metrics = value.Size.Metrics;
			shape_text(value, languageCode, text, out var glyphInfos, out var glyphPositions);
			_spanList.Clear();
			int num = 0;
			Fixed26Dot6 fixed26Dot = 0;
			IntVec2 zero = IntVec2.zero;
			int num2 = 0;
			for (int i = 0; i < glyphInfos.Length; i++)
			{
				int cluster = (int)glyphInfos[i].cluster;
				char c = ((cluster < text.Length) ? text[cluster] : 'X');
				bool flag = false;
				if (!imageList.is_null_or_empty() && c == '[' && cluster + 2 < text.Length && text[cluster + 2] == ']' && text[cluster + 1] >= '0' && text[cluster + 1] <= '9')
				{
					int num3 = text[cluster + 1] - 48;
					if (num3 < imageList.Count)
					{
						fixed26Dot += (Fixed26Dot6)((float)textureSize.x * imageList[num3].z);
						i += 2;
						flag = true;
					}
				}
				if (!flag && c != '\n')
				{
					fixed26Dot += glyphPositions[i].Advance.X;
				}
				if (i == glyphInfos.Length - 1 || c == '\n' || c == ' ')
				{
					zero.x = i + 1 - num2;
					zero.y = (int)fixed26Dot;
				}
				if (i != glyphInfos.Length - 1 && c != '\n' && ((int)fixed26Dot <= textureSize.x || !flags.HasFlag(TextFlagEnum.wrap)))
				{
					continue;
				}
				if (zero.x == 0)
				{
					int num4 = i;
					int cluster2 = (int)glyphInfos[num4].cluster;
					while (num4 > num2 && ("$(£¥·'\"〈《「『【〔〖〝﹙﹛＄（．［｛￡￥([{£¥'\"‵〈《「『〔〝\ufe34﹙﹛（｛︵︷︹︻︽︿﹁﹃\ufe4f([｛〔〈《「『【〘〖〝'\"｟«$([\\{£¥'\"々〇〉》」〔＄（［｛｠￥￦ #".contains(text[cluster2]) || (cluster2 + 1 < text.Length && "!%),.:;?]}¢°·'\"\"†‡›℃∶、。〃〆〕〗〞﹚﹜！＂％＇），．：；？！］｝～!),.:;?]}¢·–— '\"•\" 、。〆〞〕〉》」︰︱︲\ufe33﹐﹑﹒\ufe53﹔﹕﹖﹘﹚﹜！），．：；？︶︸︺︼︾﹀﹂﹗］｜｝､)]｝〕〉》」』】〙〗〟'\"｠»ヽヾーァィゥェォッャュョヮヵヶぁぃぅぇぉっゃゅょゎゕゖㇰㇱㇲㇳㇴㇵㇶㇷㇸㇹㇺㇻㇼㇽㇾㇿ々〻‐゠–〜? ! ‼ ⁇ ⁈ ⁉・、:;,。.!%),.:;?]}¢°'\"†‡℃〆〈《「『〕！％），．：；？］｝".contains(text[cluster2 + 1]))))
					{
						num4--;
						cluster2 = (int)glyphInfos[num4].cluster;
					}
					zero.x = num4 + 1 - num2;
					zero.y = (int)fixed26Dot;
				}
				num = math.max(num, zero.y);
				_spanList.Add(zero);
				num2 += zero.x;
				fixed26Dot -= (Fixed26Dot6)zero.y;
				zero = IntVec2.zero;
			}
			float num5 = math.min((float)textureSize.y / (float)(_spanList.Count * metrics.Height), (float)textureSize.x / (float)num, 1f);
			if (num5 < 1f)
			{
				textSize *= num5;
				value.SetCharSize(0, textSize, 72u, 0u);
				metrics = value.Size.Metrics;
				shape_text(value, languageCode, text, out glyphInfos, out glyphPositions);
				for (int j = 0; j < _spanList.Count; j++)
				{
					IntVec2 value2 = _spanList[j];
					value2.y = (int)((float)value2.y * num5);
					_spanList[j] = value2;
				}
			}
			Fixed26Dot6 height = metrics.Height;
			FTVector26Dot6 fTVector26Dot = new FTVector26Dot6(0, metrics.Ascender);
			fTVector26Dot.Y += (flags.HasFlag(TextFlagEnum.bottom) ? (textureSize.y - height * _spanList.Count) : (flags.HasFlag(TextFlagEnum.middle) ? ((textureSize.y - height * _spanList.Count) / 2) : ((Fixed26Dot6)0)));
			int k = 0;
			foreach (IntVec2 span in _spanList)
			{
				float num6 = 1f;
				fTVector26Dot.X = (flags.HasFlag(TextFlagEnum.right) ? (textureSize.x - span.y) : (flags.HasFlag(TextFlagEnum.center) ? ((textureSize.x - span.y) / 2) : 0));
				if (flags.HasFlag(TextFlagEnum.justify) && span != _spanList.get_last())
				{
					num6 = (float)textureSize.x / (float)span.y;
				}
				for (int l = 0; l < span.x && k < glyphInfos.Length; l++, k++)
				{
					int cluster3 = (int)glyphInfos[k].cluster;
					if (glyphInfos[k].codepoint == 0 && text[cluster3] == '\n')
					{
						continue;
					}
					if (!imageList.is_null_or_empty() && text[cluster3] == '[' && cluster3 + 2 < text.Length && text[cluster3 + 2] == ']' && text[cluster3 + 1] >= '0' && text[cluster3 + 1] <= '9')
					{
						int num7 = text[cluster3 + 1] - 48;
						if (num7 < imageList.Count)
						{
							Vec3 value3 = imageList[num7];
							float num8 = (float)textureSize.x * value3.z * num5 * num6;
							value3.x = ((float)fTVector26Dot.X + num8 / 2f) / (float)textureSize.x;
							value3.y = ((float)fTVector26Dot.Y - 0.5f * (float)(metrics.Ascender + metrics.Descender)) / (float)textureSize.y;
							imageList[num7] = value3;
							fTVector26Dot.X += (Fixed26Dot6)num8;
							l += 2;
							k += 2;
							continue;
						}
					}
					value.LoadGlyph(glyphInfos[k].codepoint, LoadFlags.Render, LoadTarget.Normal);
					GlyphSlot glyph = value.Glyph;
					if (l == 0 && glyph.BitmapLeft < 0)
					{
						fTVector26Dot.X -= (Fixed26Dot6)glyph.BitmapLeft;
					}
					FTBitmap bitmap = glyph.Bitmap;
					int num9 = bitmap.Width;
					int num10 = bitmap.Rows;
					if (num9 > 0 && num10 > 0)
					{
						try
						{
							fixed (byte* ptr3 = bitmap.BufferData)
							{
								try
								{
									fixed (byte* ptr = _textBitmapBuffer)
									{
										int num11 = (int)(fTVector26Dot.X + glyphPositions[k].Offset.X) + glyph.BitmapLeft;
										int num12 = (int)(fTVector26Dot.Y - glyphPositions[k].Offset.Y) - glyph.BitmapTop;
										int x = textureSize.x;
										int pitch = bitmap.Pitch;
										if (num9 + num11 > textureSize.x)
										{
											num9 = textureSize.x - num11;
										}
										if (num10 + num12 > textureSize.y)
										{
											num10 = textureSize.y - num12;
										}
										int num13 = num9 >> 2;
										int num14 = num9 - (num13 << 2);
										byte* ptr2 = ptr + x * num12 + num11;
										byte* ptr4 = ptr3;
										if (num12 < 0)
										{
											ptr2 += -num12 * x;
											ptr4 += -num12 * pitch;
											num10 -= -num12;
										}
										for (int m = 0; m < num10; m++)
										{
											byte* ptr5 = ptr2;
											byte* ptr6 = ptr4;
											uint* ptr7 = (uint*)ptr5;
											uint* ptr8 = (uint*)ptr6;
											for (int n = 0; n < num13; n++)
											{
												*ptr7 |= *ptr8;
												ptr7++;
												ptr8++;
											}
											ptr5 = (byte*)ptr7;
											ptr6 = (byte*)ptr8;
											for (int num15 = 0; num15 < num14; num15++)
											{
												byte* intPtr = ptr5;
												*intPtr = (byte)(*intPtr | *ptr6);
												ptr6++;
												ptr5++;
											}
											ptr2 += x;
											ptr4 += pitch;
										}
									}
								}
								finally
								{
								}
							}
						}
						finally
						{
						}
					}
					fTVector26Dot.X += glyphPositions[k].Advance.X * num6;
					fTVector26Dot.Y -= glyphPositions[k].Advance.Y;
				}
				fTVector26Dot.Y += height;
			}
			texture.SetData(_textBitmapBuffer, 0, textureSize.x * textureSize.y);
		}

		public static int get_score(string leaderboardId, LeaderboardTypeEnum type, int rankId)
		{
			if (!get_leaderboard_content_result(leaderboardId, out var result))
			{
				return -1;
			}
			switch (type)
			{
			case LeaderboardTypeEnum.global:
				if (rankId < 0 || rankId >= result.worldBest.Length)
				{
					return -1;
				}
				return result.worldBest[rankId].score;
			case LeaderboardTypeEnum.aroundGlobal:
				if (rankId < 0 || rankId >= result.worldAroundLocal.Length)
				{
					return -1;
				}
				return result.worldAroundLocal[rankId].score;
			case LeaderboardTypeEnum.friends:
				if (rankId < 0 || rankId >= result.friendBest.Length)
				{
					return -1;
				}
				return result.friendBest[rankId].score;
			default:
				return -1;
			}
		}

		public static string get_player_display_id(string leaderboardId, LeaderboardTypeEnum type, int rankId)
		{
			if (!get_leaderboard_content_result(leaderboardId, out var result))
			{
				return "N/D";
			}
			switch (type)
			{
			case LeaderboardTypeEnum.global:
				if (result.worldBest.Length == 0 || rankId >= result.worldBest.Length)
				{
					return "N/D";
				}
				return get_machine_display_name(result.worldBest[rankId].machineId);
			case LeaderboardTypeEnum.aroundGlobal:
				if (result.worldAroundLocal.Length == 0 || rankId >= result.worldAroundLocal.Length)
				{
					return "N/D";
				}
				return get_machine_display_name(result.worldAroundLocal[rankId].machineId);
			case LeaderboardTypeEnum.friends:
				if (result.friendBest.Length == 0 || rankId >= result.friendBest.Length)
				{
					return "N/D";
				}
				return get_machine_display_name(result.friendBest[rankId].machineId);
			default:
				return "N/D";
			}
		}

		public static MachineId_s get_player_machine_id(string leaderboardId, LeaderboardTypeEnum type, int rankId)
		{
			if (!get_leaderboard_content_result(leaderboardId, out var result))
			{
				return default(MachineId_s);
			}
			switch (type)
			{
			case LeaderboardTypeEnum.global:
				if (result.worldBest.Length == 0 || rankId >= result.worldBest.Length)
				{
					return default(MachineId_s);
				}
				return result.worldBest[rankId].machineId;
			case LeaderboardTypeEnum.aroundGlobal:
				if (result.worldAroundLocal.Length == 0 || rankId >= result.worldAroundLocal.Length)
				{
					return default(MachineId_s);
				}
				return result.worldAroundLocal[rankId].machineId;
			case LeaderboardTypeEnum.friends:
				if (result.friendBest.Length == 0 || rankId >= result.friendBest.Length)
				{
					return default(MachineId_s);
				}
				return result.friendBest[rankId].machineId;
			default:
				return default(MachineId_s);
			}
		}
	}
}
