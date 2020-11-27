public class CyclicTimer
{
	private float timerDuration = 1f;

	private float timerAccum;

	public CyclicTimer(float duration)
	{
		timerAccum = (timerDuration = duration);
	}

	public void update(float delta)
	{
		timerAccum -= delta;
	}

	public bool check_and_reset()
	{
		if (timerAccum <= 0f)
		{
			timerAccum = timerDuration;
			return true;
		}
		return false;
	}
}
