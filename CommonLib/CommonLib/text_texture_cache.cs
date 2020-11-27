using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace CommonLib
{
	public static class text_texture_cache
	{
		public class TextureProxy
		{
			public Texture2D texture;

			public long lastTimeUsed;
		}

		public struct TextureKey_s : IEquatable<TextureKey_s>
		{
			public string text;

			public platform.FontDefinition_s fontDef;

			public IntVec2 textureSize;

			public int pointSize;

			public TextFlagEnum flags;

			public override int GetHashCode()
			{
				return text.GetHashCode() + 13 * fontDef.GetHashCode() + 27 * textureSize.GetHashCode() + 41 * pointSize.GetHashCode() + 55 * (int)flags;
			}

			public bool Equals(TextureKey_s other)
			{
				if (text == other.text && fontDef == other.fontDef && textureSize == other.textureSize && pointSize == other.pointSize)
				{
					return flags == other.flags;
				}
				return false;
			}

			public override bool Equals(object obj)
			{
				if (obj is TextureKey_s)
				{
					return Equals((TextureKey_s)obj);
				}
				return false;
			}
		}

		public static int maxLifeTime = 60;

		public static int maxReuseLifeTime = 1800;

		public static int maxTextureCount = -1;

		public static long maxMemory = 8388608L;

		public static bool flushNow = false;

		public static Dictionary<TextureKey_s, TextureProxy> cache = new Dictionary<TextureKey_s, TextureProxy>();

		private static long lastCacheCheck;

		public static long cacheMemory = 0L;

		private const uint BYTEPERPIXEL = 1u;

		public static Texture2D get_rendered_text(string text, platform.FontDefinition_s fontDefinition, string languageCode, float fontSize, IntVec2 size, TextFlagEnum flags = TextFlagEnum.none, List<Vec3> imgLocationList = null)
		{
			if (flushNow)
			{
				flushNow = false;
				flush();
			}
			long num = DateTime.Now.Ticks * 60 / 10000000;
			size.x = ((size.x - 1) | 1) + 1;
			if (fontSize > 64f || fontSize < 1f)
			{
				float num2 = fontSize;
				fontSize = fontSize.clamp(1f, 64f);
				size = (IntVec2)((Vec2)size * fontSize / num2);
			}
			TextureKey_s textureKey_s = default(TextureKey_s);
			textureKey_s.text = text;
			textureKey_s.fontDef = fontDefinition;
			textureKey_s.pointSize = (int)fontSize.sqrt();
			textureKey_s.textureSize = size / (int)fontSize / 2;
			textureKey_s.flags = flags;
			TextureKey_s key = textureKey_s;
			TextureProxy value;
			bool flag = cache.TryGetValue(key, out value);
			if (flag && value.texture.IsDisposed)
			{
				flag = false;
				remove_texture(key);
			}
			if (flag && !imgLocationList.is_null_or_empty())
			{
				for (int i = 0; i < imgLocationList.Count; i++)
				{
					if (imgLocationList[i].x == 0f)
					{
						cache[key].lastTimeUsed = 0L;
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				value.lastTimeUsed = num;
				update_cache(num);
				return value.texture;
			}
			TextureProxy textureProxy = null;
			foreach (KeyValuePair<TextureKey_s, TextureProxy> item in cache)
			{
				if (item.Value.texture.IsDisposed)
				{
					remove_texture(item.Key);
				}
				else if (item.Value.texture.Width == size.x && item.Value.texture.Height == size.y && item.Value.lastTimeUsed + maxLifeTime < num)
				{
					cache.Remove(item.Key);
					textureProxy = item.Value;
					break;
				}
			}
			if (textureProxy == null)
			{
				textureProxy = new TextureProxy
				{
					texture = new Texture2D(graphics.Device, size.x, size.y, mipMap: false, SurfaceFormat.Alpha8)
				};
				update_cache(num, textureProxy.texture);
			}
			cache[key] = textureProxy;
			platform.render_text_to_texture(textureProxy.texture, text, fontDefinition, languageCode, size, fontSize, flags, imgLocationList);
			textureProxy.lastTimeUsed = num;
			return textureProxy.texture;
		}

		private static void remove_texture(TextureKey_s key)
		{
			cacheMemory -= cache[key].texture.Width * cache[key].texture.Height;
			cache[key].texture.Dispose();
			cache.Remove(key);
		}

		private static void update_cache(long currentTime, Texture2D newTexture = null)
		{
			if (maxReuseLifeTime > 0 && currentTime != lastCacheCheck)
			{
				lastCacheCheck = currentTime;
				List<KeyValuePair<TextureKey_s, TextureProxy>> list = new List<KeyValuePair<TextureKey_s, TextureProxy>>();
				foreach (KeyValuePair<TextureKey_s, TextureProxy> item in cache)
				{
					if (item.Value.lastTimeUsed + maxReuseLifeTime < currentTime)
					{
						list.Add(item);
					}
				}
				foreach (KeyValuePair<TextureKey_s, TextureProxy> item2 in list)
				{
					remove_texture(item2.Key);
				}
			}
			if (newTexture == null)
			{
				return;
			}
			if (maxTextureCount > 0 && cache.Count > maxTextureCount)
			{
				int count = cache.Count - maxTextureCount;
				IEnumerable<KeyValuePair<TextureKey_s, TextureProxy>> enumerable = cache.OrderByDescending((KeyValuePair<TextureKey_s, TextureProxy> x) => x.Value.lastTimeUsed).Take(count);
				foreach (KeyValuePair<TextureKey_s, TextureProxy> item3 in enumerable)
				{
					remove_texture(item3.Key);
				}
			}
			cacheMemory += newTexture.Width * newTexture.Height;
			if (maxMemory > 0 && cacheMemory > maxMemory)
			{
				List<KeyValuePair<TextureKey_s, TextureProxy>> list2 = cache.OrderByDescending((KeyValuePair<TextureKey_s, TextureProxy> x) => x.Value.lastTimeUsed).ToList();
				int num = 0;
				while (cacheMemory > maxMemory && num < list2.Count)
				{
					remove_texture(list2.ElementAt(num++).Key);
				}
			}
		}

		public static void flush()
		{
			foreach (TextureProxy value in cache.Values)
			{
				if (value.texture != null && !value.texture.IsDisposed)
				{
					value.texture.Dispose();
				}
			}
			cache.Clear();
			cacheMemory = 0L;
		}
	}
}
