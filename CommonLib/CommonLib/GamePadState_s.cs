namespace CommonLib
{
	public struct GamePadState_s
	{
		public bool isConnected;

		public float stickX;

		public float stickY;

		internal bool a;

		internal bool b;

		internal bool x;

		internal bool y;

		internal bool lb;

		internal bool rb;

		internal bool lt;

		internal bool rt;

		internal bool start;

		internal bool back;

		public bool is_pressed(GamePadButtonEnum button)
		{
			return button switch
			{
				GamePadButtonEnum.A_Cross_B_DownButton => a, 
				GamePadButtonEnum.B_Circle_A_RightButton => b, 
				GamePadButtonEnum.X_Square_Y_LeftButton => x, 
				GamePadButtonEnum.Y_Triangle_X_UpButton => y, 
				GamePadButtonEnum.LB_L1_L => lb, 
				GamePadButtonEnum.RB_R1_R => rb, 
				GamePadButtonEnum.LT_L2_ZL => lt, 
				GamePadButtonEnum.RT_R2_ZR => rt, 
				GamePadButtonEnum.Menu_Options_Plus => start, 
				GamePadButtonEnum.ChangeView_TouchPad_Minus => back, 
				GamePadButtonEnum.Up => stickY > 0f, 
				GamePadButtonEnum.Down => stickY < 0f, 
				GamePadButtonEnum.Right => stickX > 0f, 
				GamePadButtonEnum.Left => stickX < 0f, 
				_ => false, 
			};
		}

		public void reset_buttons(bool value = false)
		{
			a = value;
			b = value;
			x = value;
			y = value;
			lb = value;
			rb = value;
			lt = value;
			rt = value;
			start = value;
			back = value;
		}

		public bool is_any_button_pressed()
		{
			for (int i = 0; i < 15; i++)
			{
				if (is_pressed((GamePadButtonEnum)i))
				{
					return true;
				}
			}
			return false;
		}

		public bool is_any_pressed()
		{
			if (is_any_button_pressed())
			{
				return true;
			}
			if (stickX == 0f)
			{
				return stickY != 0f;
			}
			return true;
		}
	}
}
