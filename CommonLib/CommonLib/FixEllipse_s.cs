using System;

namespace CommonLib
{
	public struct FixEllipse_s
	{
		[Tag(1, null, true)]
		public FixVec2 position;

		[Tag(2, null, true)]
		public FixVec2 radius;

		public FixEllipse_s(FixVec2 position, FixVec2 radius)
		{
			this.position = position;
			this.radius = radius;
		}

		public static explicit operator Ellipse_s(FixEllipse_s value)
		{
			return new Ellipse_s((Vec2)value.position, (Vec2)value.radius);
		}

		[Obsolete]
		public Ellipse_s obsolete_float()
		{
			return (Ellipse_s)this;
		}

		public bool is_point_inside(FixVec2 point)
		{
			if (radius.x == 0 || radius.y == 0)
			{
				return false;
			}
			FixVec2 fixVec = point - position;
			return (fixVec / radius).get_magnitude() <= 1;
		}

		public Fix get_radius(FixVec2 direction)
		{
			direction /= radius;
			direction.normalize();
			direction *= radius;
			return direction.get_magnitude();
		}
	}
}
