namespace CommonLib
{
	public struct Pair_s<TFirst, TSecond>
	{
		[Tag(1, null, true)]
		public TFirst first;

		[Tag(2, null, true)]
		public TSecond second;
	}
}
