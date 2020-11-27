namespace CommonLib
{
	public struct IntRect_s : IYamlCustomSerialization
	{
		[Tag(1, null, true)]
		public int x;

		[Tag(2, null, true)]
		public int y;

		[Tag(3, null, true)]
		public int width;

		[Tag(4, null, true)]
		public int height;

		public static IntRect_s invalidInfinite = new IntRect_s
		{
			x = int.MaxValue,
			y = int.MaxValue,
			width = int.MinValue,
			height = int.MinValue
		};

		public IntVec2 Position
		{
			get
			{
				return new IntVec2(x, y);
			}
			set
			{
				x = value.x;
				y = value.y;
			}
		}

		public IntVec2 Size
		{
			get
			{
				return new IntVec2(width, height);
			}
			set
			{
				width = value.x;
				height = value.y;
			}
		}

		public int Left => x;

		public int Right => x + width;

		public int Top => y;

		public int Bottom => y + height;

		public IntRect_s(int x, int y, int width, int height)
		{
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
		}

		public IntRect_s(IntVec2 position, IntVec2 size)
		{
			x = position.x;
			y = position.y;
			width = size.x;
			height = size.y;
		}

		public void yaml_deserialize(string src)
		{
			utils.unpack_from_string(src, out x, out y, out width, out height);
		}

		public string yaml_serialize()
		{
			return utils.pack_to_string(x, y, width, height);
		}

		public void union(IntRect_s other)
		{
			int a = x + width;
			int a2 = y + height;
			int b = other.x + other.width;
			int b2 = other.y + other.height;
			x = math.min(x, other.x);
			y = math.min(y, other.y);
			a = math.max(a, b);
			a2 = math.max(a2, b2);
			width = a - x;
			height = a2 - y;
		}

		public static implicit operator Rect_s(IntRect_s v)
		{
			return new Rect_s(v.x, v.y, v.width, v.height);
		}

		public static IntRect_s operator *(IntRect_s value, int scaleFactor)
		{
			value.x *= scaleFactor;
			value.y *= scaleFactor;
			value.width *= scaleFactor;
			value.height *= scaleFactor;
			return value;
		}

		public static IntRect_s operator *(int scaleFactor, IntRect_s value)
		{
			value.x *= scaleFactor;
			value.y *= scaleFactor;
			value.width *= scaleFactor;
			value.height *= scaleFactor;
			return value;
		}
	}
}
