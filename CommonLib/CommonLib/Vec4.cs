using Microsoft.Xna.Framework;

namespace CommonLib
{
	public struct Vec4 : IYamlCustomSerialization
	{
		[Tag(1, null, true)]
		public float x;

		[Tag(2, null, true)]
		public float y;

		[Tag(3, null, true)]
		public float z;

		[Tag(4, null, true)]
		public float w;

		public static Vec4 zero = new Vec4(0f, 0f, 0f, 0f);

		public static Vec4 one = new Vec4(1f, 1f, 1f, 1f);

		public bool IsZero
		{
			get
			{
				if (x == 0f && y == 0f && z == 0f)
				{
					return w == 0f;
				}
				return false;
			}
		}

		public Vec2 Xy => new Vec2(x, y);

		public Vec3 Xyz => new Vec3(x, y, z);

		public Vec4(float x, float y, float z, float w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		public Vec4(float x, float y)
		{
			this.x = x;
			this.y = y;
			z = 0f;
			w = 0f;
		}

		public Vec4(Vec2 v, float z, float w)
		{
			x = v.x;
			y = v.y;
			this.z = z;
			this.w = w;
		}

		public Vec4(Vec3 v, float w)
		{
			x = v.x;
			y = v.y;
			z = v.z;
			this.w = w;
		}

		public float normalize_and_get_magnitude()
		{
			float magnitude = get_magnitude();
			this /= magnitude;
			return magnitude;
		}

		public Vec4 get_normalized_can_be_zero()
		{
			float magnitude = get_magnitude();
			if (magnitude == 0f)
			{
				return zero;
			}
			return new Vec4(x / magnitude, y / magnitude, z / magnitude, w / magnitude);
		}

		public float get_magnitude_squared()
		{
			return x * x + y * y + z * z + w * w;
		}

		public float get_magnitude()
		{
			return get_magnitude_squared().sqrt();
		}

		public void clamp_magnitude(float maxMagnitude)
		{
			float magnitude = get_magnitude();
			if (magnitude != 0f && magnitude > maxMagnitude)
			{
				this /= magnitude;
				this *= maxMagnitude;
			}
		}

		public Vec4 get_with_w(float w)
		{
			return new Vec4(x, y, z, w);
		}

		public float get_distance(Vec4 other)
		{
			return (this - other).get_magnitude();
		}

		public float dot(Vec4 other)
		{
			return x * other.x + y * other.y + z * other.z + w * other.w;
		}

		public override string ToString()
		{
			return utils.pack_to_string(x, y, z, w);
		}

		public string yaml_serialize()
		{
			return ToString();
		}

		public void yaml_deserialize(string src)
		{
			utils.unpack_from_string(src, out x, out y, out z, out w);
		}

		public static implicit operator Vector2(Vec4 v)
		{
			return new Vector2(v.x, v.y);
		}

		public static implicit operator Vector3(Vec4 v)
		{
			return new Vector3(v.x, v.y, v.z);
		}

		public static implicit operator Vector4(Vec4 v)
		{
			return new Vector4(v.x, v.y, v.z, v.w);
		}

		public static implicit operator Vec4(Vector4 v)
		{
			return new Vec4(v.X, v.Y, v.Z, v.W);
		}

		public static Vec4 operator +(Vec4 value1, Vec4 value2)
		{
			value1.x += value2.x;
			value1.y += value2.y;
			value1.z += value2.z;
			value1.w += value2.w;
			return value1;
		}

		public static Vec4 operator +(Vec4 value1, float value2)
		{
			value1.x += value2;
			value1.y += value2;
			value1.z += value2;
			value1.w += value2;
			return value1;
		}

		public static Vec4 operator -(Vec4 value1, Vec4 value2)
		{
			value1.x -= value2.x;
			value1.y -= value2.y;
			value1.z -= value2.z;
			value1.w -= value2.w;
			return value1;
		}

		public static Vec4 operator -(Vec4 value1, float value2)
		{
			value1.x -= value2;
			value1.y -= value2;
			value1.z -= value2;
			value1.w -= value2;
			return value1;
		}

		public static Vec4 operator -(Vec4 value1)
		{
			value1.x = 0f - value1.x;
			value1.y = 0f - value1.y;
			value1.z = 0f - value1.z;
			value1.w = 0f - value1.w;
			return value1;
		}

		public static Vec4 operator *(Vec4 value, float scaleFactor)
		{
			value.x *= scaleFactor;
			value.y *= scaleFactor;
			value.z *= scaleFactor;
			value.w *= scaleFactor;
			return value;
		}

		public static Vec4 operator *(Vec4 value1, Vec4 value2)
		{
			value1.x *= value2.x;
			value1.y *= value2.y;
			value1.z *= value2.z;
			value1.w *= value2.w;
			return value1;
		}

		public static Vec4 operator /(Vec4 value, float scaleFactor)
		{
			value.x /= scaleFactor;
			value.y /= scaleFactor;
			value.z /= scaleFactor;
			value.w /= scaleFactor;
			return value;
		}

		public static Vec4 operator /(Vec4 value1, Vec4 value2)
		{
			value1.x /= value2.x;
			value1.y /= value2.y;
			value1.z /= value2.z;
			value1.w /= value2.w;
			return value1;
		}
	}
}
