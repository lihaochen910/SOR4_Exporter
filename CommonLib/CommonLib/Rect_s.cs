using Microsoft.Xna.Framework;

namespace CommonLib
{
	public struct Rect_s : IYamlCustomSerialization
	{
		[Tag(1, null, true)]
		public float x;

		[Tag(2, null, true)]
		public float y;

		[Tag(3, null, true)]
		public float width;

		[Tag(4, null, true)]
		public float height;

		public Vec2 Position
		{
			get
			{
				return new Vec2(x, y);
			}
			set
			{
				x = value.x;
				y = value.y;
			}
		}

		public Vec2 Size
		{
			get
			{
				return new Vec2(width, height);
			}
			set
			{
				width = value.x;
				height = value.y;
			}
		}

		public Rect_s(float x, float y, float width, float height)
		{
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
		}

		public Rect_s(Vec2 pos, Vec2 size)
		{
			x = pos.x;
			y = pos.y;
			width = size.x;
			height = size.y;
		}

		public Rect_s(BoundingBox2d_s bb)
		{
			x = bb.min.x;
			y = bb.min.y;
			width = bb.max.x - bb.min.x;
			height = bb.max.y - bb.min.y;
		}

		public static implicit operator Rectangle(Rect_s v)
		{
			return new Rectangle((int)v.x, (int)v.y, (int)v.width, (int)v.height);
		}

		public string yaml_serialize()
		{
			return utils.pack_to_string(x, y, width, height);
		}

		public void yaml_deserialize(string src)
		{
			utils.unpack_from_string(src, out x, out y, out width, out height);
		}
	}
}
