using System;

namespace CommonLib
{
	public class TooltipAttribute : Attribute
	{
		public readonly string text;

		public readonly bool questionMark;

		public TooltipAttribute(string text, bool questionMark = false)
		{
			this.text = text;
			this.questionMark = questionMark;
		}
	}
}
