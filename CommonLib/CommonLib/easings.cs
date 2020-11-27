using System;

namespace CommonLib
{
	public static class easings
	{
		public static Fix interpolate(Fix p, CurveKeyTypeEnum function)
		{
			switch (function)
			{
			case CurveKeyTypeEnum.once:
			case CurveKeyTypeEnum.hold:
				return 0;
			case CurveKeyTypeEnum.linear:
				return p;
			case CurveKeyTypeEnum.easeIn:
				return p * p;
			case CurveKeyTypeEnum.easeOut:
				return 1 - (1 - p) * (1 - p);
			case CurveKeyTypeEnum.smooth:
				return p * p * (3 - 2 * p);
			default:
				utils.log_write_line("Easings", "Asking for unsupported interpolation type: " + function, LogImportanceEnum.warning);
				return p;
			}
		}

		public static float interpolate(float p, CurveKeyTypeEnum function)
		{
			return function switch
			{
				CurveKeyTypeEnum.linear => linear(p), 
				CurveKeyTypeEnum.easeIn => ease_in(p), 
				CurveKeyTypeEnum.easeOut => ease_out(p), 
				CurveKeyTypeEnum.smooth => smooth(p), 
				CurveKeyTypeEnum.quadraticEaseInOut => quadratic_ease_in_out(p), 
				CurveKeyTypeEnum.cubicEaseIn => cubic_ease_in(p), 
				CurveKeyTypeEnum.cubicEaseOut => cubic_ease_out(p), 
				CurveKeyTypeEnum.cubicEaseInOut => cubic_ease_in_out(p), 
				CurveKeyTypeEnum.quarticEaseIn => quartic_ease_in(p), 
				CurveKeyTypeEnum.quarticEaseOut => quartic_ease_out(p), 
				CurveKeyTypeEnum.quarticEaseInOut => quartic_ease_in_out(p), 
				CurveKeyTypeEnum.quinticEaseIn => quintic_ease_in(p), 
				CurveKeyTypeEnum.quinticEaseOut => quintic_ease_out(p), 
				CurveKeyTypeEnum.quinticEaseInOut => quintic_ease_in_out(p), 
				CurveKeyTypeEnum.sineEaseIn => sine_ease_in(p), 
				CurveKeyTypeEnum.sineEaseOut => sine_ease_out(p), 
				CurveKeyTypeEnum.sineEaseInOut => sine_ease_in_out(p), 
				CurveKeyTypeEnum.circularEaseIn => circular_ease_in(p), 
				CurveKeyTypeEnum.circularEaseOut => circular_ease_out(p), 
				CurveKeyTypeEnum.circularEaseInOut => circular_ease_in_out(p), 
				CurveKeyTypeEnum.exponentialEaseIn => exponential_ease_in(p), 
				CurveKeyTypeEnum.exponentialEaseOut => exponential_ease_out(p), 
				CurveKeyTypeEnum.exponentialEaseInOut => exponential_ease_in_out(p), 
				CurveKeyTypeEnum.elasticEaseIn => elastic_ease_in(p), 
				CurveKeyTypeEnum.elasticEaseOut => elastic_ease_out(p), 
				CurveKeyTypeEnum.elasticEaseInOut => elastic_ease_in_out(p), 
				CurveKeyTypeEnum.backEaseIn => back_ease_in(p), 
				CurveKeyTypeEnum.backEaseOut => back_ease_out(p), 
				CurveKeyTypeEnum.backEaseInOut => back_ease_in_out(p), 
				CurveKeyTypeEnum.bounceEaseIn => bounce_ease_in(p), 
				CurveKeyTypeEnum.bounceEaseOut => bounce_ease_out(p), 
				CurveKeyTypeEnum.bounceEaseInOut => bounce_ease_in_out(p), 
				_ => 0f, 
			};
		}

		public static float linear(float p)
		{
			return p;
		}

		public static float ease_in(float p)
		{
			return p * p;
		}

		public static float ease_out(float p)
		{
			return 1f - (1f - p) * (1f - p);
		}

		public static float smooth(float p)
		{
			return p * p * (3f - 2f * p);
		}

		public static float quadratic_ease_in(float p)
		{
			return p * p;
		}

		public static float quadratic_ease_out(float p)
		{
			return 0f - p * (p - 2f);
		}

		public static float quadratic_ease_in_out(float p)
		{
			if (p < 0.5f)
			{
				return 2f * p * p;
			}
			return -2f * p * p + 4f * p - 1f;
		}

		public static float cubic_ease_in(float p)
		{
			return p * p * p;
		}

		public static float cubic_ease_out(float p)
		{
			float num = p - 1f;
			return num * num * num + 1f;
		}

		public static float cubic_ease_in_out(float p)
		{
			if (p < 0.5f)
			{
				return 4f * p * p * p;
			}
			float num = 2f * p - 2f;
			return 0.5f * num * num * num + 1f;
		}

