namespace CommonLib
{
	public struct FixModifier_s
	{
		[Tag(1, null, true)]
		public ValueModificationEnum modification;

		[Tag(2, null, true)]
		public Fix value;

		public void execute(ref Fix v)
		{
			switch (modification)
			{
			case ValueModificationEnum.replace:
				v = value;
				break;
			case ValueModificationEnum.add:
				v += value;
				break;
			case ValueModificationEnum.min:
				v = math.min(v, value);
				break;
			case ValueModificationEnum.max:
				v = math.max(v, value);
				break;
			case ValueModificationEnum.none:
				break;
			}
		}
	}
}
