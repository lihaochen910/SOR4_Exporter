using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SDL2;

namespace CommonLib
{
	public static class xna
	{
		private class StandaloneGame : Game
		{
			private int applyChangesCounter;

			protected override void Update(GameTime gameTime)
			{
				if (updateDelegate == null)
				{
					return;
				}
				base.Update(gameTime);
				updateDelegate();
				if (applyChangesCounter == 0)
				{
					graphics.DeviceManager.IsFullScreen = false;
					graphics.DeviceManager.SynchronizeWithVerticalRetrace = shouldVSync;
					graphics.DeviceManager.ApplyChanges();
					applyChangesCounter = 1;
				}
				else if (applyChangesCounter == 1)
				{
					graphics.DeviceManager.IsFullScreen = shouldBeFullScreen;
					if (graphics.DeviceManager.IsFullScreen)
					{
						DisplayMode displayMode;
						if (Environment.GetEnvironmentVariable("FNA_GRAPHICS_ENABLE_HIGHDPI") == "1")
						{
							List<DisplayMode> list = base.GraphicsDevice.Adapter.SupportedDisplayModes[SurfaceFormat.Color] as List<DisplayMode>;
							displayMode = list[list.Count - 1];
						}
						else
						{
							displayMode = base.GraphicsDevice.Adapter.CurrentDisplayMode;
						}
						graphics.DeviceManager.PreferredBackBufferWidth = displayMode.Width;
						graphics.DeviceManager.PreferredBackBufferHeight = displayMode.Height;
					}
					else
					{
						graphics.DeviceManager.PreferredBackBufferWidth = GraphicsDeviceManager.DefaultBackBufferWidth;
						graphics.DeviceManager.PreferredBackBufferHeight = GraphicsDeviceManager.DefaultBackBufferHeight;
					}
					graphics.DeviceManager.SynchronizeWithVerticalRetrace = shouldVSync;
					graphics.DeviceManager.ApplyChanges();
					applyChangesCounter++;
				}
				else if (graphics.DeviceManager.IsFullScreen != shouldBeFullScreen || graphics.DeviceManager.SynchronizeWithVerticalRetrace != shouldVSync)
				{
					applyChangesCounter = 1;
				}
			}

			protected override void Draw(GameTime gameTime)
			{
				if (drawDelegate != null)
				{
					base.Draw(gameTime);
					Rectangle bounds = base.GraphicsDevice.PresentationParameters.Bounds;
					graphics.set_current_window(null, new Viewport(bounds));
					drawDelegate((float)gameTime.ElapsedGameTime.TotalSeconds);
				}
			}

			protected override void EndRun()
			{
				if (shutdownDelegate != null)
				{
					base.EndRun();
					shutdownDelegate();
				}
			}
		}

		private static Action updateDelegate;

		private static Action<float> drawDelegate;

		private static Action shutdownDelegate;

		private static bool shouldBeFullScreen;

		private static bool shouldVSync;

		public static GamePadState[] gamePadStateArray = new GamePadState[4];

		public static Game game;

		public static bool MainWindowIsActive => game.IsActive;

		public static string MainWindowTitle
		{
			get
			{
				return game.Window.Title;
			}
			set
			{
				if (game != null)
				{
					game.Window.Title = value;
				}
			}
		}

		public static void initialize(Action update, Action<float> draw, Action shutdown, bool isFullScreen, bool vSync)
		{
			game = new StandaloneGame();
			graphics.graphicsDeviceService = new GraphicsDeviceManager(game);
			graphics.serviceProvider = game.Services;
			game.Window.AllowUserResizing = true;
			game.IsFixedTimeStep = false;
			shouldBeFullScreen = isFullScreen;
			shouldVSync = vSync;
			graphics.DeviceManager.SynchronizeWithVerticalRetrace = vSync;
			game.RunOneFrame();
			game.Window.ClientSizeChanged += window_client_size_changed;
			graphics.contentManager = new graphics.CustomContentManager();
			updateDelegate = update;
			drawDelegate = draw;
			shutdownDelegate = shutdown;
			graphics.isHiDefSupported = graphics.Device.Adapter.IsProfileSupported(GraphicsProfile.HiDef);
			graphics.maxTextureSize = 8192;
		}

		private static void window_client_size_changed(object sender, EventArgs e)
		{
			text_texture_cache.flush();
		}

		public static void set_full_screen(bool isFullScreen)
		{
			shouldBeFullScreen = isFullScreen;
		}

		public static void set_v_sync(bool vSync)
		{
			shouldVSync = vSync;
		}

		public static void set_low_latency_mode(bool v)
		{
		}

