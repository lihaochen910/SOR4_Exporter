using System;

namespace CommonLib
{
	public struct FixBoundingBox2d_s : IEquatable<FixBoundingBox2d_s>
	{
		[Tag(1, null, true)]
		public FixVec2 min;

		[Tag(2, null, true)]
		public FixVec2 max;

		public static FixBoundingBox2d_s invalidInfinite = new FixBoundingBox2d_s
		{
			min = new FixVec2(Fix.MaxValue),
			max = new FixVec2(Fix.MinValue)
		};

		public FixVec2 Center => (min + max) / 2;

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

		public Fix Top => max.y;

		public Fix Bottom => min.y;

		public FixVec2 TopLeft => min;

		public FixVec2 TopRight => new FixVec2(max.x, min.y);

		public FixVec2 BottomLeft => new FixVec2(min.x, max.y);

		public FixVec2 BottomRight => max;

		public Fix Width => max.x - min.x;

		public Fix Height => max.y - min.y;

		public FixVec2 Size => max - min;

		public FixBoundingBox2d_s(FixVec2 minMax)
		{
			min = minMax;
			max = minMax;
		}

		public FixBoundingBox2d_s(FixVec2 min, FixVec2 max)
		{
			this.min = min;
			this.max = max;
		}

		public FixBoundingBox2d_s(Fix minX, Fix minY, Fix maxX, Fix maxY)
		{
			min.x = minX;
			min.y = minY;
			max.x = maxX;
			max.y = maxY;
		}

		public static FixBoundingBox2d_s new_from_points(FixVec2 v1, FixVec2 v2)
		{
			return new FixBoundingBox2d_s(v1.get_min(v2), v1.get_max(v2));
		}

		public static FixBoundingBox2d_s new_from_points(FixVec2 v1, FixVec2 v2, FixVec2 v3, FixVec2 v4)
		{
			return new FixBoundingBox2d_s(v1.get_min(v2).get_min(v3).get_min(v4), v1.get_max(v2).get_max(v3).get_max(v4));
		}

		public static FixBoundingBox2d_s new_expand_around(FixVec2 center, FixVec2 size)
		{
			return new FixBoundingBox2d_s(new FixVec2(center.x - size.x / 2, center.y - size.y / 2), new FixVec2(center.x + size.x / 2, center.y + size.y / 2));
		}

		public static FixBoundingBox2d_s new_expand_around(FixVec2 center, Fix distanceX, Fix distanceY)
		{
			return new FixBoundingBox2d_s(new FixVec2(center.x - distanceX, center.y - distanceY), new FixVec2(center.x + distanceX, center.y + distanceY));
		}

		public static explicit operator BoundingBox2d_s(FixBoundingBox2d_s value)
		{
			return new BoundingBox2d_s((Vec2)value.min, (Vec2)value.max);
		}

		public static explicit operator FixBoundingBox2d_s(BoundingBox2d_s value)
		{
			return new FixBoundingBox2d_s((FixVec2)value.min, (FixVec2)value.max);
		}

		public static bool operator ==(FixBoundingBox2d_s lhs, FixBoundingBox2d_s rhs)
		{
			if (lhs.min == rhs.min)
			{
				return lhs.max == rhs.max;
			}
			return false;
		}

		public static bool operator !=(FixBoundingBox2d_s lhs, FixBoundingBox2d_s rhs)
		{
			if (!(lhs.min != rhs.min))
			{
				return lhs.max != rhs.max;
			}
			return true;
		}

		public static FixBoundingBox2d_s operator *(FixBoundingBox2d_s value1, FixVec2 value2)
		{
			value1.min *= value2;
			value1.max *= value2;
			return value1;
		}

		public static FixBoundingBox2d_s operator /(FixBoundingBox2d_s value1, FixVec2 value2)
		{
			value1.min /= value2;
			value1.max /= value2;
			return value1;
		}

		public override int GetHashCode()
		{
			return min.GetHashCode() + max.GetHashCode();
		}

		public bool Equals(FixBoundingBox2d_s other)
		{
			if (min == other.min)
			{
				return max == other.max;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is FixBoundingBox2d_s)
			{
				return Equals((FixBoundingBox2d_s)obj);
			}
			return false;
		}

		public bool is_point_inside(FixVec2 point)
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

		public void expand(Fix distance)
		{
			min.x -= distance;
			min.y -= distance;
			max.x += distance;
			max.y += distance;
		}

		public void expand(FixVec2 distance)
		{
			min.x -= distance.x;
			min.y -= distance.y;
			max.x += distance.x;
			max.y += distance.y;
		}

		public FixBoundingBox2d_s get_expanded(FixVec2 distance)
		{
			FixBoundingBox2d_s result = this;
			result.expand(distance);
			return result;
		}

		public bool intersects(FixBoundingBox2d_s other)
		{
			if (min.x <= other.max.x && min.y <= other.max.y && other.min.x <= max.x)
			{
				return other.min.y <= max.y;
			}
			return false;
		}

		public FixBoundingBox2d_s get_intersection(FixBoundingBox2d_s other)
		{
			FixBoundingBox2d_s result = default(FixBoundingBox2d_s);
			result.max.x = math.min(max.x, other.max.x);
			result.min.x = math.max(min.x, other.min.x);
			result.max.y = math.min(max.y, other.max.y);
			result.min.y = math.max(min.y, other.min.y);
			return result;
		}

		public void translate(FixVec2 v)
		{
			min += v;
			max += v;
		}

		public FixBoundingBox2d_s get_translated(FixVec2 v)
		{
			return new FixBoundingBox2d_s(min + v, max + v);
		}

		public void union(FixBoundingBox2d_s other)
		{
			min = min.get_min(other.min);
			max = max.get_max(other.max);
		}

		public void union(FixVec2 displacement)
		{
			FixBoundingBox2d_s other = get_translated(displacement);
			union(other);
		}

		public FixBoundingBox2d_s get_union(FixBoundingBox2d_s other)
		{
			FixBoundingBox2d_s result = this;
			result.union(other);
			return result;
		}

		public void rotate_radians(Fix angle)
		{
			this = new_from_points(TopLeft.get_rotated_radians(angle), TopRight.get_rotated_radians(angle), BottomLeft.get_rotated_radians(angle), BottomRight.get_rotated_radians(angle));
		}

		public void flip_x()
		{
			this = new FixBoundingBox2d_s(-max.x, min.y, -min.x, max.y);
		}
	}
}
