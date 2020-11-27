using System.Collections.Generic;

namespace CommonLib
{
	public struct Zone_s
	{
		[Tag(1, null, true)]
		public FixVec2[] pointArray;

		public FixVec2[] triangleArray;

		public Zone_s(Rect_s rect)
		{
			pointArray = new FixVec2[4]
			{
				new FixVec2(rect.Position - rect.Size / 2f),
				new FixVec2(rect.Position - rect.Size / 2f * new Vec2(1f, -1f)),
				new FixVec2(rect.Position + rect.Size / 2f),
				new FixVec2(rect.Position + rect.Size / 2f * new Vec2(1f, -1f))
			};
			triangleArray = null;
		}

		public FixVec2 point_at(int i)
		{
			if (i > 0 && i < pointArray.Length)
			{
				return pointArray[i];
			}
			return pointArray[(i + pointArray.Length) % pointArray.Length];
		}

		public Zone_s create_clone()
		{
			Zone_s result = default(Zone_s);
			result.pointArray = pointArray.create_clone();
			return result;
		}

		public bool simulate_collision(FixEllipse_s startEllipse, ref FixVec2 displacement, out FixVec2 normal)
		{
			int iter = 0;
			normal = FixVec2.zero;
			if (pointArray == null || pointArray.Length == 0)
			{
				return false;
			}
			if (displacement.IsZero)
			{
				return false;
			}
			Fix magnitude = displacement.get_magnitude();
			Fix fix = startEllipse.radius.get_magnitude() / 2;
			int num = (magnitude / fix).ceiling_to_int();
			FixVec2 displacement2 = displacement / num;
			displacement = FixVec2.zero;
			for (int i = 0; i < num; i++)
			{
				bool flag = simulate_collision_internal(startEllipse, ref displacement2, ref iter, ref normal, null);
				displacement += displacement2;
				startEllipse.position += displacement2;
				if (flag)
				{
					normal.normalize_can_be_zero();
					return true;
				}
			}
			return false;
		}

		private bool simulate_collision_internal(FixEllipse_s startEllipse, ref FixVec2 displacement, ref int iter, ref FixVec2 normal, DebugDrawingContext debugDraw)
		{
			FixVec2 fixVec = FixVec2.one / startEllipse.radius;
			FixVec2 fixVec2 = startEllipse.position * fixVec;
			FixVec2 fixVec3 = displacement * fixVec;
			FixVec2 fixVec4 = fixVec2 + fixVec3;
			bool flag = false;
			FixVec2 fixVec5 = default(FixVec2);
			FixVec2 fixVec6 = default(FixVec2);
			Fix fix = Fix.MaxValue;
			for (int i = 0; i < pointArray.Length; i++)
			{
				int num = i;
				int num2 = ((i != pointArray.Length - 1) ? (i + 1) : 0);
				FixVec2 fixVec7 = pointArray[num] * fixVec;
				FixVec2 fixVec8 = pointArray[num2] * fixVec;
				FixVec2 normalized = (fixVec8 - fixVec7).get_rotated_90_degrees_clockwise().get_normalized();
				if (fixVec3.dot(normalized) > 0)
				{
					continue;
				}
				FixVec2 fixVec9 = fixVec8 - fixVec7;
				Fix magnitude = fixVec9.get_magnitude();
				fixVec9 /= magnitude;
				Fix fix2 = fixVec9.dot(fixVec4 - fixVec7);
				if (fix2 < -Fix.One / 10000 || fix2 > magnitude + Fix.One / 10000)
				{
					FixVec2 fixVec10 = ((fix2 < 0) ? fixVec7 : fixVec8);
					Fix fix3 = fixVec10.get_distance(fixVec4);
					if (fix3 < 1 && fix3 < fix)
					{
						fix = fix3;
						fixVec5 = fixVec10;
						fixVec6 = fixVec4 - fixVec10;
						flag = true;
					}
				}
				else
				{
					FixVec2 fixVec11 = fixVec7 + fixVec9 * fix2;
					Fix fix4 = fixVec11.get_distance(fixVec4);
					if (fix4 < 1 && fix4 < fix)
					{
						fix = fix4;
						fixVec5 = fixVec11;
						fixVec6 = normalized;
						flag = true;
					}
				}
			}
			if (!flag)
			{
				return false;
			}
			fixVec4 = fixVec5 + fixVec6.get_normalized() * new Fix(10001, 10000);
			normal += fixVec6;
			Fix fix5 = (fixVec4 - (fixVec2 + fixVec3)).dot(fixVec4 - fixVec2);
			if (fix5 > new Fix(1, 10000))
			{
				fix5 /= (fixVec4 - fixVec2).get_magnitude();
				fixVec4 -= (fixVec4 - fixVec2).get_normalized() * fix5;
			}
			displacement = (fixVec4 - fixVec2) / fixVec;
			if (++iter < 256)
			{
				simulate_collision_internal(startEllipse, ref displacement, ref iter, ref normal, debugDraw);
			}
			return true;
		}

		public BoundingBox2d_s get_bounds()
		{
			BoundingBox2d_s result = default(BoundingBox2d_s);
			if (pointArray == null)
			{
				return result;
			}
			bool flag = false;
			FixVec2[] array = pointArray;
			for (int i = 0; i < array.Length; i++)
			{
				Vec2 point = (Vec2)array[i];
				if (!flag)
				{
					result = new BoundingBox2d_s(point);
					flag = true;
				}
				else
				{
					result.union(new BoundingBox2d_s(point));
				}
			}
			return result;
		}

