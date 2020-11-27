using Microsoft.Xna.Framework;

namespace CommonLib
{
	public struct FixBoundingBox3d_s
	{
		[Tag(1, null, true)]
		public FixVec3 min;

		[Tag(2, null, true)]
		public FixVec3 max;

		public static FixBoundingBox3d_s invalidInfinite = new FixBoundingBox3d_s
		{
			min = new FixVec3(Fix.MaxValue, Fix.MaxValue, Fix.MaxValue),
			max = new FixVec3(Fix.MinValue, Fix.MinValue, Fix.MinValue)
		};

		public Fix Width => max.x - min.x;

		public FixVec3 Center => (min + max) / 2;

		public FixVec3 Size => max - min;

		public bool IsZero
		{
			get
			{
				if (min.IsZero)
				{
					return max.IsZero;
				}
				return false;
			}
		}

		public Fix Left
		{
			get
			{
				return min.x;
			}
			set
			{
				min.x = value;
			}
		}

		public Fix Right
		{
			get
			{
				return max.x;
			}
			set
			{
				max.x = value;
			}
		}

		public FixBoundingBox3d_s(FixVec3 min, FixVec3 max)
		{
			this.min = min;
			this.max = max;
		}

		public FixBoundingBox3d_s(Fix minX, Fix minY, Fix minZ, Fix maxX, Fix maxY, Fix maxZ)
		{
			min.x = minX;
			min.y = minY;
			min.z = minZ;
			max.x = maxX;
			max.y = maxY;
			max.z = maxZ;
		}

		public FixBoundingBox3d_s(FixBoundingBox2d_s bb, Fix minZ, Fix maxZ)
		{
			min = new FixVec3(bb.min, minZ);
			max = new FixVec3(bb.max, maxZ);
		}

		public static FixBoundingBox3d_s new_from_points(FixVec3 v1, FixVec3 v2)
		{
			return new FixBoundingBox3d_s(v1.get_min(v2), v1.get_max(v2));
		}

		public static FixBoundingBox3d_s new_from_points(FixVec3 v1, FixVec3 v2, FixVec3 v3, FixVec3 v4)
		{
			return new FixBoundingBox3d_s(v1.get_min(v2).get_min(v3).get_min(v4), v1.get_max(v2).get_max(v3).get_max(v4));
		}

		public void translate(FixVec3 t)
		{
			min += t;
			max += t;
		}

		public void translate(FixVec2 t)
		{
			min += t;
			max += t;
		}

		public static explicit operator BoundingBox(FixBoundingBox3d_s value)
		{
			return new BoundingBox((Vector3)value.min, (Vector3)value.max);
		}

		public static explicit operator FixBoundingBox3d_s(BoundingBox value)
		{
			return new FixBoundingBox3d_s((FixVec3)value.Min, (FixVec3)value.Max);
		}

		public static explicit operator FixBoundingBox2d_s(FixBoundingBox3d_s value)
		{
			return new FixBoundingBox2d_s((FixVec2)value.min, (FixVec2)value.max);
		}

		public static FixBoundingBox3d_s operator *(FixBoundingBox3d_s value1, FixVec2 value2)
		{
			value1.min *= value2;
			value1.max *= value2;
			return value1;
		}

		public bool intersects(FixBoundingBox3d_s other)
		{
			if (min.x <= other.max.x && min.y <= other.max.y && min.z <= other.max.z && other.min.x <= max.x && other.min.y <= max.y)
			{
				return other.min.z <= max.z;
			}
			return false;
		}

		public FixBoundingBox3d_s intersection(FixBoundingBox3d_s other)
		{
			return new FixBoundingBox3d_s(new FixVec3(math.max(min.x, other.min.x), math.max(min.y, other.min.y), math.max(min.z, other.min.z)), new FixVec3(math.min(max.x, other.max.x), math.min(max.y, other.max.y), math.min(max.z, other.max.z)));
		}

		public void rotate_radians(Fix angle)
		{
			FixBoundingBox2d_s bb = (FixBoundingBox2d_s)this;
			bb.rotate_radians(angle);
			this = new FixBoundingBox3d_s(bb, min.z, max.z);
		}

		public void flip_x()
		{
			this = new FixBoundingBox3d_s(-max.x, min.y, min.z, -min.x, max.y, max.z);
		}

		public void scale_centered(Fix x, Fix y, Fix z)
		{
			FixVec3 size = Size;
			size.x *= x;
			size.y *= y;
			size.z *= z;
			FixVec3 center = Center;
			min.x = center.x - size.x / 2;
			min.y = center.y - size.y / 2;
			min.z = center.z - size.z / 2;
			max.x = center.x + size.x / 2;
			max.y = center.y + size.y / 2;
			max.z = center.z + size.z / 2;
		}
	}
}