		public static float quartic_ease_in(float p)
		{
			return p * p * p * p;
		}

		public static float quartic_ease_out(float p)
		{
			float num = p - 1f;
			return num * num * num * (1f - p) + 1f;
		}

		public static float quartic_ease_in_out(float p)
		{
			if (p < 0.5f)
			{
				return 8f * p * p * p * p;
			}
			float num = p - 1f;
			return -8f * num * num * num * num + 1f;
		}

		public static float quintic_ease_in(float p)
		{
			return p * p * p * p * p;
		}

		public static float quintic_ease_out(float p)
		{
			float num = p - 1f;
			return num * num * num * num * num + 1f;
		}

		public static float quintic_ease_in_out(float p)
		{
			if (p < 0.5f)
			{
				return 16f * p * p * p * p * p;
			}
			float num = 2f * p - 2f;
			return 0.5f * num * num * num * num * num + 1f;
		}

		public static float sine_ease_in(float p)
		{
			return ((p - 1f) * ((float)Math.PI / 2f)).sin() + 1f;
		}

		public static float sine_ease_out(float p)
		{
			return (p * ((float)Math.PI / 2f)).sin();
		}

		public static float sine_ease_in_out(float p)
		{
			return 0.5f * (1f - (p * (float)Math.PI).cos());
		}

		public static float circular_ease_in(float p)
		{
			return 1f - (1f - p * p).sqrt();
		}

		public static float circular_ease_out(float p)
		{
			return ((2f - p) * p).sqrt();
		}

		public static float circular_ease_in_out(float p)
		{
			if (p < 0.5f)
			{
				return 0.5f * (1f - (1f - 4f * (p * p)).sqrt());
			}
			return 0.5f * (((0f - (2f * p - 3f)) * (2f * p - 1f)).sqrt() + 1f);
		}

		public static float exponential_ease_in(float p)
		{
			if (p != 0f)
			{
				return math.pow(2f, 10f * (p - 1f));
			}
			return p;
		}

		public static float exponential_ease_out(float p)
		{
			if (p != 1f)
			{
				return 1f - math.pow(2f, -10f * p);
			}
			return p;
		}

		public static float exponential_ease_in_out(float p)
		{
			if ((double)p == 0.0 || (double)p == 1.0)
			{
				return p;
			}
			if (p < 0.5f)
			{
				return 0.5f * math.pow(2f, 20f * p - 10f);
			}
			return -0.5f * math.pow(2f, -20f * p + 10f) + 1f;
		}

		public static float elastic_ease_in(float p)
		{
			return (20.4203529f * p).sin() * math.pow(2f, 10f * (p - 1f));
		}

		public static float elastic_ease_out(float p)
		{
			return (-20.4203529f * (p + 1f)).sin() * math.pow(2f, -10f * p) + 1f;
		}

		public static float elastic_ease_in_out(float p)
		{
			if (p < 0.5f)
			{
				return 0.5f * (20.4203529f * (2f * p)).sin() * math.pow(2f, 10f * (2f * p - 1f));
			}
			return 0.5f * ((-20.4203529f * (2f * p - 1f + 1f)).sin() * math.pow(2f, -10f * (2f * p - 1f)) + 2f);
		}

		public static float back_ease_in(float p)
		{
			return p * p * p - p * (p * (float)Math.PI).sin();
		}

		public static float back_ease_out(float p)
		{
			float num = 1f - p;
			return 1f - (num * num * num - num * (num * (float)Math.PI).sin());
		}

		public static float back_ease_in_out(float p)
		{
			if (p < 0.5f)
			{
				float num = 2f * p;
				return 0.5f * (num * num * num - num * (num * (float)Math.PI).sin());
			}
			float num2 = 1f - (2f * p - 1f);
			return 0.5f * (1f - (num2 * num2 * num2 - num2 * (num2 * (float)Math.PI).sin())) + 0.5f;
		}

		public static float bounce_ease_in(float p)
		{
			return 1f - bounce_ease_out(1f - p);
		}

		public static float bounce_ease_out(float p)
		{
			if (p < 0.363636374f)
			{
				return 121f * p * p / 16f;
			}
			if (p < 0.727272749f)
			{
				return 9.075f * p * p - 9.9f * p + 3.4f;
			}
			if (p < 0.9f)
			{
				return 12.0664816f * p * p - 19.635458f * p + 8.898061f;
			}
			return 10.8f * p * p - 20.52f * p + 10.72f;
		}

		public static float bounce_ease_in_out(float p)
		{
			if (p < 0.5f)
			{
				return 0.5f * bounce_ease_in(p * 2f);
			}
			return 0.5f * bounce_ease_out(p * 2f - 1f) + 0.5f;
		}
	}
}
