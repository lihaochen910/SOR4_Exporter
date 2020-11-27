using System;

namespace CommonLib
{
	public class DragAttribute : Attribute
	{
		public float speed = 1f;

		public float min;

		public float max;

		public float power = 1f;

		public DragAttribute(float speed = 1f, float min = 0f, float max = 0f, float power = 1f)
		{
			this.speed = speed;
			this.min = min;
			this.max = max;
			this.power = power;
		}
	}
}
