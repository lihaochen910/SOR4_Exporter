using System;

namespace CommonLib
{
	public struct MachineId_s : IEquatable<MachineId_s>
	{
		[Tag(1, null, true)]
		public ulong machineId;

		public MachineId_s(ulong machineId)
		{
			this.machineId = machineId;
		}

		public bool is_not_null()
		{
			return machineId != 0;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is MachineId_s))
			{
				return false;
			}
			MachineId_s machineId_s = (MachineId_s)obj;
			return machineId == machineId_s.machineId;
		}

		public bool Equals(MachineId_s other)
		{
			return machineId == other.machineId;
		}

		public override int GetHashCode()
		{
			return machineId.GetHashCode();
		}

		public static bool operator ==(MachineId_s value1, MachineId_s value2)
		{
			return value1.machineId == value2.machineId;
		}

		public static bool operator !=(MachineId_s value1, MachineId_s value2)
		{
			return value1.machineId != value2.machineId;
		}

		public override string ToString()
		{
			return "steamGamerId: " + machineId;
		}
	}
}
