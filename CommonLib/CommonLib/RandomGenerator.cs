using System;

namespace CommonLib
{
	public class RandomGenerator
	{
		private const int MBIG = int.MaxValue;

		private const int MSEED = 161803398;

		private const int MZ = 0;

		[Tag(1, null, true)]
		public int inext;

		[Tag(2, null, true)]
		public int inextp;

		[Tag(3, null, true)]
		public int[] seedArray;

		public Fix get_fix(Fix min, Fix max)
		{
			long num = max.raw - min.raw;
			num *= InternalSample();
			num /= int.MaxValue;
			return min + new Fix((int)num);
		}

		public Fix get_fix_around(Fix baseValue, Fix spread)
		{
			return get_fix(baseValue - spread, baseValue + spread);
		}

		public float get_float()
		{
			return (float)InternalSample() * 4.656613E-10f;
		}

		public float get_float(float max)
		{
			return get_float() * max;
		}

		public float get_float(float min, float max)
		{
			return get_float() * (max - min) + min;
		}

		public int get_int_inclusive(int min, int max)
		{
			return Next(min, max + 1);
		}

		public int get_int_exclusive(int max)
		{
			return Next(0, max);
		}

		public float get_float_around(float baseValue, float spread)
		{
			return baseValue - spread + get_float() * 2f * spread;
		}

		public float get_float_around(float baseValue, float minSpread, float maxSpread)
		{
			float num = maxSpread - minSpread;
			float num2 = 0f - num + get_float() * 2f * num;
			if (num2 > 0f)
			{
				return baseValue + minSpread + num2;
			}
			return baseValue - minSpread + num2;
		}

		public Vec2 get_direction()
		{
			float f = get_float((float)Math.PI * 2f);
			return new Vec2(f.cos(), f.sin());
		}

		public FixVec2 get_fix_direction()
		{
			Fix angle = get_fix(0, 360);
			return new FixVec2(math.cos_degrees(angle), math.sin_degrees(angle));
		}

		public bool get_bool()
		{
			return InternalSample() > 1073741823;
		}

		public void init()
		{
			init(Environment.TickCount);
		}

		public void init(int Seed)
		{
			seedArray = new int[56];
			int num = ((Seed == int.MinValue) ? int.MaxValue : Math.Abs(Seed));
			int num2 = 161803398 - num;
			seedArray[55] = num2;
			int num3 = 1;
			for (int i = 1; i < 55; i++)
			{
				int num4 = 21 * i % 55;
				seedArray[num4] = num3;
				num3 = num2 - num3;
				if (num3 < 0)
				{
					num3 += int.MaxValue;
				}
				num2 = seedArray[num4];
			}
			for (int j = 1; j < 5; j++)
			{
				for (int k = 1; k < 56; k++)
				{
					seedArray[k] -= seedArray[1 + (k + 30) % 55];
					if (seedArray[k] < 0)
					{
						seedArray[k] += int.MaxValue;
					}
				}
			}
			inext = 0;
			inextp = 21;
			Seed = 1;
		}

		protected virtual double Sample()
		{
			return (double)InternalSample() * 4.6566128752457969E-10;
		}

		private int InternalSample()
		{
			if (seedArray == null)
			{
				init();
			}
			int num = inext;
			int num2 = inextp;
			if (++num >= 56)
			{
				num = 1;
			}
			if (++num2 >= 56)
			{
				num2 = 1;
			}
			int num3 = seedArray[num] - seedArray[num2];
			if (num3 == int.MaxValue)
			{
				num3--;
			}
			if (num3 < 0)
			{
				num3 += int.MaxValue;
			}
			seedArray[num] = num3;
			inext = num;
			inextp = num2;
			return num3;
		}

		protected virtual int Next()
		{
			return InternalSample();
		}

		private double GetSampleForLargeRange()
		{
			int num = InternalSample();
			if ((InternalSample() % 2 == 0) ? true : false)
			{
				num = -num;
			}
			double num2 = num;
			num2 += 2147483646.0;
			return num2 / 4294967293.0;
		}

		protected virtual int Next(int minValue, int maxValue)
		{
			if (minValue > maxValue)
			{
				throw new ArgumentOutOfRangeException("minValue");
			}
			long num = (long)maxValue - (long)minValue;
			if (num <= int.MaxValue)
			{
				return (int)(Sample() * (double)num) + minValue;
			}
			return (int)((long)(GetSampleForLargeRange() * (double)num) + minValue);
		}

		protected virtual int Next(int maxValue)
		{
			if (maxValue < 0)
			{
				throw new ArgumentOutOfRangeException("maxValue");
			}
			return (int)(Sample() * (double)maxValue);
		}

		protected virtual double NextDouble()
		{
			return Sample();
		}

		protected virtual void NextBytes(byte[] buffer)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			for (int i = 0; i < buffer.Length; i++)
			{
				buffer[i] = (byte)(InternalSample() % 256);
			}
		}
	}
}
