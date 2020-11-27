using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CommonLib
{
	public struct EffectParameterRef_s
	{
		private string effectPath;

		private string parameterName;

		private Effect effect;

		private EffectParameter parameter;

		public EffectParameter RawEffectParameter
		{
			get
			{
				if (effect == null || effect.IsDisposed)
				{
					parameter = null;
					effect = asset_cache.get<Effect>(effectPath);
				}
				if (parameter == null)
				{
					parameter = effect.Parameters[parameterName];
				}
				return parameter;
			}
		}

		public EffectParameterRef_s(string effectPath, string parameterName)
		{
			this.effectPath = effectPath;
			this.parameterName = parameterName;
			effect = null;
			parameter = null;
		}

		public void set_value(bool v)
		{
			RawEffectParameter?.SetValue(v);
		}

		public void set_value(float v)
		{
			RawEffectParameter?.SetValue(v);
		}

		public void set_value(Matrix v)
		{
			RawEffectParameter?.SetValue(v);
		}

		public void set_value(Vector4 v)
		{
			RawEffectParameter?.SetValue(v);
		}

		public void set_value(Vector3 v)
		{
			RawEffectParameter?.SetValue(v);
		}

		public void set_value(Vector2 v)
		{
			RawEffectParameter?.SetValue(v);
		}

		public void set_value(float[] v)
		{
			RawEffectParameter?.SetValue(v);
		}

		public void set_value(Vector3[] v)
		{
			RawEffectParameter?.SetValue(v);
		}

		public void set_value(Vector4[] v)
		{
			RawEffectParameter?.SetValue(v);
		}

		public void set_value(Texture v)
		{
			RawEffectParameter?.SetValue(v);
		}
	}
}
