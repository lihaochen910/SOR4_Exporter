using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CommonLib
{
	public static class graphics
	{
		internal class CustomContentManager : ContentManager
		{
			private Stream stream;

			private static Action<IDisposable> recordDisposable = RecordDisposable;

			public CustomContentManager()
				: base(graphics.ServiceProvider)
			{
			}

			public T read_asset<T>(string assetName, Stream stream)
			{
				this.stream = stream;
				T result = ReadAsset<T>(assetName, recordDisposable);
				this.stream.Dispose();
				this.stream = null;
				return result;
			}

			public override T Load<T>(string assetName)
			{
				throw new InvalidOperationException();
			}

			protected override Stream OpenStream(string assetName)
			{
				return stream;
			}

			private static void RecordDisposable(IDisposable disposable)
			{
			}
		}

		internal static IGraphicsDeviceService graphicsDeviceService;

		internal static IServiceProvider serviceProvider;

		private static RenderTarget2D currentWindowRenderTarget;

		private static Viewport currentWindowViewport;

		public static Rect_s currentRenderRect;

		public static RenderTarget2D currentRenderTarget;

		internal static CustomContentManager contentManager;

		internal static bool isHiDefSupported;

		internal static int maxTextureSize = 2048;

		public static BlendState defaultBlendState = BlendState.AlphaBlend;

		public static SamplerState defaultSamplerState = SamplerState.LinearClamp;

		public static DepthStencilState defaultDepthStencilState = DepthStencilState.None;

		public static RasterizerState defaultRasterizerState = RasterizerState.CullNone;

		[ThreadStatic]
		public static DebugDrawingContext debugDraw;

		public static GraphicsDeviceManager DeviceManager => (GraphicsDeviceManager)graphicsDeviceService;

		public static GraphicsDevice Device => graphicsDeviceService.GraphicsDevice;

		public static IServiceProvider ServiceProvider => serviceProvider;

		public static IntVec2 CurrentWindowViewportSize => new IntVec2(currentWindowViewport.Width, currentWindowViewport.Height);

		public static bool IsHiDefSupported => isHiDefSupported;

		public static int MaxTextureSize => maxTextureSize;

		public static void set_render_target(RenderTarget2D renderTarget)
		{
			if (renderTarget == null)
			{
				Device.SetRenderTarget(currentWindowRenderTarget);
				Device.Viewport = currentWindowViewport;
				currentRenderRect = new Rect_s(math.get_bounds_for_aspect_ratio(new Vec2(currentWindowViewport.Width, currentWindowViewport.Height), 1.77777779f));
			}
			else
			{
				Device.SetRenderTarget(renderTarget);
				currentRenderRect = new Rect_s(math.get_bounds_for_aspect_ratio(new Vec2(renderTarget.Width, renderTarget.Height), 1.77777779f));
			}
			currentRenderTarget = renderTarget;
		}

		public static void set_current_window(RenderTarget2D renderTarget, Viewport viewport)
		{
			currentWindowViewport = viewport;
			if (renderTarget == null)
			{
				renderTarget = currentWindowRenderTarget;
			}
			currentWindowRenderTarget = renderTarget;
			set_render_target(null);
		}

		public static void set_states(BlendState blend = null, SamplerState sampler = null, DepthStencilState depthStencil = null, RasterizerState rasterizer = null)
		{
			GraphicsDevice device = Device;
			device.BlendState = blend ?? defaultBlendState;
			device.SamplerStates[0] = sampler ?? defaultSamplerState;
			device.DepthStencilState = depthStencil ?? defaultDepthStencilState;
			device.RasterizerState = rasterizer ?? defaultRasterizerState;
		}

		public static void draw_sprite(Effect effect, Texture2D texture, Vec3 position, Vec2 size, float rotation, Vec2 origin, Vec2 sourcePosition, Vec2 sourceSize, Color_s color, bool pointSampling, bool wrapX, bool wrapY)
		{
		}

		public static void end_draw()
		{
		}

		public static T read_asset<T>(string assetName, Stream stream)
		{
			if (typeof(T) == typeof(Texture2D) || typeof(T) == typeof(Effect) || typeof(T) == typeof(SpriteFont))
			{
				return contentManager.read_asset<T>(assetName, stream);
			}
			throw new NotImplementedException();
		}
	}
}
