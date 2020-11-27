using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonLib
{
	public static class math
	{
		public interface IKeyframe
		{
			CurveKeyTypeEnum CurveKeyType
			{
				get;
			}

			int Time
			{
				get;
				set;
			}
		}

		public const float pi = (float)Math.PI;

		public const float two_pi = (float)Math.PI * 2f;

		public const float half_pi = (float)Math.PI / 2f;

		public const float quarter_pi = (float)Math.PI / 4f;

		public const float eighth_pi = (float)Math.PI / 8f;

		public const float angle_down = -(float)Math.PI / 2f;

		public const float angle_up = (float)Math.PI / 2f;

		public const float angle_right = 0f;

		public const float angle_left = (float)Math.PI;

		public const float one_sixtieth = 0.0166666675f;

		public const float cos45 = 0.707106769f;

		public const float sin45 = 0.707106769f;

		private const float deg_to_rad_factor = (float)Math.PI / 180f;

		private const float rad_to_deg_factor = 180f / (float)Math.PI;

		public static T[] get_sorted<T>(this T[] keyArray) where T : IKeyframe
		{
			return keyArray.OrderBy((T x) => x.Time).ToArray();
		}

		private static bool get_keys<T>(this T[] keyArray, int time, out T k0, out T k1, Type ofType = null) where T : IKeyframe
		{
			bool result = true;
			int num = -1;
			int num2 = -1;
			for (int i = 0; i < keyArray.Length; i++)
			{
				if (ofType != null && keyArray[i].GetType() != ofType)
				{
					continue;
				}
				if (keyArray[i].Time > time)
				{
					if (num2 == -1 || keyArray[i].Time <= keyArray[num2].Time)
					{
						num2 = i;
					}
				}
				else if (keyArray[i].Time < time)
				{
					if (num == -1 || keyArray[i].Time >= keyArray[num].Time)
					{
						num = i;
					}
				}
				else
				{
					num = (num2 = i);
				}
			}
			if (num == -1)
			{
				num = num2;
				result = false;
			}
			if (num2 == -1)
			{
				num2 = num;
			}
			if (num == -1 && num2 == -1)
			{
				k0 = (k1 = default(T));
				return false;
			}
			k0 = keyArray[num];
			k1 = keyArray[num2];
			return result;
		}

		public static bool interpolate<T>(this T[] keyArray, int time, out T k0, out T k1, out float mix, Type ofType = null) where T : IKeyframe
		{
			bool result = keyArray.get_keys(time, out k0, out k1);
			mix = ((k1.Time - k0.Time > 0) ? ((float)(time - k0.Time) / (float)(k1.Time - k0.Time)) : 0f);
			mix = easings.interpolate(mix, k0.CurveKeyType);
			return result;
		}

		public static bool interpolate<T>(this T[] keyArray, int time, out T k0, out T k1, out Fix mix, Type ofType = null) where T : IKeyframe
		{
			if (keyArray.get_keys(time, out k0, out k1, ofType))
			{
				mix = ((k1.Time - k0.Time > 0) ? new Fix(time - k0.Time, k1.Time - k0.Time) : ((Fix)0));
				mix = easings.interpolate(mix, k0.CurveKeyType);
				return true;
			}
			mix = 0;
			return false;
		}

		public static float sign(this float f)
		{
			return Math.Sign(f);
		}

		public static int sign(this int i)
		{
			return Math.Sign(i);
		}

		public static float abs(this float f)
		{
			return Math.Abs(f);
		}

		public static int abs(this int i)
		{
			return Math.Abs(i);
		}

		public static long abs(this long i)
		{
			return Math.Abs(i);
		}

		public static Vec2 abs(this Vec2 v)
		{
			return new Vec2(v.x.abs(), v.y.abs());
		}

		public static IntVec2 abs(this IntVec2 v)
		{
			return new IntVec2(v.x.abs(), v.y.abs());
		}

		public static uint distance(uint a, uint b)
		{
			if (a > b)
			{
				return a - b;
			}
			return b - a;
		}

		public static float sqrt(this float f)
		{
			return (float)Math.Sqrt(f);
		}

		public static float floor(this float f)
		{
			return (float)Math.Floor(f);
		}

		public static float ceiling(this float f)
		{
			return (float)Math.Ceiling(f);
		}

		public static int floor_to_int(this float f)
		{
			return (int)Math.Floor(f);
		}

		public static int ceiling_to_int(this float f)
		{
			return (int)Math.Ceiling(f);
		}

		public static int closest_int(this float f)
		{
			float num = f - f.floor();
			if (num < 0.5f)
			{
				return f.floor_to_int();
			}
			return f.ceiling_to_int();
		}

		public static float sin(this float f)
		{
			return (float)Math.Sin(f);
		}

		public static float cos(this float f)
		{
			return (float)Math.Cos(f);
		}

		public static float atan2(float x, float y)
		{
			return (float)Math.Atan2(y, x);
		}

		public static float atan2(Vec2 xy)
		{
			return (float)Math.Atan2(xy.y, xy.x);
		}

		public static float deg_to_rad(this float f)
		{
			return (float)Math.PI / 180f * f;
		}

		public static float rad_to_deg(this float f)
		{
			return 180f / (float)Math.PI * f;
		}

		public static ushort add_no_overflow(this ushort a, ushort b)
		{
			int num = a + b;
			if (num > 65535)
			{
				return ushort.MaxValue;
			}
			if (num < 0)
			{
				return 0;
			}
			return (ushort)num;
		}

		public static float lerp(this float a, float b, float t)
		{
			return a + (b - a) * t;
		}

		public static Vec2 lerp(Vec2 a, Vec2 b, float v)
		{
			return a + (b - a) * v;
		}

		public static Vec3 lerp(Vec3 a, Vec3 b, float v)
		{
			return a + (b - a) * v;
		}

		public static Vec4 lerp(Vec4 a, Vec4 b, float v)
		{
			return a + (b - a) * v;
		}

		public static float lerp_angle(this float a, float b, float t)
		{
			throw new NotImplementedException();
		}

		public static float damping(this float src, float dst, float dt, float factor)
		{
			return (src * factor + dst * dt) / (factor + dt);
		}

		public static float damping(this float src, float dst, float dt, float factor, float maxSpeed)
		{
			dst = src.damping(dst, dt, factor);
			return src.linear_damping(dst, maxSpeed * dt);
		}

		public static float linear_damping(this float src, float dest, float speed)
		{
			float num = (src - dest).abs();
			if (num < speed)
			{
				return dest;
			}
			return src + speed * (dest - src) / num;
		}

		public static bool approximately(this float a, float b)
		{
			throw new NotImplementedException();
		}

		public static float clamp(this float f, float min, float max)
		{
			if (f < min)
			{
				return min;
			}
			if (f > max)
			{
				return max;
			}
			return f;
		}

		public static int clamp(this int i, int min, int max)
		{
			if (i < min)
			{
				return min;
			}
			if (i > max)
			{
				return max;
			}
			return i;
		}

		public static float clamp_and_get_cursor(this float f, float min, float max)
		{
			if (max == min)
			{
				return 0f;
			}
			if (f < min)
			{
				return 0f;
			}
			if (f > max)
			{
				return 1f;
			}
			return (f - min) / (max - min);
		}

		public static float get_cursor(this float f, float min, float max)
		{
			if (max == min)
			{
				return 0f;
			}
			return (f - min) / (max - min);
		}

		public static int pow(int a, int b)
		{
			if (b == 0)
			{
				return 1;
			}
			if (b < 0)
			{
				return 1 / pow(a, -b);
			}
			int num = a;
			for (int i = 2; i <= b; i++)
			{
				a *= num;
			}
			return a;
		}

		public static long pow(long a, long b)
		{
			if (b == 0L)
			{
				return 1L;
			}
			if (b < 0)
			{
				return 1 / pow(a, -b);
			}
			long num = a;
			for (long num2 = 2L; num2 <= b; num2++)
			{
				a *= num;
			}
			return a;
		}

		public static float pow(float a, float b)
		{
			if (b == 0f)
			{
				return 1f;
			}
			if (b < 0f)
			{
				return 1f / pow(a, 0f - b);
			}
			a = (float)Math.Pow(a, b);
			return a;
		}

		public static float pow_signed(float a, float b)
		{
			if (!(a < 0f))
			{
				return pow(a, b);
			}
			return 0f - pow(0f - a, b);
		}

		public static Vec2 pow(Vec2 a, Vec2 b)
		{
			return new Vec2(pow(a.x, b.x), pow(a.y, b.y));
		}

		public static float log10(float f)
		{
			return (float)Math.Log10(f);
		}

		public static uint log10(uint v)
		{
			if (v < 1000000000)
			{
				if (v < 100000000)
				{
					if (v < 10000000)
					{
						if (v < 1000000)
						{
							if (v < 100000)
							{
								if (v < 10000)
								{
									if (v < 1000)
									{
										switch (v)
										{
										case 0u:
										case 1u:
										case 2u:
										case 3u:
										case 4u:
										case 5u:
										case 6u:
										case 7u:
										case 8u:
										case 9u:
											return 0u;
										case 10u:
										case 11u:
										case 12u:
										case 13u:
										case 14u:
										case 15u:
										case 16u:
										case 17u:
										case 18u:
										case 19u:
										case 20u:
										case 21u:
										case 22u:
										case 23u:
										case 24u:
										case 25u:
										case 26u:
										case 27u:
										case 28u:
										case 29u:
										case 30u:
										case 31u:
										case 32u:
										case 33u:
										case 34u:
										case 35u:
										case 36u:
										case 37u:
										case 38u:
										case 39u:
										case 40u:
										case 41u:
										case 42u:
										case 43u:
										case 44u:
										case 45u:
										case 46u:
										case 47u:
										case 48u:
										case 49u:
										case 50u:
										case 51u:
										case 52u:
										case 53u:
										case 54u:
										case 55u:
										case 56u:
										case 57u:
										case 58u:
										case 59u:
										case 60u:
										case 61u:
										case 62u:
										case 63u:
										case 64u:
										case 65u:
										case 66u:
										case 67u:
										case 68u:
										case 69u:
										case 70u:
										case 71u:
										case 72u:
										case 73u:
										case 74u:
										case 75u:
										case 76u:
										case 77u:
										case 78u:
										case 79u:
										case 80u:
										case 81u:
										case 82u:
										case 83u:
										case 84u:
										case 85u:
										case 86u:
										case 87u:
										case 88u:
										case 89u:
										case 90u:
										case 91u:
										case 92u:
										case 93u:
										case 94u:
										case 95u:
										case 96u:
										case 97u:
										case 98u:
										case 99u:
											return 1u;
										default:
											return 2u;
										}
									}
									return 3u;
								}
								return 4u;
							}
							return 5u;
						}
						return 6u;
					}
					return 7u;
				}
				return 8u;
			}
			return 9u;
		}

		public static int log10(int v)
		{
			return (int)log10((uint)v.abs());
		}

		public static float min(float a, float b)
		{
			if (b < a)
			{
				return b;
			}
			return a;
		}

		public static int min(int a, int b)
		{
			if (b < a)
			{
				return b;
			}
			return a;
		}

		public static uint min(uint a, uint b)
		{
			if (b < a)
			{
				return b;
			}
			return a;
		}

		public static int min(int a, int b, int c)
		{
			if (b < a)
			{
				a = b;
			}
			if (c < a)
			{
				a = c;
			}
			return a;
		}

		public static float min(float a, float b, float c)
		{
			if (b < a)
			{
				a = b;
			}
			if (c < a)
			{
				a = c;
			}
			return a;
		}

		public static float min(float a, float b, float c, float d)
		{
			if (b < a)
			{
				a = b;
			}
			if (c < a)
			{
				a = c;
			}
			if (d < a)
			{
				a = d;
			}
			return a;
		}

		public static float max(float a, float b)
		{
			if (a > b)
			{
				return a;
			}
			return b;
		}

		public static float max(float a, float b, float c)
		{
			if (b > a)
			{
				a = b;
			}
			if (c > a)
			{
				a = c;
			}
			return a;
		}

		public static int max(int a, int b)
		{
			if (a > b)
			{
				return a;
			}
			return b;
		}

		public static int max(int a, int b, int c)
		{
			return max(max(a, b), c);
		}

		public static uint max(uint a, uint b)
		{
			if (a > b)
			{
				return a;
			}
			return b;
		}

		public static float max(float a, float b, float c, float d)
		{
			if (b > a)
			{
				a = b;
			}
			if (c > a)
			{
				a = c;
			}
			if (d > a)
			{
				a = d;
			}
			return a;
		}

		public static Vec2 max(Vec2 a, Vec2 b)
		{
			return new Vec2(max(a.x, b.x), max(a.y, b.y));
		}

		public static void get_min_max(Vec2 a, Vec2 b, Vec2 c, Vec2 d, out Vec2 outMin, out Vec2 outMax)
		{
			outMin = new Vec2(min(a.x, b.x, c.x, d.x), min(a.y, b.y, c.y, d.y));
			outMax = new Vec2(max(a.x, b.x, c.x, d.x), max(a.y, b.y, c.y, d.y));
		}

		public static BoundingBox2d_s get_bounds_for_aspect_ratio(Vec2 viewportSize, float aspectRatio)
		{
			if ((int)viewportSize.y == (int)(viewportSize.x / aspectRatio) || (int)viewportSize.x == (int)(viewportSize.y * aspectRatio))
			{
				return new BoundingBox2d_s(Vec2.zero, viewportSize);
			}
			float num = viewportSize.y * aspectRatio;
			BoundingBox2d_s result = default(BoundingBox2d_s);
			if (num < viewportSize.x)
			{
				result.min.x = (viewportSize.x - num) * 0.5f;
				result.min.y = 0f;
				result.max.x = result.min.x + num;
				result.max.y = viewportSize.y;
			}
			else
			{
				float num2 = viewportSize.x / aspectRatio;
				result.min.x = 0f;
				result.min.y = (viewportSize.y - num2) * 0.5f;
				result.max.x = viewportSize.x;
				result.max.y = result.min.y + num2;
			}
			return result;
		}

		public static Fix sqrt(Fix value)
		{
			if (value.Raw < 0)
			{
				throw new ArgumentOutOfRangeException("value", "Value must be non-negative.");
			}
			if (value.Raw == 0)
			{
				return 0;
			}
			return new Fix((int)(sqrt_ulong((ulong)((long)value.Raw << 18)) + 1) >> 1);
		}

		public static Fix cos_degrees(Fix angle)
		{
			return Fix.cos_degrees(angle);
		}

		public static Fix cos_radians(Fix angle)
		{
			return Fix.cos_degrees(angle * 180 / Fix.pi);
		}

		public static Fix sin_degrees(Fix angle)
		{
			return Fix.sin_degrees(angle);
		}

		public static Fix sin_radians(Fix angle)
		{
			return Fix.sin_degrees(angle * 180 / Fix.pi);
		}

		public static Fix tan_degrees(Fix angle)
		{
			return Fix.tan_degrees(angle);
		}

		public static Fix atan2_degrees(FixVec2 v)
		{
			return Fix.Atan2(v.x, v.y);
		}

		public static Fix get_clothest_8_dir_angle_radians(Fix angle)
		{
			FixVec2 fixVec = FixVec2.new_direction_radians(angle);
			Fix fix = new Fix(924, 1000);
			if (fixVec.x > fix)
			{
				return 0;
			}
			if (fixVec.y > fix)
			{
				return Fix.pi / 2;
			}
			if (fixVec.x < -fix)
			{
				return Fix.pi;
			}
			if (fixVec.y < -fix)
			{
				return -Fix.pi / 2;
			}
			if (fixVec.x > 0 && fixVec.y > 0)
			{
				return Fix.pi / 4;
			}
			if (fixVec.x < 0 && fixVec.y > 0)
			{
				return 3 * Fix.pi / 4;
			}
			if (fixVec.x > 0 && fixVec.y < 0)
			{
				return -Fix.pi / 4;
			}
			if (fixVec.x < 0 && fixVec.y < 0)
			{
				return -3 * Fix.pi / 4;
			}
			return 0;
		}

		public static Fix abs(Fix value)
		{
			if (value.raw < 0)
			{
				return new Fix(-value.raw);
			}
			return value;
		}

		public static FixVec2 abs(FixVec2 v)
		{
			return new FixVec2(abs(v.x), abs(v.y));
		}

		public static Fix sign(Fix value)
		{
			if (value < 0)
			{
				return -1;
			}
			if (value > 0)
			{
				return 1;
			}
			return 0;
		}

		public static Fix sign_not_zero(Fix value)
		{
			if (value < 0)
			{
				return -1;
			}
			return 1;
		}

		public static Fix max(Fix v1, Fix v2)
		{
			if (!(v1 > v2))
			{
				return v2;
			}
			return v1;
		}

		public static Fix max(Fix v1, Fix v2, Fix v3)
		{
			return max(max(v1, v2), v3);
		}

		public static Fix min(Fix v1, Fix v2)
		{
			if (!(v1 < v2))
			{
				return v2;
			}
			return v1;
		}

		public static Fix min(Fix v1, Fix v2, Fix v3)
		{
			return min(min(v1, v2), v3);
		}

		public static long min(long v1, long v2)
		{
			if (v1 >= v2)
			{
				return v2;
			}
			return v1;
		}

		internal static uint sqrt_ulong(ulong N)
		{
			switch (N)
			{
			case 0uL:
				return 0u;
			case 1uL:
				return 1u;
			default:
			{
				ulong num = 33554432uL;
				while (true)
				{
					ulong num2 = num + N / num >> 1;
					if (num2 >= num)
					{
						break;
					}
					num = num2;
				}
				return (uint)num;
			}
			}
		}

		public static Fix clamp(this Fix f, Fix min, Fix max)
		{
			if (f < min)
			{
				return min;
			}
			if (f > max)
			{
				return max;
			}
			return f;
		}

		public static Fix clamp_magnitude(Fix f, Fix maxMagnitude)
		{
			if (abs(f) < maxMagnitude)
			{
				return f;
			}
			return sign(f) * maxMagnitude;
		}

		public static Fix lerp(Fix a, Fix b, Fix r)
		{
			return a + (b - a) * r;
		}

		public static FixVec2 lerp(FixVec2 a, FixVec2 b, Fix r)
		{
			return a + (b - a) * r;
		}

		public static FixVec3 lerp(FixVec3 a, FixVec3 b, Fix r)
		{
			return a + (b - a) * r;
		}

		public static Fix clamp_and_get_cursor(Fix f, Fix min, Fix max)
		{
			if (max == min)
			{
				return 0;
			}
			if (f < min)
			{
				return 0;
			}
			if (f > max)
			{
				return 1;
			}
			return (f - min) / (max - min);
		}

		public static Fix increase_in_same_direction(Fix a, Fix b)
		{
			Fix fix = sign(a);
			if (fix != sign(b))
			{
				return b;
			}
			return fix * max(abs(a), abs(b));
		}

		public static int floor_to_int(this Fix f)
		{
			return (int)Fix.Floor(f);
		}

		public static int ceiling_to_int(this Fix f)
		{
			return (int)Fix.Ceiling(f);
		}

		public static int closest_int(this Fix f)
		{
			Fix fix = Fix.Floor(f);
			Fix fix2 = Fix.Ceiling(f);
			if (abs(fix2 - f) <= abs(fix - f))
			{
				return (int)fix2;
			}
			return (int)fix;
		}

		public static Fix linear_damping(Fix src, Fix dest, Fix speed)
		{
			if (speed <= 0)
			{
				return src;
			}
			Fix fix = abs(src - dest);
			if (fix == 0)
			{
				return dest;
			}
			if (fix < speed)
			{
				return dest;
			}
			return src + speed * (dest - src) / fix;
		}

		public static Fix damping_with_max_speed(this Fix src, Fix dst, Fix factor, Fix maxSpeed)
		{
			dst = src + (dst - src) * factor;
			return linear_damping(src, dst, maxSpeed);
		}

		public static Fix pow(Fix b, Fix exp)
		{
			if (b == 1 || exp == 0)
			{
				return 1;
			}
			if (b == 0)
			{
				return 0;
			}
			int num;
			Fix result;
			if ((exp.Raw & 0xFFFF) == 0)
			{
				num = exp.Raw + 32768 >> 16;
				Fix fix;
				int num2;
				if (num < 0)
				{
					fix = 1 / b;
					num2 = -num;
				}
				else
				{
					fix = b;
					num2 = num;
				}
				result = 1;
				while (num2 > 0)
				{
					if (((uint)num2 & (true ? 1u : 0u)) != 0)
					{
						result *= fix;
					}
					fix *= fix;
					num2 >>= 1;
				}
				return result;
			}
			exp *= Fix.Log(b, 2);
			b = 2;
			num = exp.Raw + 32768 >> 16;
			result = ((num < 0) ? (Fix.One >> -num) : (Fix.One << num));
			long num3 = (exp.Raw - (num << 16)) * Fix._ln2Const.Raw + 32768 >> 16;
			if (num3 == 0L)
			{
				return result;
			}
			long num4 = num3;
			long num5 = num3;
			for (int i = 2; i < Fix._invFactConsts.Length; i++)
			{
				if (num5 == 0L)
				{
					break;
				}
				num5 *= num3;
				num5 += 2147483648u;
				num5 >>= 32;
				long num6 = num5 * Fix._invFactConsts[i].Raw;
				num6 += 2147483648u;
				num6 >>= 32;
				num4 += num6;
			}
			return new Fix((int)((result.Raw * num4 + 2147483648u >> 32) + result.Raw));
		}

		public static Fix exp(Fix value)
		{
			return pow(Fix.e, value);
		}

		public static Vec2 project_point_on_line(Vec2 point, Vec2 pointOnLine, Vec2 lineNormal)
		{
			float num = (pointOnLine - point).dot(lineNormal);
			return pointOnLine - num * lineNormal;
		}

		public static float intersect_ray_to_plane(Vec2 rayOrigin, Vec2 rayDirection, Vec2 pointOnPlane, Vec2 planeNormal)
		{
			float num = 0f - planeNormal.dot(pointOnPlane);
			float num2 = planeNormal.dot(rayOrigin) + num;
			float num3 = planeNormal.dot(rayDirection);
			return 0f - num2 / num3;
		}

		public static float intersect_ray_to_sphere(Vec2 rayOrigin, Vec2 rayDirection, Vec2 center, float radius)
		{
			Vec2 vec = center - rayOrigin;
			float magnitude = vec.get_magnitude();
			float num = vec.dot(rayDirection);
			float num2 = radius * radius - (magnitude * magnitude - num * num);
			if ((double)num2 < 0.0)
			{
				return -1f;
			}
			return num - num2.sqrt();
		}

		public static bool intersect_ellipses(FixEllipse_s a, FixEllipse_s b, out FixVec2 contactPoint)
		{
			contactPoint = a.position / 2 + b.position / 2;
			FixVec2 fixVec = a.radius + b.radius;
			return (a.position / fixVec).get_distance(b.position / fixVec) < 1;
		}

		public static bool intersect_ellipses(FixEllipse_s a, FixEllipse_s b)
		{
			FixVec2 contactPoint;
			return intersect_ellipses(a, b, out contactPoint);
		}

		public static bool ellipses_collision(FixEllipse_s a, FixEllipse_s b, out FixVec2 dispA, out FixVec2 dispB)
		{
			if (!intersect_ellipses(a, b, out var _))
			{
				dispA = FixVec2.zero;
				dispB = FixVec2.zero;
				return false;
			}
			FixVec2 fixVec = b.position - a.position;
			Fix fix = 0;
			if (fixVec.IsZero)
			{
				fixVec.x = 1;
				fix = 0;
			}
			else
			{
				fix = fixVec.normalize_and_get_magnitude_can_be_zero();
			}
			Fix fix2 = a.get_radius(fixVec);
			Fix fix3 = b.get_radius(fixVec);
			Fix fix4 = fix2 + fix3 - fix;
			dispA = -fixVec * fix4;
			dispB = fixVec * fix4;
			return true;
		}

		public static bool is_convex(FixVec2 a, FixVec2 b, FixVec2 c)
		{
			return (b.y - a.y) * (c.x - b.x) - (b.x - a.x) * (c.y - b.y) > 0;
		}

		public static bool is_point_on_triangle(FixVec2[] triangles, FixVec2 point)
		{
			for (int i = 0; i < triangles.Length; i += 3)
			{
				if (is_convex(point, triangles[i], triangles[i + 1]) && is_convex(point, triangles[i + 1], triangles[i + 2]) && is_convex(point, triangles[i + 2], triangles[i]))
				{
					return true;
				}
			}
			return false;
		}

		private static void contract_collisions(ListOfStruct<FixLineSegment2d_s> lineList, FixVec2 offset)
		{
			for (int i = 0; i < lineList.Count; i++)
			{
				if (lineList.Array[i].v2 == lineList.Array[(i + 1) % lineList.Count].v1 && lineList.Array[i].IsVertical == lineList.Array[(i + 1) % lineList.Count].IsVertical)
				{
					lineList.Array[(i + 1) % lineList.Count].isContigousWithPrevious = true;
				}
			}
			int count = lineList.Count;
			for (int j = 0; j < count; j++)
			{
				FixVec2 v = lineList.Array[j].v1;
				FixVec2 v2 = lineList.Array[j].v2;
				FixVec2 n = lineList.Array[j].n;
				bool isVertical = lineList.Array[j].IsVertical;
				if (n.x < 0)
				{
					v.x -= offset.x;
					v2.x -= offset.x;
				}
				if (n.x > 0)
				{
					v.x += offset.x;
					v2.x += offset.x;
				}
				FixLineSegment2d_s item;
				if (isVertical)
				{
					if (n.y < 0)
					{
						v.y -= offset.y;
						v2.y -= offset.y;
					}
					if (n.y > 0)
					{
						v.y += offset.y;
						v2.y += offset.y;
					}
					FixLineSegment2d_s fixLineSegment2d_s = lineList.Array[(j + count - 1) % count];
					FixLineSegment2d_s fixLineSegment2d_s2 = lineList.Array[(j + 1) % count];
					if (!fixLineSegment2d_s.IsVertical && fixLineSegment2d_s.n.x * n.x > 0)
					{
						FixVec2 v3 = v + (v - v2);
						item = new FixLineSegment2d_s(v, v3, n)
						{
							isContigousWithPrevious = false
						};
						lineList.add(item);
					}
					if (!fixLineSegment2d_s2.IsVertical && fixLineSegment2d_s2.n.x * n.x > 0)
					{
						FixVec2 v4 = v2 + (v2 - v);
						item = new FixLineSegment2d_s(v2, v4, n)
						{
							isContigousWithPrevious = false
						};
						lineList.add(item);
					}
				}
				if (lineList.Array[j].isContigousWithPrevious)
				{
					FixVec2 v5 = lineList.Array[(j + count - 1) % count].v2;
					FixVec2 rotated_90_degrees_clockwise = (v - v5).get_rotated_90_degrees_clockwise();
					if (rotated_90_degrees_clockwise.dot(n) > 0)
					{
						item = new FixLineSegment2d_s(v5, v, rotated_90_degrees_clockwise.get_normalized())
						{
							isContigousWithPrevious = true
						};
						lineList.add(item);
					}
				}
				lineList.Array[j].v1 = v;
				lineList.Array[j].v2 = v2;
			}
		}

		private static void scale_collisions(ListOfStruct<FixLineSegment2d_s> lineList, FixVec2 scale)
		{
			for (int i = 0; i < lineList.Count; i++)
			{
				lineList.Array[i].v1 *= scale;
				lineList.Array[i].v2 *= scale;
				lineList.Array[i].n /= scale;
				lineList.Array[i].n.normalize();
			}
		}

		public static void simulate_camera_collision_side_scroller(ListOfStruct<FixLineSegment2d_s> lineList, FixVec2 pos, Fix sizeX, ref Fix sizeY, ref FixVec2 displacement, bool isVerticalScrolling)
		{
			FixVec2 offset = new FixVec2(sizeX, sizeY) / 2;
			if (isVerticalScrolling)
			{
				for (int i = 0; i < lineList.Count; i++)
				{
					lineList.Array[i].v1.swapXY();
					lineList.Array[i].v2.swapXY();
					lineList.Array[i].n.swapXY();
				}
				pos.swapXY();
				offset.swapXY();
				displacement.swapXY();
			}
			contract_collisions(lineList, offset);
			for (int j = 0; j < 2; j++)
			{
				bool flag = j != 0;
				FixVec2 fixVec = pos + displacement;
				Fix fix = Fix.MinValue;
				Fix fix2 = Fix.MinValue;
				Fix fix3 = Fix.MaxValue;
				Fix fix4 = Fix.MaxValue;
				for (int k = 0; k < lineList.Count; k++)
				{
					FixVec2 v = lineList.Array[k].v1;
					FixVec2 v2 = lineList.Array[k].v2;
					FixVec2 n = lineList.Array[k].n;
					bool isVertical = lineList.Array[k].IsVertical;
					if (isVertical && flag && fixVec.x >= min(v.x, v2.x) && fixVec.x <= max(v.x, v2.x))
					{
						FixVec2 fixVec2 = ((v.y == v2.y) ? v : (fixVec2 = v + (v2 - v) * (fixVec - v).x / (v2 - v).x));
						if (n.y < 0)
						{
							if (fixVec2.y < fix4)
							{
								fix4 = fixVec2.y;
							}
						}
						else if (n.y > 0 && fixVec2.y > fix2)
						{
							fix2 = fixVec2.y;
						}
					}
					if (isVertical || flag || !(fixVec.y >= min(v.y, v2.y)) || !(fixVec.y <= max(v.y, v2.y)))
					{
						continue;
					}
					FixVec2 fixVec3 = ((v.x == v2.x) ? v : (fixVec3 = v + (v2 - v) * (fixVec - v).y / (v2 - v).y));
					if (n.x < 0)
					{
						if (fixVec3.x < fix3)
						{
							fix3 = fixVec3.x;
						}
					}
					else if (n.x > 0 && fixVec3.x > fix)
					{
						fix = fixVec3.x;
					}
				}
				if (fix4 < fix2)
				{
					fixVec.y = fix4 / 2 + fix2 / 2;
				}
				else
				{
					fixVec.y = fixVec.y.clamp(fix2, fix4);
				}
				if (fix3 < fix)
				{
					fixVec.x = fix / 2 + fix3 / 2;
				}
				else
				{
					fixVec.x = fixVec.x.clamp(fix, fix3);
				}
				if (fixVec != pos + displacement)
				{
					displacement = fixVec - pos;
				}
			}
			if (isVerticalScrolling)
			{
				pos.swapXY();
				displacement.swapXY();
			}
		}

		public static bool new_simulate_collision(FixEllipse_s startEllipse, ref FixVec2 displacement, ListOfStruct<FixLineSegment2d_s> lineList, List<ContactPoint_s> contactList, Fix noSlidingThreshold, int maxIteration)
		{
			if (lineList == null || lineList.IsEmpty || displacement.IsZero)
			{
				return false;
			}
			FixVec2 fixVec = FixVec2.one / startEllipse.radius;
			scale_collisions(lineList, fixVec);
			FixVec2 start = startEllipse.position * fixVec;
			FixVec2 fixVec2 = displacement * fixVec;
			if (fixVec2.IsZero)
			{
				return false;
			}
			int num = fixVec2.get_magnitude().ceiling_to_int();
			FixVec2 displacement2 = fixVec2 / num;
			displacement = FixVec2.zero;
			bool flag = false;
			for (int i = 0; i < num; i++)
			{
				flag |= simulate_collision_internal(start, ref displacement2, lineList, ref maxIteration, ref contactList, noSlidingThreshold, startEllipse.radius);
				start += displacement2;
				displacement += displacement2 * startEllipse.radius;
			}
			if (contactList != null)
			{
				for (int j = 0; j < contactList.Count; j++)
				{
					ContactPoint_s value = contactList[j];
					value.position *= startEllipse.radius;
					value.normal *= startEllipse.radius;
					contactList[j] = value;
				}
			}
			return flag;
		}

		private static bool simulate_collision_internal(FixVec2 start, ref FixVec2 displacement, ListOfStruct<FixLineSegment2d_s> lineList, ref int iter, ref List<ContactPoint_s> contactList, Fix noSlidingThreshold, FixVec2 toScreen)
		{
			if (displacement.IsZero)
			{
				return false;
			}
			FixVec2 fixVec = start + displacement;
			bool flag = false;
			FixVec2 fixVec2 = default(FixVec2);
			FixVec2 fixVec3 = default(FixVec2);
			uint objectUid = 0u;
			int zoneId = -1;
			Fix fix = Fix.MaxValue;
			for (int i = 0; i < lineList.Count; i++)
			{
				FixVec2 n = lineList.Array[i].n;
				if (!(displacement.dot(n) > 0))
				{
					FixVec2 v = lineList.Array[i].v1;
					FixVec2 v2 = lineList.Array[i].v2;
					FixVec2 c;
					Fix fix2 = point_segment_distance(fixVec, v, v2, ref n, out c);
					if (fix2 < 1 && fix2 < fix && (lineList.Array[i].isInfinite || -fix2 < 1))
					{
						fix = fix2;
						fixVec2 = c;
						fixVec3 = n;
						objectUid = lineList.Array[i].uid;
						zoneId = lineList.Array[i].zoneId;
						flag = true;
					}
				}
			}
			if (!flag)
			{
				return false;
			}
			FixVec2 normalized = displacement.get_normalized();
			Fix fix3 = fixVec3.dot(normalized);
			FixVec2 zero = FixVec2.zero;
			if (fix3 < noSlidingThreshold)
			{
				zero = normalized / fixVec3.dot(normalized) * (1 - fix);
			}
			else
			{
				zero = (fixVec2 + fixVec3 - fixVec) * toScreen;
				FixVec2 normalized2 = (fixVec3 / toScreen).get_normalized();
				zero = normalized2 * normalized2.dot(zero) / toScreen;
			}
			fixVec += zero * new Fix(1, 1L, 1000L);
			if (contactList != null)
			{
				contactList.Add(new ContactPoint_s
				{
					position = fixVec2,
					normal = fixVec3,
					objectUid = objectUid,
					zoneId = zoneId
				});
			}
			FixVec2 rhs = displacement * toScreen;
			FixVec2 fixVec4 = (fixVec - start) * toScreen;
			if (fixVec4.dot(rhs) > 0)
			{
				Fix fix4 = rhs.normalize_and_get_magnitude_can_be_zero();
				Fix fix5 = fixVec4.normalize_and_get_magnitude_can_be_zero();
				if (fix5 > fix4)
				{
					fixVec = start + fixVec4 * fix4 / toScreen;
				}
			}
			displacement = fixVec - start;
			if (iter-- > 0)
			{
				simulate_collision_internal(start, ref displacement, lineList, ref iter, ref contactList, noSlidingThreshold, toScreen);
			}
			return true;
		}

		private static Fix point_segment_distance(FixVec2 p, FixVec2 a, FixVec2 b, ref FixVec2 n, out FixVec2 c)
		{
			if ((b - a).x == 0 && ((p.y > a.y && p.y < b.y) || (p.y < a.y && p.y > b.y)))
			{
				c = new FixVec2(a.x, p.y);
				return (p.x - a.x) * n.x;
			}
			FixVec2 fixVec = a;
			FixVec2 fixVec2 = b;
			FixVec2 fixVec3 = p;
			while (fixVec2.get_manhattan_distance(fixVec) > 180)
			{
				fixVec2 /= (Fix)2;
				fixVec /= (Fix)2;
				fixVec3 /= (Fix)2;
			}
			Fix fix = (fixVec3 - fixVec).dot(fixVec2 - fixVec) / (fixVec2 - fixVec).dot(fixVec2 - fixVec);
			c = ((fix < 0) ? a : ((fix > 1) ? b : (a + fix * (b - a))));
			if (fix < 0 || fix > 1)
			{
				n = (p - c).get_normalized();
			}
			return (p - c).dot(n);
		}
	}
}
