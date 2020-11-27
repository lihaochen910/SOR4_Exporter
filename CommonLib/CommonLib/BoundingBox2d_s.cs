using System;
using Microsoft.Xna.Framework;

namespace CommonLib
{
	public struct BoundingBox2d_s : IYamlCustomSerialization, IEquatable<BoundingBox2d_s>
	{
		[Tag(1, null, true)]
		public Vec2 min;

		[Tag(2, null, true)]
		public Vec2 max;

		public static BoundingBox2d_s invalidInfinite = new BoundingBox2d_s
		{
			min = new Vec2(float.MaxValue),
			max = new Vec2(float.MinValue)
		};

		public Vec2 Center => new Vec2((min.x + max.x) * 0.5f, (min.y + max.y) * 0.5f);

		public Vec2 Size
		{
			get
			{
				return new Vec2(max.x - min.x, max.y - min.y);
			}
			set
			{
				max.x = min.x + value.x;
				max.y = min.y + value.y;
			}
		}

		public float Left => min.x;

		public float Right => max.x;

		public float Top => max.y;

		public float Bottom => min.y;

		public float YPointsDownTop => min.y;

		public float YPointsDownBottom => max.y;

		public Vec2 TopLeft => new Vec2(min.x, max.y);

		public Vec2 TopRight => max;

		public Vec2 BottomLeft => min;

		public Vec2 BottomRight => new Vec2(max.x, min.y);

		public Vec2 YPointsDownTopLeft => min;

		public Vec2 YPointsDownTopRight => new Vec2(max.x, min.y);

		public Vec2 YPointsDownBottomLeft => new Vec2(min.x, max.y);

		public Vec2 YPointsDownBottomRight => max;

		public bool IsPoint => min == max;

		public bool IsValid
		{
			get
			{
				if (!(min != min) && !(max != max))
				{
					if (min == invalidInfinite.min)
					{
						return !(max == invalidInfinite.max);
					}
					return true;
				}
				return false;
			}
		}

		public float Width => Right - Left;

		public float Height => Top - Bottom;

		public BoundingBox2d_s(Vec2 min, Vec2 max)
		{
			this.min = min;
			this.max = max;
		}

		public BoundingBox2d_s(Vec2 point)
		{
			min = point;
			max = point;
		}

		public BoundingBox2d_s(float minX, float minY, float maxX, float maxY)
		{
			min = new Vec2(minX, minY);
			max = new Vec2(maxX, maxY);
		}

		public BoundingBox2d_s(BoundingBox bb)
		{
			min = new Vec2(bb.Min.X, bb.Min.Y);
			max = new Vec2(bb.Max.X, bb.Max.Y);
		}

		public static explicit operator Rectangle(BoundingBox2d_s bb)
		{
			return new Rectangle((int)bb.min.x, (int)bb.min.y, (int)bb.Width, (int)bb.Height);
		}

		public static BoundingBox2d_s new_expand_around(Vec2 center, float distance)
		{
			return new BoundingBox2d_s(new Vec2(center.x - distance, center.y - distance), new Vec2(center.x + distance, center.y + distance));
		}

		public static BoundingBox2d_s new_expand_around(Vec2 center, Vec2 size)
		{
			return new BoundingBox2d_s(new Vec2(center.x - size.x * 0.5f, center.y - size.y * 0.5f), new Vec2(center.x + size.x * 0.5f, center.y + size.y * 0.5f));
		}

		public static BoundingBox2d_s new_from_points(Vec2 v1, Vec2 v2)
		{
			return new BoundingBox2d_s(v1.get_min(v2), v1.get_max(v2));
		}

		public static BoundingBox2d_s new_from_points(Vec2 v1, Vec2 v2, Vec2 v3, Vec2 v4)
		{
			return new BoundingBox2d_s(v1.get_min(v2).get_min(v3).get_min(v4), v1.get_max(v2).get_max(v3).get_max(v4));
		}

		public static BoundingBox2d_s new_zero()
		{
			BoundingBox2d_s result = default(BoundingBox2d_s);
			result.min = new Vec2(float.MaxValue, float.MaxValue);
			result.max = new Vec2(float.MinValue, float.MinValue);
			return result;
		}

		public void union(BoundingBox2d_s other)
		{
			min = min.get_min(other.min);
			max = max.get_max(other.max);
		}

		public void union(Vec2 displacement)
		{
			BoundingBox2d_s other = get_translated(displacement);
			union(other);
		}

		public BoundingBox2d_s get_union(BoundingBox2d_s other)
		{
			BoundingBox2d_s result = this;
			result.union(other);
			return result;
		}

