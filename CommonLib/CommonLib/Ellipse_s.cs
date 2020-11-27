namespace CommonLib
{
	public struct Ellipse_s
	{
		[Tag(1, null, true)]
		public Vec2 position;

		[Tag(2, null, true)]
		public Vec2 radius;

		public Ellipse_s(Vec2 position, Vec2 radius)
		{
			this.position = position;
			this.radius = radius;
		}
	}
}
