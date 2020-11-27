using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CommonLib
{
	public class DebugDrawingContext
	{
		public enum TextFlagEnum
		{
			alignLeft = 0,
			alignRight = 1,
			alignCenter = 2,
			alignTop = 0,
			alignMiddle = 4,
			alignBottom = 8,
			scaleNone = 0x10,
			scaleFull = 0x20,
			scaleLimited = 0,
			bolden = 0x40,
			Default = 6,
			TopLeft = 0
		}

		private struct Text_s
		{
			public Vector3 pos;

			public Color color;

			public float scale;

			public string text;

			public TextFlagEnum flag;

			public Text_s(string text, Vector3 pos, Color color, float scale = 1f, TextFlagEnum flag = TextFlagEnum.Default)
			{
				this.pos = pos;
				this.scale = scale;
				this.text = text;
				this.color = color;
				this.flag = flag;
			}
		}

		private BasicEffect effect;

		private SpriteBatch spriteBatch;

		private SpriteFont font;

		private ListOfStruct<VertexPositionColor> lineVertexList = new ListOfStruct<VertexPositionColor>(1024);

		private ListOfStruct<VertexPositionColor> triVertexList = new ListOfStruct<VertexPositionColor>(1024);

		private ListOfStruct<Text_s> textList = new ListOfStruct<Text_s>(1024);

		public void clear_and_update()
		{
			lineVertexList.clear();
			triVertexList.clear();
			textList.clear();
		}

		public Vec2 measure_string(string text)
		{
			if (font == null || text == null)
			{
				return Vec2.zero;
			}
			return (Vec2)font.MeasureString(text);
		}

		public void draw(Matrix worldViewProj)
		{
			GraphicsDevice device = graphics.Device;
			graphics.set_states();
			if (effect == null)
			{
				effect = new BasicEffect(device);
				effect.VertexColorEnabled = true;
			}
			if (spriteBatch == null)
			{
				spriteBatch = new SpriteBatch(device);
			}
			if (font == null)
			{
				font = asset_cache.get_default<SpriteFont>();
			}
			effect.View = worldViewProj;
			effect.CurrentTechnique.Passes[0].Apply();
			if (triVertexList.Count != 0)
			{
				device.DrawUserPrimitives(PrimitiveType.TriangleList, triVertexList.Array, 0, triVertexList.Count / 3);
			}
			if (lineVertexList.Count != 0)
			{
				device.DrawUserPrimitives(PrimitiveType.LineList, lineVertexList.Array, 0, lineVertexList.Count / 2);
			}
			if (textList.Count == 0)
			{
				return;
			}
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
			for (int i = 0; i < textList.Count; i++)
			{
				Text_s text_s = textList.Array[i];
				Vector2 vector = ((Vec3)device.Viewport.Project(text_s.pos, worldViewProj, Matrix.Identity, Matrix.Identity)).Xy;
				float num = 1f;
				if (!text_s.flag.HasFlag(TextFlagEnum.scaleNone))
				{
					num = (((Vec3)device.Viewport.Project(text_s.pos + Vec2.one, worldViewProj, Matrix.Identity, Matrix.Identity)).x - vector.X) * 0.01f;
					if (text_s.flag.HasFlag(TextFlagEnum.alignLeft))
					{
						num = math.min(num, 1.5f);
					}
				}
				Vector2 origin = new Vector2(text_s.flag.HasFlag(TextFlagEnum.alignRight) ? 1f : (text_s.flag.HasFlag(TextFlagEnum.alignCenter) ? 0.5f : 0f), text_s.flag.HasFlag(TextFlagEnum.alignBottom) ? 1f : (text_s.flag.HasFlag(TextFlagEnum.alignMiddle) ? 0.5f : 0f));
				origin *= font.MeasureString(text_s.text);
				if (text_s.flag.HasFlag(TextFlagEnum.bolden))
				{
					spriteBatch.DrawString(font, text_s.text, vector + new Vec2(2.5f, 2.5f) * num, text_s.color, 0f, origin, text_s.scale * num, SpriteEffects.None, 0f);
					spriteBatch.DrawString(font, text_s.text, vector + new Vec2(2.5f, -2.5f) * num, text_s.color, 0f, origin, text_s.scale * num, SpriteEffects.None, 0f);
					spriteBatch.DrawString(font, text_s.text, vector - new Vec2(2.5f, 2.5f) * num, text_s.color, 0f, origin, text_s.scale * num, SpriteEffects.None, 0f);
					spriteBatch.DrawString(font, text_s.text, vector - new Vec2(2.5f, -2.5f) * num, text_s.color, 0f, origin, text_s.scale * num, SpriteEffects.None, 0f);
				}
				else
				{
					spriteBatch.DrawString(font, text_s.text, vector, text_s.color, 0f, origin, text_s.scale * num, SpriteEffects.None, 0f);
				}
			}
			spriteBatch.End();
		}

		public void line(FixVec2 a, FixVec2 b, Color_s color)
		{
			line((Vec2)a, (Vec2)b, color);
		}

		public void line(FixVec3 a, FixVec3 b, Color_s color)
		{
			line((Vec3)a, (Vec3)b, color);
		}

		public void line(Vec3 a, Vec3 b, Color_s color)
		{
			lineVertexList.add(new VertexPositionColor(a, color));
			lineVertexList.add(new VertexPositionColor(b, color));
		}

		public void line_fat(FixVec3 a, FixVec3 b, Color_s color, float width)
		{
			line_fat((Vec3)a, (Vec3)b, color, width);
		}

		public void line_fat(Vec3 a, Vec3 b, Color_s color, float width)
		{
			Vec2 vec = (b.Xy - a.Xy).get_normalized_can_be_zero() * width * 0.5f;
			Vec2 rotated_90_degrees_clockwise = vec.get_rotated_90_degrees_clockwise();
			VertexPositionColor[] array = new VertexPositionColor[4]
			{
				new VertexPositionColor(a - vec - rotated_90_degrees_clockwise, color),
				new VertexPositionColor(a - vec + rotated_90_degrees_clockwise, color),
				new VertexPositionColor(b + vec + rotated_90_degrees_clockwise, color),
				new VertexPositionColor(b + vec - rotated_90_degrees_clockwise, color)
			};
			triVertexList.add(array[0]);
			triVertexList.add(array[1]);
			triVertexList.add(array[2]);
			triVertexList.add(array[2]);
			triVertexList.add(array[3]);
			triVertexList.add(array[0]);
		}

		public void cross(Vec2 position, Color_s color, float size = 0.1f)
		{
			line(new Vec3(position - new Vec2(size, 0f), 0f), new Vec3(position + new Vec2(size, 0f), 0f), color);
			line(new Vec3(position - new Vec2(0f, size), 0f), new Vec3(position + new Vec2(0f, size), 0f), color);
		}

		public void cross(FixVec2 position, Color_s color, float size = 0.1f)
		{
			cross((Vec2)position, color, size);
		}

		public void cross(Vec3 position, Color_s color, float size = 0.1f)
		{
			line(position - new Vec3(size, 0f, 0f), position + new Vec3(size, 0f, 0f), color);
			line(position - new Vec3(0f, size, 0f), position + new Vec3(0f, size, 0f), color);
			line(position - new Vec3(0f, 0f, size), position + new Vec3(0f, 0f, size), color);
		}

		public void arrow(FixVec2 a, FixVec2 b, Color_s color, float tipSize = 0.1f)
		{
			arrow((Vec2)a, (Vec2)b, color, tipSize);
		}

		public void arrow(Vec2 a, Vec2 b, Color_s color, float tipSize = 0.1f, float z = 0f)
		{
			Vec2 vec = (b - a).get_normalized_can_be_zero() * tipSize;
			line(new Vec3(a, z), new Vec3(b - vec * 2f, z), color);
			triVertexList.add(new VertexPositionColor(new Vec3(b, z), color));
			triVertexList.add(new VertexPositionColor(new Vec3(b - vec * 2f + vec.get_rotated_90_degrees_clockwise(), z), color));
			triVertexList.add(new VertexPositionColor(new Vec3(b - vec * 2f - vec.get_rotated_90_degrees_clockwise(), z), color));
		}

		public void arrow_fat(Vec2 a, Vec2 b, Color_s color, float width, float tipSize = 0.1f)
		{
			Vec2 vec = (b - a).get_normalized() * tipSize;
			line_fat(a, b - vec * 2f, color, width);
			triVertexList.add(new VertexPositionColor(b, color));
			triVertexList.add(new VertexPositionColor(b - vec * 2f + vec.get_rotated_90_degrees_clockwise(), color));
			triVertexList.add(new VertexPositionColor(b - vec * 2f - vec.get_rotated_90_degrees_clockwise(), color));
		}

		public void rect(BoundingBox2d_s bbox, Color_s color, float z = 0f)
		{
			line(new Vec3(bbox.TopLeft, z), new Vec3(bbox.TopRight, z), color);
			line(new Vec3(bbox.TopRight, z), new Vec3(bbox.BottomRight, z), color);
			line(new Vec3(bbox.BottomRight, z), new Vec3(bbox.BottomLeft, z), color);
			line(new Vec3(bbox.BottomLeft, z), new Vec3(bbox.TopLeft, z), color);
		}

		public void rect(FixBoundingBox2d_s bbox, Color_s color, float z = 0f)
		{
			rect((BoundingBox2d_s)bbox, color, z);
		}

		public void rect(Rect_s rect, Color_s color)
		{
			line(rect.Position, rect.Position + new Vec2(rect.width, 0f), color);
			line(rect.Position + rect.Size, rect.Position + new Vec2(rect.width, 0f), color);
			line(rect.Position + rect.Size, rect.Position + new Vec2(0f, rect.height), color);
			line(rect.Position, rect.Position + new Vec2(0f, rect.height), color);
		}

		public void rect_fat(BoundingBox2d_s bbox, Color_s color, float width, float z = 0f)
		{
			line_fat(new Vec3(bbox.TopLeft + new Vec2(width, 0f), z), new Vec3(bbox.TopRight - new Vec2(width, 0f), z), color, width);
			line_fat(new Vec3(bbox.TopRight, z), new Vec3(bbox.BottomRight, z), color, width);
			line_fat(new Vec3(bbox.BottomRight - new Vec2(width, 0f), z), new Vec3(bbox.BottomLeft + new Vec2(width, 0f), z), color, width);
			line_fat(new Vec3(bbox.BottomLeft, z), new Vec3(bbox.TopLeft, z), color, width);
		}

		public void zone(Zone_s zone, Color_s color, float normals = 0.1f, Vec3 offset = default(Vec3))
		{
			this.zone(zone, FixVec2.zero, color, normals, offset);
		}

		public void zone(Zone_s zone, FixVec2 position, Color_s color, float normals = 0.1f, Vec3 offset = default(Vec3))
		{
			FixVec2[] pointArray = zone.pointArray;
			FixVec2 fixVec = new FixVec2((Fix)offset.x, -(Fix)offset.z);
			int num = pointArray.Length;
			for (int i = 0; i < num; i++)
			{
				FixVec2 fixVec2 = pointArray[i] + fixVec;
				FixVec2 fixVec3 = pointArray[(i + 1) % num] + fixVec;
				line(position + fixVec2, position + fixVec3, color);
				if (normals != 0f && fixVec3 != fixVec2)
				{
					FixVec2 fixVec4 = (fixVec2 + fixVec3) / 2;
					FixVec2 fixVec5 = fixVec3 - fixVec2;
					fixVec5.normalize();
					fixVec5 = (fixVec5 * (Fix)normals).get_rotated_90_degrees_clockwise() + fixVec4;
					line(fixVec4, fixVec5, color);
				}
			}
		}

		public void poly(Vec2[] vertArray, Color_s color)
		{
			int num = vertArray.Length / 3 * 3;
			for (int i = 0; i < num; i += 3)
			{
				tri(vertArray[i], vertArray[i + 1], vertArray[i + 2], color);
			}
		}

		public void poly(FixVec2[] vertArray, Color_s color)
		{
			int num = vertArray.Length / 3 * 3;
			for (int i = 0; i < num; i += 3)
			{
				tri((Vec2)vertArray[i], (Vec2)vertArray[i + 1], (Vec2)vertArray[i + 2], color);
			}
		}

		public void line_list(ListOfStruct<FixLineSegment2d_s> lineList)
		{
			for (int i = 0; i < lineList.Count; i++)
			{
				Color_s color = (lineList.Array[i].IsVertical ? Color_s.green : Color_s.yellow) * 0.25f;
				FixVec2 fixVec = lineList.Array[i].v1 / 2 + lineList.Array[i].v2 / 2;
				line(lineList.Array[i].v1, lineList.Array[i].v2, color);
				line(fixVec, fixVec + lineList.Array[i].n, color);
			}
		}

		public void circle(FixVec2 position, float radius, Color_s color)
		{
			circle((Vec2)position, radius, color);
		}

		public void circle(Vec2 position, float radius, Color_s color)
		{
			Vec2 vec = new Vec2(0f, radius);
			for (int i = 1; i < 22; i++)
			{
				Vec2 vec2 = new Vec2(((float)(i * 2) * (float)Math.PI / 21f).sin(), ((float)(i * 2) * (float)Math.PI / 21f).cos()) * radius;
				line(position + vec, position + vec2, color);
				vec = vec2;
			}
		}

		public void circle_filled(Vec2 position, float radius, Color_s color)
		{
			ellipse_filled(position, new Vec2(radius), color);
		}

		public void ellipse(FixEllipse_s e, Color_s color)
		{
			ellipse((Vec2)e.position, (Vec2)e.radius, color);
		}

		public void ellipse(FixVec2 position, FixVec2 radius, Color_s color)
		{
			ellipse((Vec2)position, (Vec2)radius, color);
		}

		public void ellipse(Ellipse_s e, Color_s color)
		{
			ellipse(e.position, e.radius, color);
		}

		public void ellipse(Vec2 position, Vec2 radius, Color_s color)
		{
			Vec2 vec = new Vec2(0f, radius.y);
			for (int i = 1; i < 22; i++)
			{
				Vec2 vec2 = new Vec2(((float)(i * 2) * (float)Math.PI / 21f).sin(), ((float)(i * 2) * (float)Math.PI / 21f).cos()) * radius;
				line(position + vec, position + vec2, color);
				vec = vec2;
			}
		}

		public void ellipse_filled(FixEllipse_s e, Color_s color)
		{
			ellipse_filled((Vec2)e.position, (Vec2)e.radius, color);
		}

		public void ellipse_filled(FixVec2 position, FixVec2 radius, Color_s color)
		{
			ellipse_filled((Vec2)position, (Vec2)radius, color);
		}

		public void ellipse_filled(Ellipse_s e, Color_s color)
		{
			ellipse_filled(e.position, e.radius, color);
		}

		public void ellipse_filled(Vec2 position, Vec2 radius, Color_s color)
		{
			Vec2 vec = new Vec2(0f, radius.y);
			for (int i = 1; i < 22; i++)
			{
				Vec2 vec2 = new Vec2(((float)(i * 2) * (float)Math.PI / 21f).sin(), ((float)(i * 2) * (float)Math.PI / 21f).cos()) * radius;
				tri(position, position + vec, position + vec2, color);
				vec = vec2;
			}
		}

		public void quad(Rect_s rect, Color_s color, float z = 0f)
		{
			Vector3 position = new Vector3(rect.x, rect.y, z);
			Vector3 position2 = new Vector3(rect.x + rect.width, rect.y, z);
			Vector3 position3 = new Vector3(rect.x, rect.y + rect.height, z);
			Vector3 position4 = new Vector3(rect.x + rect.width, rect.y + rect.height, z);
			triVertexList.add(new VertexPositionColor(position, color));
			triVertexList.add(new VertexPositionColor(position2, color));
			triVertexList.add(new VertexPositionColor(position3, color));
			triVertexList.add(new VertexPositionColor(position2, color));
			triVertexList.add(new VertexPositionColor(position4, color));
			triVertexList.add(new VertexPositionColor(position3, color));
		}

		public void quad(BoundingBox2d_s bbox, Color_s color, float z = 0f)
		{
			quad(new Rect_s(bbox), color, z);
		}

		public void tri(Vec2 a, Vec2 b, Vec2 c, Color_s color)
		{
			tri((Vec3)a, (Vec3)b, (Vec3)c, color);
		}

		public void tri(Vec3 a, Vec3 b, Vec3 c, Color_s color)
		{
			triVertexList.add(new VertexPositionColor(a, color));
			triVertexList.add(new VertexPositionColor(b, color));
			triVertexList.add(new VertexPositionColor(c, color));
		}

		public void text(string text, Vec3 pos, Color_s color, float scale = 1f, TextFlagEnum flag = TextFlagEnum.Default)
		{
			if (text != null)
			{
				textList.add(new Text_s(text, pos, color, scale, flag));
			}
		}
	}
}