		public BoundingBox2d_s get_union(Vec2 displacement)
		{
			BoundingBox2d_s result = this;
			result.union(displacement);
			return result;
		}

		public void expand(float distance)
		{
			min.x -= distance;
			min.y -= distance;
			max.x += distance;
			max.y += distance;
		}

		public BoundingBox2d_s get_expanded(float distance)
		{
			BoundingBox2d_s result = this;
			result.expand(distance);
			return result;
		}

		public bool is_point_inside(Vec2 point)
		{
			if (point.x < min.x)
			{
				return false;
			}
			if (point.x > max.x)
			{
				return false;
			}
			if (point.y > max.y)
			{
				return false;
			}
			if (point.y < min.y)
			{
				return false;
			}
			return true;
		}

		public bool intersects(BoundingBox2d_s other)
		{
			if (min.x <= other.max.x && min.y <= other.max.y && other.min.x <= max.x)
			{
				return other.min.y <= max.y;
			}
			return false;
		}

		public bool intersects(ref BoundingBox2d_s other)
		{
			if (min.x <= other.max.x && min.y <= other.max.y && other.min.x <= max.x)
			{
				return other.min.y <= max.y;
			}
			return false;
		}

		public void add_point(Vec2 v)
		{
			min.set_min(v);
			max.set_max(v);
		}

		public void translate(Vec2 v)
		{
			min += v;
			max += v;
		}

		public void rotate(float radians)
		{
			this = new_from_points(TopLeft.get_rotated(radians), TopRight.get_rotated(radians), BottomLeft.get_rotated(radians), BottomRight.get_rotated(radians));
		}

		public BoundingBox2d_s get_translated(Vec2 v)
		{
			BoundingBox2d_s result = this;
			result.translate(v);
			return result;
		}

		public BoundingBox2d_s get_projected(Matrix viewProj)
		{
			return new_from_points(min.get_projected(viewProj), max.get_projected(viewProj));
		}

		public void flip_x()
		{
			this = new BoundingBox2d_s(0f - max.x, min.y, 0f - min.x, max.y);
		}

		public override string ToString()
		{
			return yaml_serialize();
		}

		public string yaml_serialize()
		{
			return utils.pack_to_string(min.x, min.y, max.x, max.y);
		}

		public void yaml_deserialize(string src)
		{
			utils.unpack_from_string(src, out min.x, out min.y, out max.x, out max.y);
		}

		public static BoundingBox2d_s operator +(BoundingBox2d_s value1, Vec2 value2)
		{
			value1.min += value2;
			value1.max += value2;
			return value1;
		}

		public static BoundingBox2d_s operator +(BoundingBox2d_s value1, float value2)
		{
			value1.min += value2;
			value1.max += value2;
			return value1;
		}

		public static BoundingBox2d_s operator -(BoundingBox2d_s value1, Vec2 value2)
		{
			value1.min -= value2;
			value1.max -= value2;
			return value1;
		}

		public static BoundingBox2d_s operator -(BoundingBox2d_s value1, float value2)
		{
			value1.min -= value2;
			value1.max -= value2;
			return value1;
		}

		public static BoundingBox2d_s operator *(BoundingBox2d_s value1, Vec2 value2)
		{
			value1.min *= value2;
			value1.max *= value2;
			return value1;
		}

		public static BoundingBox2d_s operator *(BoundingBox2d_s value1, float value2)
		{
			value1.min *= value2;
			value1.max *= value2;
			return value1;
		}

		public static BoundingBox2d_s operator /(BoundingBox2d_s value1, Vec2 value2)
		{
			value1.min /= value2;
			value1.max /= value2;
			return value1;
		}

		public static BoundingBox2d_s operator /(BoundingBox2d_s value1, float value2)
		{
			value1.min /= value2;
			value1.max /= value2;
			return value1;
		}

		public static bool operator ==(BoundingBox2d_s value1, BoundingBox2d_s value2)
		{
			if (value1.min == value2.min)
			{
				return value1.max == value2.max;
			}
			return false;
		}

		public static bool operator !=(BoundingBox2d_s value1, BoundingBox2d_s value2)
		{
			return !(value1 == value2);
		}

		public override int GetHashCode()
		{
			return min.GetHashCode() + max.GetHashCode();
		}

		public bool Equals(BoundingBox2d_s other)
		{
			if (min == other.min)
			{
				return max == other.max;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is BoundingBox2d_s))
			{
				return false;
			}
			return Equals((BoundingBox2d_s)obj);
		}
	}
}
