using System;

namespace CommonLib
{
	public struct FixLineSegment2d_s
	{
		[ThreadStatic]
		private static ListOfStruct<FixLineSegment2d_s> tempList;

		public FixVec2 v1;

		public FixVec2 v2;

		public FixVec2 n;

		public uint uid;

		public int zoneId;

		public bool isContigousWithPrevious;

		public bool isInfinite;

		public static ListOfStruct<FixLineSegment2d_s> TempList
		{
			get
			{
				utils.initialize_thread_static_list(ref tempList, 16);
				return tempList;
			}
		}

		public bool IsVertical => math.abs(n.y) * 4 > math.abs(n.x);

		public FixLineSegment2d_s(FixVec2 v1, FixVec2 v2, FixVec2 n = default(FixVec2), uint uid = 0u, int zoneId = -1, bool infinite = false)
		{
			this.v1 = v1;
			this.v2 = v2;
			this.n = ((n != default(FixVec2)) ? n : (v2 - v1).get_rotated_90_degrees_clockwise().get_normalized());
			this.uid = uid;
			this.zoneId = zoneId;
			isContigousWithPrevious = false;
			isInfinite = infinite;
		}
	}
}
