namespace CommonLib
{
	[ProtoRoot(false)]
	public struct NetworkData_s
	{
		[Tag(1, null, true)]
		public MachineId_s dataSender;

		[Tag(2, null, true)]
		public byte[] sentData;

		[Tag(3, null, true)]
		public int sentDataLength;

		[Tag(4, null, true)]
		public int secondsTimestamp;

		[Tag(5, null, true)]
		public int millisecondsTimestamp;

		internal NetworkData_s(MachineId_s sender, byte[] data, int length, int seconds, int milliseconds)
		{
			dataSender = sender;
			sentData = data;
			sentDataLength = length;
			secondsTimestamp = seconds;
			millisecondsTimestamp = milliseconds;
		}

		public bool is_valid()
		{
			return sentDataLength != 0;
		}
	}
}