		public void translate(FixVec2 v)
		{
			if (pointArray != null)
			{
				for (int i = 0; i < pointArray.Length; i++)
				{
					pointArray[i] += v;
				}
			}
		}

		public void translate(Vec2 v)
		{
			translate((FixVec2)v);
		}

		public Zone_s get_inverted()
		{
			Zone_s zone_s = default(Zone_s);
			zone_s.pointArray = new FixVec2[pointArray.Length];
			Zone_s result = zone_s;
			for (int i = 0; i < result.pointArray.Length; i++)
			{
				result.pointArray[i] = pointArray[pointArray.Length - 1 - i];
			}
			return result;
		}

		public void add_potential_collision_lines(ListOfStruct<FixLineSegment2d_s> lineList, FixBoundingBox2d_s bb, uint uid = 0u, int zoneId = -1, FixVec2 offset = default(FixVec2), bool inverseNormals = false)
		{
			bb = bb.get_translated(-offset);
			for (int i = 0; i < pointArray.Length; i++)
			{
				FixBoundingBox2d_s other = FixBoundingBox2d_s.new_from_points(pointArray[i], pointArray[(i + 1) % pointArray.Length]);
				if (!bb.intersects(other))
				{
					continue;
				}
				FixVec2 fixVec = pointArray[i] + offset;
				FixVec2 fixVec2 = pointArray[(i + 1) % pointArray.Length] + offset;
				if (!(fixVec == fixVec2))
				{
					if (inverseNormals)
					{
						lineList.add(new FixLineSegment2d_s(fixVec2, fixVec, default(FixVec2), uid, zoneId));
					}
					else
					{
						lineList.add(new FixLineSegment2d_s(fixVec, fixVec2, default(FixVec2), uid, zoneId));
					}
				}
			}
		}

		public bool is_point_inside(FixVec2 pos)
		{
			int num = 0;
			for (int i = 0; i < pointArray.Length; i++)
			{
				FixVec2 fixVec = pointArray[i];
				FixVec2 fixVec2 = pointArray[(i + 1) % pointArray.Length];
				int num2 = 1;
				if (fixVec.y > fixVec2.y)
				{
					FixVec2 fixVec3 = fixVec;
					fixVec = fixVec2;
					fixVec2 = fixVec3;
					num2 = -num2;
				}
				if (fixVec.y < pos.y && fixVec2.y > pos.y && (fixVec.x < pos.x || fixVec2.x < pos.x) && ((fixVec.x < pos.x && fixVec2.x < pos.x) || (pos.y - fixVec.y) / (fixVec2.y - fixVec.y) * (fixVec2.x - fixVec.x) + fixVec.x < pos.x))
				{
					num += num2;
				}
			}
			return num != 0;
		}

		public FixVec2[] get_triangulated()
		{
			if (triangleArray != null)
			{
				return triangleArray;
			}
			List<FixVec2> list = new List<FixVec2>(pointArray);
			List<FixVec2> list2 = new List<FixVec2>(pointArray.Length - 2);
			List<bool> list3 = new List<bool>(pointArray.Length);
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				bool flag = math.is_convex(list[(i + list.Count - 1) % list.Count], list[i], list[(i + 1) % list.Count]);
				list3.Add(flag);
				if (flag)
				{
					num++;
				}
			}
			if (num < list3.Count / 2)
			{
				list.Reverse();
			}
			int num2 = 0;
			int num3 = list.Count - 1;
			while (list.Count > 2)
			{
				int index = num2 % list.Count;
				int index2 = (num2 + 1) % list.Count;
				int index3 = (num2 + 2) % list.Count;
				int index4 = (num2 + list.Count - 1) % list.Count;
				int index5 = (num2 + 3) % list.Count;
				if (list.Count == 3)
				{
					list2.AddRange(list);
					list.Clear();
					continue;
				}
				if (num3 == num2)
				{
					if (math.is_convex(list[index], list[index2], list[index3]))
					{
						list2.Add(list[index]);
						list2.Add(list[index2]);
						list2.Add(list[index3]);
					}
					list.RemoveAt(index);
					num3 = list.Count - 1;
					continue;
				}
				if (list3[index2])
				{
					bool flag2 = true;
					FixVec2[] array = pointArray;
					foreach (FixVec2 point in array)
					{
						if (math.is_point_on_triangle(new FixVec2[3]
						{
							list[index],
							list[index2],
							list[index3]
						}, point))
						{
							flag2 = false;
							break;
						}
					}
					if (flag2)
					{
						list2.Add(list[index]);
						list2.Add(list[index2]);
						list2.Add(list[index3]);
						list3[index] = math.is_convex(list[index4], list[index], list[index3]);
						list3[index3] = math.is_convex(list[index], list[index3], list[index5]);
						list.RemoveAt(index2);
						list3.RemoveAt(index2);
						num3 = num2 % list.Count;
					}
				}
				num2 = (num2 + 1) % list.Count;
			}
			triangleArray = list2.ToArray();
			return triangleArray;
		}

		public FixVec2[] triangulated_naive()
		{
			FixVec2[] array = new FixVec2[(pointArray.Length - 2) * 3];
			for (int i = 0; i < pointArray.Length - 2; i++)
			{
				array[i * 3] = pointArray[i];
				array[i * 3 + 1] = pointArray[i + 1];
				array[i * 3 + 2] = pointArray[i + 2];
			}
			return array;
		}
	}
}
