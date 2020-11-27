namespace CommonLib
{
	public struct ContactPoint_s
	{
		[Tag(1, null, true)]
		public FixVec2 position;

		[Tag(2, null, true)]
		public FixVec2 normal;

		[Tag(3, null, true)]
		public uint objectUid;

		[Tag(4, null, true)]
		public int zoneId;
	}
}