		public static void update_game_pad_states()
		{
			for (int i = 0; i < 4; i++)
			{
				gamePadStateArray[i] = GamePad.GetState((PlayerIndex)i, GamePadDeadZone.None);
			}
		}

		public static void exit()
		{
			game.Exit();
		}

		public static void show_runtime_error_message_box_and_exit(string title, string message)
		{
			if (string.IsNullOrEmpty(title))
			{
				throw new ArgumentException("null or empty string", "title");
			}
			if (string.IsNullOrEmpty(message))
			{
				throw new ArgumentException("null or empty string", "message");
			}
			SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, title, message, IntPtr.Zero);
			Environment.Exit(0);
		}

		public static bool show_runtime_error_can_retry(string title, string message)
		{
			if (string.IsNullOrEmpty(title))
			{
				throw new ArgumentException("null or empty string", "title");
			}
			if (string.IsNullOrEmpty(message))
			{
				throw new ArgumentException("null or empty string", "message");
			}
			SDL.SDL_MessageBoxData messageboxdata = default(SDL.SDL_MessageBoxData);
			messageboxdata.flags = SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR;
			messageboxdata.title = title;
			messageboxdata.message = message;
			messageboxdata.numbuttons = 2;
			SDL.SDL_MessageBoxButtonData[] array = new SDL.SDL_MessageBoxButtonData[2];
			SDL.SDL_MessageBoxButtonData sDL_MessageBoxButtonData = (array[0] = new SDL.SDL_MessageBoxButtonData
			{
				buttonid = 0,
				text = "Exit"
			});
			sDL_MessageBoxButtonData = (array[1] = new SDL.SDL_MessageBoxButtonData
			{
				buttonid = 1,
				text = "Retry"
			});
			messageboxdata.buttons = array;
			int buttonid = 0;
			SDL.SDL_ShowMessageBox(ref messageboxdata, out buttonid);
			return buttonid == 1;
		}

		public static int show_runtime_error(string title, string message, string button0, string button1, string button2)
		{
			if (string.IsNullOrEmpty(title))
			{
				throw new ArgumentException("null or empty string", "title");
			}
			if (string.IsNullOrEmpty(message))
			{
				throw new ArgumentException("null or empty string", "message");
			}
			if (string.IsNullOrEmpty(button0))
			{
				throw new ArgumentException("null or empty string", "button0");
			}
			if (string.IsNullOrEmpty(button1))
			{
				throw new ArgumentException("null or empty string", "button1");
			}
			if (string.IsNullOrEmpty(button2))
			{
				throw new ArgumentException("null or empty string", "button2");
			}
			SDL.SDL_MessageBoxData messageboxdata = default(SDL.SDL_MessageBoxData);
			messageboxdata.flags = SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR;
			messageboxdata.title = title;
			messageboxdata.message = message;
			messageboxdata.numbuttons = 3;
			SDL.SDL_MessageBoxButtonData[] array = new SDL.SDL_MessageBoxButtonData[3];
			SDL.SDL_MessageBoxButtonData sDL_MessageBoxButtonData = (array[0] = new SDL.SDL_MessageBoxButtonData
			{
				buttonid = 0,
				text = button2
			});
			sDL_MessageBoxButtonData = (array[1] = new SDL.SDL_MessageBoxButtonData
			{
				buttonid = 1,
				text = button1
			});
			sDL_MessageBoxButtonData = (array[2] = new SDL.SDL_MessageBoxButtonData
			{
				buttonid = 2,
				text = button0
			});
			messageboxdata.buttons = array;
			int buttonid = 0;
			SDL.SDL_ShowMessageBox(ref messageboxdata, out buttonid);
			return 2 - buttonid;
		}

		public static void show_message_box_ok(string title, string message)
		{
			if (string.IsNullOrEmpty(title))
			{
				throw new ArgumentException("null or empty string", "title");
			}
			if (string.IsNullOrEmpty(message))
			{
				throw new ArgumentException("null or empty string", "message");
			}
			SDL.SDL_MessageBoxData messageboxdata = default(SDL.SDL_MessageBoxData);
			messageboxdata.flags = SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_INFORMATION;
			messageboxdata.title = title;
			messageboxdata.message = message;
			messageboxdata.numbuttons = 1;
			messageboxdata.buttons = new SDL.SDL_MessageBoxButtonData[1]
			{
				new SDL.SDL_MessageBoxButtonData
				{
					buttonid = 0,
					text = "OK"
				}
			};
			int buttonid = 0;
			SDL.SDL_ShowMessageBox(ref messageboxdata, out buttonid);
		}
	}
}
