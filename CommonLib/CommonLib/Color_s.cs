using Microsoft.Xna.Framework;

namespace CommonLib
{
	public struct Color_s : IYamlCustomSerialization
	{
		[Tag(1, null, true)]
		public byte r;

		[Tag(2, null, true)]
		public byte g;

		[Tag(3, null, true)]
		public byte b;

		[Tag(4, null, true)]
		public byte a;

		public static Color_s white = new Color_s(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

		public static Color_s black = new Color_s(0, 0, 0, byte.MaxValue);

		public static Color_s red = new Color_s(byte.MaxValue, 0, 0, byte.MaxValue);

		public static Color_s green = new Color_s(0, byte.MaxValue, 0, byte.MaxValue);

		public static Color_s darkGreen = new Color_s(0, 128, 0, byte.MaxValue);

		public static Color_s blue = new Color_s(0, 0, byte.MaxValue, byte.MaxValue);

		public static Color_s cyan = new Color_s(0, byte.MaxValue, byte.MaxValue, byte.MaxValue);

		public static Color_s transparent = new Color_s(0, 0, 0, 0);

		public static Color_s razzleDazzleRose = new Color_s(byte.MaxValue, 64, 192, byte.MaxValue);

		public static Color_s apple = new Color_s(105, 181, 65, byte.MaxValue);

		public static Color_s appleDark = new Color_s(71, 170, 0, byte.MaxValue);

		public static Color_s redText = new Color_s(163, 21, 21, byte.MaxValue);

		public static Color_s grey = new Color_s(4286545791u);

		public static Color_s lightGrey = new Color_s(4292072403u);

		public static Color_s yellow = new Color_s(4294967040u);

		public static Color_s orange = new Color_s(4293765376u);

		public static Color_s pink = new Color_s(4291543295u);

		public static Color_s blacken = new Color_s(1056964608u);

		public float FloatR
		{
			get
			{
				return (float)(int)r / 255f;
			}
			set
			{
				r = (byte)(value * 255f).clamp(0f, 255f);
			}
		}

		public float FloatG
		{
			get
			{
				return (float)(int)g / 255f;
			}
			set
			{
				g = (byte)(value * 255f).clamp(0f, 255f);
			}
		}

		public float FloatB
		{
			get
			{
				return (float)(int)b / 255f;
			}
			set
			{
				b = (byte)(value * 255f).clamp(0f, 255f);
			}
		}

		public float FloatA
		{
			get
			{
				return (float)(int)a / 255f;
			}
			set
			{
				a = (byte)(value * 255f).clamp(0f, 255f);
			}
		}

		public Vec3 FloatRgb
		{
			get
			{
				return new Vec3(FloatR, FloatG, FloatB);
			}
			set
			{
				FloatR = value.x;
				FloatG = value.y;
				FloatB = value.z;
			}
		}

		public bool IsZero
		{
			get
			{
				if (a == 0 && r == 0 && g == 0)
				{
					return b == 0;
				}
				return false;
			}
		}

		public Color_s(byte r, byte g, byte b, byte a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		public Color_s(uint hexARGB)
		{
			r = (byte)((hexARGB & 0xFF0000) >> 16);
			g = (byte)((hexARGB & 0xFF00) >> 8);
			b = (byte)(hexARGB & 0xFFu);
			a = (byte)((hexARGB & 0xFF000000u) >> 24);
		}

		public Color_s(float r, float g, float b, float a)
		{
			this.r = (byte)(r * 255f).clamp(0f, 255f);
			this.g = (byte)(g * 255f).clamp(0f, 255f);
			this.b = (byte)(b * 255f).clamp(0f, 255f);
			this.a = (byte)(a * 255f).clamp(0f, 255f);
		}

		public Color_s(Vec3 rgb, float a)
		{
			r = (byte)(rgb.x * 255f).clamp(0f, 255f);
			g = (byte)(rgb.y * 255f).clamp(0f, 255f);
			b = (byte)(rgb.z * 255f).clamp(0f, 255f);
			this.a = (byte)(a * 255f).clamp(0f, 255f);
		}

		public void premultiply_alpha()
		{
			float floatA = FloatA;
			FloatR *= floatA;
			FloatG *= floatA;
			FloatB *= floatA;
		}

		public Color_s get_premultiplied()
		{
			Color_s result = this;
			result.premultiply_alpha();
			return result;
		}

		public Color_s get_additivePremult()
		{
			return new Color_s(FloatR * FloatA, FloatG * FloatA, FloatB * FloatA, 0f);
		}

		public Color_s get_desaturated()
		{
			float num = FloatRgb.dot(new Vec3(0.3f, 0.59f, 0.11f));
			return new Color_s(num, num, num, FloatA);
		}

		public override string ToString()
		{
			return yaml_serialize();
		}

		public string yaml_serialize()
		{
			return utils.pack_to_string(r, g, b, a);
		}

		public void yaml_deserialize(string src)
		{
			utils.unpack_from_string(src, out r, out g, out b, out a);
		}

		public static implicit operator Color(Color_s v)
		{
			return new Color(v.r, v.g, v.b, v.a);
		}

		public static implicit operator Color_s(Color v)
		{
			return new Color_s(v.R, v.G, v.B, v.A);
		}

		public static implicit operator Vector4(Color_s v)
		{
			return (Vec4)v;
		}

		public static implicit operator Vec4(Color_s v)
		{
			return new Vec4(v.FloatR, v.FloatG, v.FloatB, v.FloatA);
		}

		public static implicit operator Color_s(Vec4 v)
		{
			return new Color_s(v.x, v.y, v.z, v.w);
		}

		public static explicit operator uint(Color_s col)
		{
			return (uint)(col.b | (col.g << 8) | (col.r << 16) | (col.a << 24));
		}

		public uint to_ABGR()
		{
			uint num = (uint)this;
			return (num & 0xFF00FF00u) | ((num & 0xFF0000) >> 16) | ((num & 0xFF) << 16);
		}

		public uint to_ABGR(byte overrideAlpha)
		{
			uint num = to_ABGR();
			return (num & 0xFFFFFFu) | (uint)(overrideAlpha << 24);
		}

		public static Color_s operator +(Color_s a, Color_s b)
		{
			a.r = (byte)(a.r + b.r).clamp(0, 255);
			a.g = (byte)(a.g + b.g).clamp(0, 255);
			a.b = (byte)(a.b + b.b).clamp(0, 255);
			a.a = (byte)(a.a + b.a).clamp(0, 255);
			return a;
		}

		public static Color_s operator -(Color_s a, Color_s b)
		{
			a.r -= b.r;
			a.g -= b.g;
			a.b -= b.b;
			a.a -= b.a;
			return a;
		}

		public static Color_s operator *(Color_s a, Color_s b)
		{
			return new Color_s(a.FloatR * b.FloatR, a.FloatG * b.FloatG, a.FloatB * b.FloatB, a.FloatA * b.FloatA);
		}

		public static Color_s operator *(Color_s a, float b)
		{
			return new Color_s(a.FloatR * b, a.FloatG * b, a.FloatB * b, a.FloatA * b);
		}

		public static Color_s operator /(Color_s a, int b)
		{
			a.r /= (byte)b;
			a.g /= (byte)b;
			a.b /= (byte)b;
			a.a /= (byte)b;
			return a;
		}

		public static Color_s mix(Color_s a, Color_s b, float coef)
		{
			return a * (1f - coef) + b * coef;
		}

		public static bool operator !=(Color_s a, Color_s b)
		{
			if (a.r == b.r && a.g == b.g && a.b == b.b)
			{
				return a.a != b.a;
			}
			return true;
		}

		public static bool operator ==(Color_s a, Color_s b)
		{
			if (a.r == b.r && a.g == b.g && a.b == b.b)
			{
				return a.a == b.a;
			}
			return false;
		}

		public override bool Equals(object o)
		{
			if (!(o is Color_s))
			{
				return false;
			}
			return this == (Color_s)o;
		}

		public override int GetHashCode()
		{
			return (int)(uint)this;
		}

		public static Color_s from_gradient(Color_s[] colorArray, float value)
		{
			value = (value * (float)colorArray.Length - 1f).clamp(0f, (float)colorArray.Length - 1.0001f);
			return mix(colorArray[(int)value], colorArray[(int)value + 1], value - (float)(int)value);
		}
	}
}
