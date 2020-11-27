using Microsoft.Xna.Framework;

namespace CommonLib
{
	public struct Vec3 : IYamlCustomSerialization
	{
		[Tag(1, null, true)]
		public float x;

		[Tag(2, null, true)]
		public float y;

		[Tag(3, null, true)]
		public float z;

		public static Vec3 zero = new Vec3(0f, 0f, 0f);

		public static Vec3 one = new Vec3(1f, 1f, 1f);

		public bool IsZero
		{
			get
			{
				if (x == 0f && y == 0f)
				{
					return z == 0f;
				}
				return false;
			}
		}

		public Vec2 Xy
		{
			get
			{
				return new Vec2(x, y);
			}
			set
			{
				x = value.x;
				y = value.y;
			}
		}

		public Vec2 Xz
		{
			get
			{
				return new Vec2(x, z);
			}
			set
			{
				x = value.x;
				z = value.y;
			}
		}

		public Vec3(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public Vec3(float x, float y)
		{
			this.x = x;
			this.y = y;
			z = 0f;
		}

		public Vec3(Vec2 v, float z)
		{
			x = v.x;
			y = v.y;
			this.z = z;
		}

		public float normalize_and_get_magnitude()
		{
			float num = (x * x + y * y + z * z).sqrt();
			x /= num;
			y /= num;
			z /= num;
			return num;
		}

		public Vec3 get_normalized_can_be_zero()
		{
			float num = (x * x + y * y + z * z).sqrt();
			if (num == 0f)
			{
				return zero;
			}
			return new Vec3(x / num, y / num, z / num);
		}

		public float get_magnitude_squared()
		{
			return x * x + y * y + z * z;
		}

		public float get_magnitude()
		{
			return (x * x + y * y + z * z).sqrt();
		}

		public void clamp_magnitude(float maxMagnitude)
		{
			float num = (x * x + y * y + z * z).sqrt();
			if (num != 0f && num > maxMagnitude)
			{
				x /= num;
				y /= num;
				z /= num;
				x *= maxMagnitude;
				y *= maxMagnitude;
				z *= maxMagnitude;
			}
		}

		public Vec3 get_with_z(float z)
		{
			return new Vec3(x, y, z);
		}

		public float get_distance(Vec3 other)
		{
			float num = other.x - x;
			float num2 = other.y - y;
			float num3 = other.z - z;
			return (num * num + num2 * num2 + num3 * num3).sqrt();
		}

		public float dot(Vec3 other)
		{
			return x * other.x + y * other.y + z * other.z;
		}

		public Vec2 get_projected(Matrix viewProj)
		{
			float num = viewProj.M14 * x + viewProj.M24 * y + viewProj.M34 * z + viewProj.M44;
			return new Vec2((viewProj.M11 * x + viewProj.M21 * y + viewProj.M31 * z + viewProj.M41) / num, (viewProj.M12 * x + viewProj.M22 * y + viewProj.M32 * z + viewProj.M42) / num);
		}

		public override string ToString()
		{
			return utils.pack_to_string(x, y, z);
		}

		public string yaml_serialize()
		{
			return ToString();
		}

		public void yaml_deserialize(string src)
		{
			utils.unpack_from_string(src, out x, out y, out z);
		}

		public static implicit operator Vec3(Vec2 v)
		{
			return new Vec3(v.x, v.y);
		}

		public static implicit operator Vector2(Vec3 v)
		{
			return new Vector2(v.x, v.y);
		}

		public static implicit operator Vector3(Vec3 v)
		{
			return new Vector3(v.x, v.y, v.z);
		}

		public static implicit operator Vec3(Vector3 v)
		{
			return new Vec3(v.X, v.Y, v.Z);
		}

		public static Vec3 operator +(Vec3 value1, Vec3 value2)
		{
			value1.x += value2.x;
			value1.y += value2.y;
			value1.z += value2.z;
			return value1;
		}

		public static Vec3 operator +(Vec3 value1, float value2)
		{
			value1.x += value2;
			value1.y += value2;
			value1.z += value2;
			return value1;
		}

		public static Vec3 operator -(Vec3 value1, Vec3 value2)
		{
			value1.x -= value2.x;
			value1.y -= value2.y;
			value1.z -= value2.z;
			return value1;
		}

		public static Vec3 operator -(Vec3 value1, float value2)
		{
			value1.x -= value2;
			value1.y -= value2;
			value1.z -= value2;
			return value1;
		}

		public static Vec3 operator -(Vec3 value1)
		{
			value1.x = 0f - value1.x;
			value1.y = 0f - value1.y;
			value1.z = 0f - value1.z;
			return value1;
		}

		public static Vec3 operator +(Vec3 value1, Vec2 value2)
		{
			value1.x += value2.x;
			value1.y += value2.y;
			return value1;
		}

		public static Vec3 operator *(Vec3 value, float scaleFactor)
		{
			value.x *= scaleFactor;
			value.y *= scaleFactor;
			value.z *= scaleFactor;
			return value;
		}

		public static Vec3 operator *(Vec3 value1, Vec3 value2)
		{
			value1.x *= value2.x;
			value1.y *= value2.y;
			value1.z *= value2.z;
			return value1;
		}

		public static Vec3 operator /(Vec3 value, float scaleFactor)
		{
			value.x /= scaleFactor;
			value.y /= scaleFactor;
			value.z /= scaleFactor;
			return value;
		}

		public static Vec3 operator /(Vec3 value1, Vec3 value2)
		{
			value1.x /= value2.x;
			value1.y /= value2.y;
			value1.z /= value2.z;
			return value1;
		}

		public static bool operator ==(Vec3 value1, Vec3 value2)
		{
			if (value1.x == value2.x && value1.y == value2.y)
			{
				return value1.z == value2.z;
			}
			return false;
		}

		public static bool operator !=(Vec3 value1, Vec3 value2)
		{
			if (value1.x == value2.x && value1.y == value2.y)
			{
				return value1.z != value2.z;
			}
			return true;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Vec3))
			{
				return false;
			}
			return this == (Vec3)obj;
		}

		public override int GetHashCode()
		{
			return (int)(x + y * 251f + z * 251f * 251f);
		}
	}
}
