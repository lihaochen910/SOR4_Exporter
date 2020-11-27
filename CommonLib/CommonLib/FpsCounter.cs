using System.Diagnostics;

namespace CommonLib
{
	public class FpsCounter
	{
		public float[] lastValueArray = new float[64];

		public int lastValueRoot;

		private int frames;

		private long fps_duration;

		private Stopwatch timer;

		private const int refresh = 500;

		public float FpsRate
		{
			get;
			private set;
		}

		public void update()
		{
			if (timer == null)
			{
				timer = Stopwatch.StartNew();
			}
			frames++;
			if (frames > 30)
			{
				FpsRate = 1000f * (float)frames / (float)timer.ElapsedMilliseconds;
				frames = 0;
				timer.Restart();
				lastValueArray[lastValueRoot] = FpsRate;
				if (++lastValueRoot > lastValueArray.Length - 1)
				{
					lastValueRoot = 0;
				}
			}
		}
	}
}
