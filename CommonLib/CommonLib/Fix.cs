using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace CommonLib
{
	public struct Fix : IYamlCustomSerialization
	{
		public struct FixConst
		{
			private long _raw;

			public long Raw => _raw;

			public static explicit operator double(FixConst f)
			{
				return (double)(f._raw >> 32) + (double)(uint)f._raw / 4294967296.0;
			}

			public static implicit operator FixConst(double value)
			{
				if (value < -2147483648.0 || value >= 2147483648.0)
				{
					throw new OverflowException();
				}
				double num = Math.Floor(value);
				return new FixConst(((long)num << 32) + (long)((value - num) * 4294967296.0 + 0.5));
			}

			public static implicit operator Fix(FixConst value)
			{
				return new Fix((int)(value.Raw + 32768 >> 16));
			}

			public static explicit operator int(FixConst value)
			{
				if (value._raw > 0)
				{
					return (int)(value._raw >> 32);
				}
				return (int)(value._raw + uint.MaxValue >> 32);
			}

			public static implicit operator FixConst(int value)
			{
				return new FixConst(value);
			}

			public static bool operator ==(FixConst lhs, FixConst rhs)
			{
				return lhs._raw == rhs._raw;
			}

			public static bool operator !=(FixConst lhs, FixConst rhs)
			{
				return lhs._raw != rhs._raw;
			}

			public static bool operator >(FixConst lhs, FixConst rhs)
			{
				return lhs._raw > rhs._raw;
			}

			public static bool operator >=(FixConst lhs, FixConst rhs)
			{
				return lhs._raw >= rhs._raw;
			}

			public static bool operator <(FixConst lhs, FixConst rhs)
			{
				return lhs._raw < rhs._raw;
			}

			public static bool operator <=(FixConst lhs, FixConst rhs)
			{
				return lhs._raw <= rhs._raw;
			}

			public static FixConst operator +(FixConst value)
			{
				return value;
			}

			public static FixConst operator -(FixConst value)
			{
				return new FixConst(-value._raw);
			}

			public FixConst(long raw)
			{
				_raw = raw;
			}

			public override bool Equals(object obj)
			{
				if (obj is FixConst)
				{
					return (FixConst)obj == this;
				}
				return false;
			}

			public override int GetHashCode()
			{
				return Raw.GetHashCode();
			}

			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (_raw < 0)
				{
					stringBuilder.Append(CultureInfo.CurrentCulture.NumberFormat.NegativeSign);
				}
				long num = (int)this;
				stringBuilder.Append(((num < 0) ? (-num) : num).ToString());
				ulong num2 = (ulong)(_raw & 0xFFFFFFFFu);
				if (num2 == 0L)
				{
					return stringBuilder.ToString();
				}
				num2 = ((_raw < 0) ? (4294967296L - num2) : num2);
				num2 *= 1000000000;
				num2 += 2147483648u;
				num2 >>= 32;
				stringBuilder.Append(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
				stringBuilder.Append(num2.ToString("D9").TrimEnd('0'));
				return stringBuilder.ToString();
			}
		}

		internal const int FRACTIONAL_BITS = 16;

		internal const int INTEGER_BITS = 16;

		internal const int FRACTION_MASK = 65535;

		internal const int INTEGER_MASK = -65536;

		internal const int FRACTION_RANGE = 65536;

		internal const int MIN_INTEGER = -32768;

		internal const int MAX_INTEGER = 32767;

		public static readonly Fix Zero;

		public static readonly Fix One;

		public static readonly Fix MinValue;

		public static readonly Fix MaxValue;

		public static readonly Fix Epsilon;

		public static readonly Fix cos45;

		public static readonly Fix sin45;

		[Tag(1, null, true)]
		public int raw;

		[ThreadStatic]
		private static StringBuilder stringBuilder;

		private static FixConst _piConst;

		private static FixConst _eConst;

		private static FixConst _log2_EConst;

		private static FixConst _log2_10Const;

		public static FixConst _ln2Const;

		private static FixConst _log10_2Const;

		private const int _quarterSineResPower = 2;

		private static FixConst[] _quarterSineConsts;

		private static FixConst[] _cordicAngleConsts;

		private static FixConst[] _cordicGainConsts;

		public static FixConst[] _invFactConsts;

		public static readonly Fix pi;

		public static readonly Fix e;

		private static Fix _log2_E;

		private static Fix _log2_10;

		private static Fix _ln2;

		private static Fix _log10_2;

		private static Fix[] _quarterSine;

		private static Fix[] _cordicAngles;

		private static Fix[] _cordicGains;

		public int Raw => raw;

		public int IntegerPart
		{
			get
			{
				return (int)this;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		static Fix()
		{
			Zero = new Fix(0);
			One = new Fix(65536);
			MinValue = new Fix(int.MinValue);
			MaxValue = new Fix(int.MaxValue);
			Epsilon = new Fix(1);
			cos45 = new Fix(70710678, 100000000);
			sin45 = new Fix(70710678, 100000000);
			_piConst = new FixConst(13493037705L);
			_eConst = new FixConst(11674931555L);
			_log2_EConst = new FixConst(6196328019L);
			_log2_10Const = new FixConst(14267572527L);
			_ln2Const = new FixConst(2977044472L);
			_log10_2Const = new FixConst(1292913986L);
			_quarterSineConsts = new FixConst[361]
			{
				new FixConst(0L),
				new FixConst(18740271L),
				new FixConst(37480185L),
				new FixConst(56219385L),
				new FixConst(74957515L),
				new FixConst(93694218L),
				new FixConst(112429137L),
				new FixConst(131161916L),
				new FixConst(149892197L),
				new FixConst(168619625L),
				new FixConst(187343842L),
				new FixConst(206064493L),
				new FixConst(224781220L),
				new FixConst(243493669L),
				new FixConst(262201481L),
				new FixConst(280904301L),
				new FixConst(299601773L),
				new FixConst(318293542L),
				new FixConst(336979250L),
				new FixConst(355658543L),
				new FixConst(374331065L),
				new FixConst(392996460L),
				new FixConst(411654373L),
				new FixConst(430304448L),
				new FixConst(448946331L),
				new FixConst(467579667L),
				new FixConst(486204101L),
				new FixConst(504819278L),
				new FixConst(523424844L),
				new FixConst(542020445L),
				new FixConst(560605727L),
				new FixConst(579180335L),
				new FixConst(597743917L),
				new FixConst(616296119L),
				new FixConst(634836587L),
				new FixConst(653364969L),
				new FixConst(671880911L),
				new FixConst(690384062L),
				new FixConst(708874069L),
				new FixConst(727350581L),
				new FixConst(745813244L),
				new FixConst(764261708L),
				new FixConst(782695622L),
				new FixConst(801114635L),
				new FixConst(819518395L),
				new FixConst(837906553L),
				new FixConst(856278758L),
				new FixConst(874634661L),
				new FixConst(892973913L),
				new FixConst(911296163L),
				new FixConst(929601063L),
				new FixConst(947888266L),
				new FixConst(966157422L),
				new FixConst(984408183L),
				new FixConst(1002640203L),
				new FixConst(1020853134L),
				new FixConst(1039046630L),
				new FixConst(1057220343L),
				new FixConst(1075373929L),
				new FixConst(1093507041L),
				new FixConst(1111619334L),
				new FixConst(1129710464L),
				new FixConst(1147780085L),
				new FixConst(1165827855L),
				new FixConst(1183853429L),
				new FixConst(1201856464L),
				new FixConst(1219836617L),
				new FixConst(1237793546L),
				new FixConst(1255726910L),
				new FixConst(1273636366L),
				new FixConst(1291521575L),
				new FixConst(1309382194L),
				new FixConst(1327217885L),
				new FixConst(1345028307L),
				new FixConst(1362813122L),
				new FixConst(1380571991L),
				new FixConst(1398304576L),
				new FixConst(1416010539L),
				new FixConst(1433689544L),
				new FixConst(1451341253L),
				new FixConst(1468965330L),
				new FixConst(1486561441L),
				new FixConst(1504129249L),
				new FixConst(1521668421L),
				new FixConst(1539178623L),
				new FixConst(1556659521L),
				new FixConst(1574110783L),
				new FixConst(1591532075L),
				new FixConst(1608923068L),
				new FixConst(1626283428L),
				new FixConst(1643612827L),
				new FixConst(1660910933L),
				new FixConst(1678177418L),
				new FixConst(1695411953L),
				new FixConst(1712614210L),
				new FixConst(1729783862L),
				new FixConst(1746920580L),
				new FixConst(1764024040L),
				new FixConst(1781093915L),
				new FixConst(1798129881L),
				new FixConst(1815131613L),
				new FixConst(1832098787L),
				new FixConst(1849031081L),
				new FixConst(1865928172L),
				new FixConst(1882789739L),
				new FixConst(1899615460L),
				new FixConst(1916405015L),
				new FixConst(1933158084L),
				new FixConst(1949874349L),
				new FixConst(1966553491L),
				new FixConst(1983195193L),
				new FixConst(1999799137L),
				new FixConst(2016365009L),
				new FixConst(2032892491L),
				new FixConst(2049381270L),
				new FixConst(2065831032L),
				new FixConst(2082241464L),
				new FixConst(2098612252L),
				new FixConst(2114943086L),
				new FixConst(2131233655L),
				new FixConst(2147483648L),
				new FixConst(2163692756L),
				new FixConst(2179860670L),
				new FixConst(2195987083L),
				new FixConst(2212071688L),
				new FixConst(2228114178L),
				new FixConst(2244114248L),
				new FixConst(2260071593L),
				new FixConst(2275985909L),
				new FixConst(2291856895L),
				new FixConst(2307684246L),
				new FixConst(2323467662L),
				new FixConst(2339206844L),
				new FixConst(2354901489L),
				new FixConst(2370551301L),
				new FixConst(2386155981L),
				new FixConst(2401715233L),
				new FixConst(2417228758L),
				new FixConst(2432696264L),
				new FixConst(2448117454L),
				new FixConst(2463492036L),
				new FixConst(2478819716L),
				new FixConst(2494100203L),
				new FixConst(2509333207L),
				new FixConst(2524518436L),
				new FixConst(2539655602L),
				new FixConst(2554744416L),
				new FixConst(2569784592L),
				new FixConst(2584775843L),
				new FixConst(2599717883L),
				new FixConst(2614610429L),
				new FixConst(2629453196L),
				new FixConst(2644245902L),
				new FixConst(2658988265L),
				new FixConst(2673680006L),
				new FixConst(2688320843L),
				new FixConst(2702910498L),
				new FixConst(2717448694L),
				new FixConst(2731935154L),
				new FixConst(2746369601L),
				new FixConst(2760751762L),
				new FixConst(2775081362L),
				new FixConst(2789358128L),
				new FixConst(2803581789L),
				new FixConst(2817752074L),
				new FixConst(2831868713L),
				new FixConst(2845931437L),
				new FixConst(2859939978L),
				new FixConst(2873894071L),
				new FixConst(2887793449L),
				new FixConst(2901637847L),
				new FixConst(2915427003L),
				new FixConst(2929160652L),
				new FixConst(2942838535L),
				new FixConst(2956460391L),
				new FixConst(2970025959L),
				new FixConst(2983534983L),
				new FixConst(2996987204L),
				new FixConst(3010382368L),
				new FixConst(3023720217L),
				new FixConst(3037000500L),
				new FixConst(3050222962L),
				new FixConst(3063387353L),
				new FixConst(3076493421L),
				new FixConst(3089540917L),
				new FixConst(3102529593L),
				new FixConst(3115459201L),
				new FixConst(3128329495L),
				new FixConst(3141140230L),
				new FixConst(3153891163L),
				new FixConst(3166582050L),
				new FixConst(3179212649L),
				new FixConst(3191782722L),
				new FixConst(3204292027L),
				new FixConst(3216740327L),
				new FixConst(3229127385L),
				new FixConst(3241452965L),
				new FixConst(3253716833L),
				new FixConst(3265918754L),
				new FixConst(3278058497L),
				new FixConst(3290135830L),
				new FixConst(3302150525L),
				new FixConst(3314102350L),
				new FixConst(3325991081L),
				new FixConst(3337816489L),
				new FixConst(3349578350L),
				new FixConst(3361276439L),
				new FixConst(3372910535L),
				new FixConst(3384480416L),
				new FixConst(3395985861L),
				new FixConst(3407426651L),
				new FixConst(3418802568L),
				new FixConst(3430113397L),
				new FixConst(3441358921L),
				new FixConst(3452538927L),
				new FixConst(3463653201L),
				new FixConst(3474701533L),
				new FixConst(3485683711L),
				new FixConst(3496599527L),
				new FixConst(3507448772L),
				new FixConst(3518231241L),
				new FixConst(3528946727L),
				new FixConst(3539595028L),
				new FixConst(3550175940L),
				new FixConst(3560689261L),
				new FixConst(3571134792L),
				new FixConst(3581512334L),
				new FixConst(3591821689L),
				new FixConst(3602062661L),
				new FixConst(3612235055L),
				new FixConst(3622338677L),
				new FixConst(3632373336L),
				new FixConst(3642338838L),
				new FixConst(3652234996L),
				new FixConst(3662061621L),
				new FixConst(3671818526L),
				new FixConst(3681505524L),
				new FixConst(3691122431L),
				new FixConst(3700669065L),
				new FixConst(3710145244L),
				new FixConst(3719550787L),
				new FixConst(3728885515L),
				new FixConst(3738149250L),
				new FixConst(3747341816L),
				new FixConst(3756463039L),
				new FixConst(3765512743L),
				new FixConst(3774490758L),
				new FixConst(3783396912L),
				new FixConst(3792231035L),
				new FixConst(3800992960L),
				new FixConst(3809682520L),
				new FixConst(3818299548L),
				new FixConst(3826843882L),
				new FixConst(3835315358L),
				new FixConst(3843713815L),
				new FixConst(3852039094L),
				new FixConst(3860291035L),
				new FixConst(3868469481L),
				new FixConst(3876574278L),
				new FixConst(3884605270L),
				new FixConst(3892562305L),
				new FixConst(3900445232L),
				new FixConst(3908253899L),
				new FixConst(3915988159L),
				new FixConst(3923647864L),
				new FixConst(3931232868L),
				new FixConst(3938743028L),
				new FixConst(3946178199L),
				new FixConst(3953538241L),
				new FixConst(3960823014L),
				new FixConst(3968032378L),
				new FixConst(3975166196L),
				new FixConst(3982224333L),
				new FixConst(3989206654L),
				new FixConst(3996113026L),
				new FixConst(4002943318L),
				new FixConst(4009697400L),
				new FixConst(4016375143L),
				new FixConst(4022976420L),
				new FixConst(4029501105L),
				new FixConst(4035949075L),
				new FixConst(4042320205L),
				new FixConst(4048614376L),
				new FixConst(4054831467L),
				new FixConst(4060971360L),
				new FixConst(4067033938L),
				new FixConst(4073019085L),
				new FixConst(4078926688L),
				new FixConst(4084756634L),
				new FixConst(4090508812L),
				new FixConst(4096183113L),
				new FixConst(4101779428L),
				new FixConst(4107297652L),
				new FixConst(4112737678L),
				new FixConst(4118099404L),
				new FixConst(4123382727L),
				new FixConst(4128587547L),
				new FixConst(4133713764L),
				new FixConst(4138761282L),
				new FixConst(4143730003L),
				new FixConst(4148619834L),
				new FixConst(4153430681L),
				new FixConst(4158162453L),
				new FixConst(4162815059L),
				new FixConst(4167388412L),
				new FixConst(4171882423L),
				new FixConst(4176297008L),
				new FixConst(4180632082L),
				new FixConst(4184887562L),
				new FixConst(4189063369L),
				new FixConst(4193159422L),
				new FixConst(4197175643L),
				new FixConst(4201111956L),
				new FixConst(4204968286L),
				new FixConst(4208744559L),
				new FixConst(4212440704L),
				new FixConst(4216056650L),
				new FixConst(4219592328L),
				new FixConst(4223047672L),
				new FixConst(4226422614L),
				new FixConst(4229717092L),
				new FixConst(4232931042L),
				new FixConst(4236064403L),
				new FixConst(4239117116L),
				new FixConst(4242089121L),
				new FixConst(4244980364L),
				new FixConst(4247790788L),
				new FixConst(4250520341L),
				new FixConst(4253168970L),
				new FixConst(4255736624L),
				new FixConst(4258223255L),
				new FixConst(4260628816L),
				new FixConst(4262953261L),
				new FixConst(4265196545L),
				new FixConst(4267358626L),
				new FixConst(4269439463L),
				new FixConst(4271439016L),
				new FixConst(4273357246L),
				new FixConst(4275194119L),
				new FixConst(4276949597L),
				new FixConst(4278623649L),
				new FixConst(4280216242L),
				new FixConst(4281727345L),
				new FixConst(4283156931L),
				new FixConst(4284504972L),
				new FixConst(4285771441L),
				new FixConst(4286956316L),
				new FixConst(4288059574L),
				new FixConst(4289081193L),
				new FixConst(4290021154L),
				new FixConst(4290879439L),
				new FixConst(4291656032L),
				new FixConst(4292350918L),
				new FixConst(4292964084L),
				new FixConst(4293495518L),
				new FixConst(4293945210L),
				new FixConst(4294313152L),
				new FixConst(4294599336L),
				new FixConst(4294803757L),
				new FixConst(4294926411L),
				new FixConst(4294967296L)
			};
			_cordicAngleConsts = new FixConst[24]
			{
				new FixConst(193273528320L),
				new FixConst(114096026022L),
				new FixConst(60285206653L),
				new FixConst(30601712202L),
				new FixConst(15360239180L),
				new FixConst(7687607525L),
				new FixConst(3844741810L),
				new FixConst(1922488225L),
				new FixConst(961258780L),
				new FixConst(480631223L),
				new FixConst(240315841L),
				new FixConst(120157949L),
				new FixConst(60078978L),
				new FixConst(30039490L),
				new FixConst(15019745L),
				new FixConst(7509872L),
				new FixConst(3754936L),
				new FixConst(1877468L),
				new FixConst(938734L),
				new FixConst(469367L),
				new FixConst(234684L),
				new FixConst(117342L),
				new FixConst(58671L),
				new FixConst(29335L)
			};
			_cordicGainConsts = new FixConst[24]
			{
				new FixConst(3037000500L),
				new FixConst(2716375826L),
				new FixConst(2635271635L),
				new FixConst(2614921743L),
				new FixConst(2609829388L),
				new FixConst(2608555990L),
				new FixConst(2608237621L),
				new FixConst(2608158028L),
				new FixConst(2608138129L),
				new FixConst(2608133154L),
				new FixConst(2608131911L),
				new FixConst(2608131600L),
				new FixConst(2608131522L),
				new FixConst(2608131503L),
				new FixConst(2608131498L),
				new FixConst(2608131497L),
				new FixConst(2608131496L),
				new FixConst(2608131496L),
				new FixConst(2608131496L),
				new FixConst(2608131496L),
				new FixConst(2608131496L),
				new FixConst(2608131496L),
				new FixConst(2608131496L),
				new FixConst(2608131496L)
			};
			_invFactConsts = new FixConst[14]
			{
				new FixConst(4294967296L),
				new FixConst(4294967296L),
				new FixConst(2147483648L),
				new FixConst(715827883L),
				new FixConst(178956971L),
				new FixConst(35791394L),
				new FixConst(5965232L),
				new FixConst(852176L),
				new FixConst(106522L),
				new FixConst(11836L),
				new FixConst(1184L),
				new FixConst(108L),
				new FixConst(9L),
				new FixConst(1L)
			};
			if (_quarterSineConsts.Length != 361)
			{
				throw new Exception("_quarterSineConst.Length must be 90 * 2^(_quarterSineResPower) + 1.");
			}
			pi = _piConst;
			e = _eConst;
			_log2_E = _log2_EConst;
			_log2_10 = _log2_10Const;
			_ln2 = _ln2Const;
			_log10_2 = _log10_2Const;
			_quarterSine = Array.ConvertAll(_quarterSineConsts, (Converter<FixConst, Fix>)((FixConst c) => c));
			_cordicAngles = Array.ConvertAll(_cordicAngleConsts, (Converter<FixConst, Fix>)((FixConst c) => c));
			_cordicGains = Array.ConvertAll(_cordicGainConsts, (Converter<FixConst, Fix>)((FixConst c) => c));
		}

		public static explicit operator double(Fix value)
		{
			return (double)(value.raw >> 16) + (double)(value.raw & 0xFFFF) / 65536.0;
		}

		public static explicit operator float(Fix value)
		{
			return (float)(double)value;
		}

		public static explicit operator Fix(float value)
		{
			value *= 65536f;
			return new Fix((int)value);
		}

		[Obsolete]
		public float obsolete_float()
		{
			return (float)this;
		}

		public static explicit operator int(Fix value)
		{
			if (value.raw > 0)
			{
				return value.raw >> 16;
			}
			return value.raw + 65535 >> 16;
		}

		public static implicit operator Fix(int value)
		{
			return new Fix(value << 16);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Fix lhs, Fix rhs)
		{
			return lhs.raw == rhs.raw;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Fix lhs, Fix rhs)
		{
			return lhs.raw != rhs.raw;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >(Fix lhs, Fix rhs)
		{
			return lhs.raw > rhs.raw;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >=(Fix lhs, Fix rhs)
		{
			return lhs.raw >= rhs.raw;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <(Fix lhs, Fix rhs)
		{
			return lhs.raw < rhs.raw;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <=(Fix lhs, Fix rhs)
		{
			return lhs.raw <= rhs.raw;
		}

		public static Fix operator +(Fix value)
		{
			return value;
		}

		public static Fix operator -(Fix value)
		{
			return new Fix(-value.raw);
		}

		public static Fix operator +(Fix lhs, Fix rhs)
		{
			return new Fix(lhs.raw + rhs.raw);
		}

		public static Fix operator -(Fix lhs, Fix rhs)
		{
			return new Fix(lhs.raw - rhs.raw);
		}

		public static Fix operator *(Fix lhs, Fix rhs)
		{
			return new Fix((int)((long)lhs.raw * (long)rhs.raw + 32768 >> 16));
		}

		public static Fix operator /(Fix lhs, Fix rhs)
		{
			return new Fix((int)(((long)lhs.raw << 17) / rhs.raw + 1 >> 1));
		}

		public static Fix operator %(Fix lhs, Fix rhs)
		{
			return new Fix(lhs.Raw % rhs.Raw);
		}

		public static Fix operator <<(Fix lhs, int rhs)
		{
			return new Fix(lhs.Raw << rhs);
		}

		public static Fix operator >>(Fix lhs, int rhs)
		{
			return new Fix(lhs.Raw >> rhs);
		}

		public static Fix operator ++(Fix a)
		{
			a += (Fix)1;
			return a;
		}

		public static Fix operator --(Fix a)
		{
			a -= (Fix)1;
			return a;
		}

		public void add_with_overflow_check(Fix rhs)
		{
			long num = (long)raw + (long)rhs.raw;
			if (num >= MaxValue.raw)
			{
				this = MaxValue;
			}
			else
			{
				this = new Fix(raw + rhs.raw);
			}
		}

		public Fix(int raw)
		{
			this.raw = raw;
		}

		public Fix(int integer, long numerator, long denominator)
		{
			if (denominator < 0)
			{
				throw new ArgumentException("Denominator must be positive.");
			}
			int num = 1;
			if (numerator < 0)
			{
				if (integer > 0)
				{
					throw new ArgumentException("If numerator is negative, integer should be negative.");
				}
				num = -1;
			}
			else if (integer < 0)
			{
				num = -1;
			}
			integer = integer.abs();
			numerator = numerator.abs();
			ulong num2 = (ulong)integer * 65536uL;
			ulong num3 = 10000000000000000uL / (ulong)denominator;
			ulong num4 = (ulong)numerator * num3;
			ulong num5 = num4;
			num5 /= 152587890625uL;
			num5 += num2;
			raw = (int)num5 * num;
		}

		public Fix(int numerator, int denominator)
		{
			raw = (int)(((long)numerator << 17) / denominator + 1 >> 1);
		}

		public override bool Equals(object obj)
		{
			if (obj is Fix)
			{
				return (Fix)obj == this;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Raw.GetHashCode();
		}

		public override string ToString()
		{
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder();
			}
			else
			{
				stringBuilder.Clear();
			}
			if (raw < 0)
			{
				stringBuilder.Append("-");
			}
			int num = (int)this;
			stringBuilder.Append(((num < 0) ? (-num) : num).ToString());
			ulong num2 = (ulong)(int)((uint)raw & 0xFFFFu);
			if (num2 == 0L)
			{
				return stringBuilder.ToString();
			}
			num2 = ((raw < 0) ? (65536 - num2) : num2);
			num2 *= 152587890625L;
			stringBuilder.Append(".");
			stringBuilder.Append(num2.ToString("D16").TrimEnd('0'));
			return stringBuilder.ToString();
		}

		public static Fix parse(string s)
		{
			if (s.is_null_or_empty())
			{
				return 0;
			}
			s = s.Trim();
			string[] array = s.Split('.');
			if (array.Length > 2)
			{
				throw new ArgumentException("Invalid number of . in Fix string: " + s);
			}
			if (array.Length == 1)
			{
				return int.Parse(array[0]);
			}
			long num = long.Parse(array[0]);
			long num2 = long.Parse(array[1]);
			if (s[0] == '-')
			{
				num2 = -num2;
			}
			return new Fix((int)num, num2, math.pow(10L, array[1].Length));
		}

		public string yaml_serialize()
		{
			return ToString();
		}

		public void yaml_deserialize(string src)
		{
			this = parse(src);
		}

		public static Fix Ceiling(Fix value)
		{
			if (value == MaxValue)
			{
				return value;
			}
			return new Fix((value.Raw + 65535) & -65536);
		}

		public static Fix Floor(Fix value)
		{
			if (value == MinValue)
			{
				return value;
			}
			return new Fix(value.Raw & -65536);
		}

		public static Fix Truncate(Fix value)
		{
			if (value < 0)
			{
				return new Fix((value.Raw + 65536) & -65536);
			}
			return new Fix(value.Raw & -65536);
		}

		public static Fix Round(Fix value)
		{
			return new Fix((value.Raw + 32768) & -65536);
		}

		public static Fix Min(Fix v1, Fix v2)
		{
			if (!(v1 < v2))
			{
				return v2;
			}
			return v1;
		}

		public static Fix Sqrt(Fix value)
		{
			if (value.Raw < 0)
			{
				throw new ArgumentOutOfRangeException("value", "Value must be non-negative.");
			}
			if (value.Raw == 0)
			{
				return 0;
			}
			return new Fix((int)(SqrtULong((ulong)((long)value.Raw << 18)) + 1) >> 1);
		}

		internal static uint SqrtULong(ulong N)
		{
			ulong num = 33554432uL;
			while (true)
			{
				ulong num2 = num + N / num >> 1;
				if (num2 >= num)
				{
					break;
				}
				num = num2;
			}
			return (uint)num;
		}

		public static Fix sin_degrees(Fix degrees)
		{
			return CosRaw(degrees.Raw - 5898240);
		}

		public static Fix cos_degrees(Fix degrees)
		{
			return CosRaw(degrees.Raw);
		}

		private static Fix CosRaw(int raw)
		{
			raw = ((raw < 0) ? (-raw) : raw);
			int num = raw & 0x3FFF;
			raw >>= 14;
			if (num == 0)
			{
				return CosRawLookup(raw);
			}
			Fix fix = CosRawLookup(raw);
			Fix fix2 = CosRawLookup(raw + 1);
			return new Fix((int)((long)fix.Raw * (long)(16384 - num) + (long)fix2.Raw * (long)num + 8192 >> 14));
		}

		private static Fix CosRawLookup(int raw)
		{
			raw %= 1440;
			if (raw < 360)
			{
				return _quarterSine[360 - raw];
			}
			if (raw < 720)
			{
				raw -= 360;
				return -_quarterSine[raw];
			}
			if (raw < 1080)
			{
				raw -= 720;
				return -_quarterSine[360 - raw];
			}
			raw -= 1080;
			return _quarterSine[raw];
		}

		public static Fix tan_degrees(Fix degrees)
		{
			return sin_degrees(degrees) / cos_degrees(degrees);
		}

		public static Fix Asin(Fix value)
		{
			return Atan2(value, Sqrt((1 + value) * (1 - value)));
		}

		public static Fix Acos(Fix value)
		{
			return Atan2(Sqrt((1 + value) * (1 - value)), value);
		}

		public static Fix Atan(Fix value)
		{
			return Atan2(value, 1);
		}

		public static Fix Atan2(Fix y, Fix x)
		{
			if (x == 0 && y == 0)
			{
				return 0;
			}
			Fix result = 0;
			if (x < 0)
			{
				Fix fix;
				Fix fix2;
				if (y < 0)
				{
					fix = -y;
					fix2 = x;
					result = -90;
				}
				else if (y > 0)
				{
					fix = y;
					fix2 = -x;
					result = 90;
				}
				else
				{
					fix = x;
					fix2 = y;
					result = 180;
				}
				x = fix;
				y = fix2;
			}
			for (int i = 0; i < 18; i++)
			{
				Fix fix;
				Fix fix2;
				if (y > 0)
				{
					fix = x + (y >> i);
					fix2 = y - (x >> i);
					result += _cordicAngles[i];
				}
				else
				{
					if (!(y < 0))
					{
						break;
					}
					fix = x - (y >> i);
					fix2 = y + (x >> i);
					result -= _cordicAngles[i];
				}
				x = fix;
				y = fix2;
			}
			return result;
		}

		public static Fix Log(Fix value)
		{
			return Log2(value) * _ln2;
		}

		public static Fix Log(Fix value, Fix b)
		{
			if (b == 2)
			{
				return Log2(value);
			}
			if (b == e)
			{
				return Log(value);
			}
			if (b == 10)
			{
				return Log10(value);
			}
			return Log2(value) / Log2(b);
		}

		public static Fix Log10(Fix value)
		{
			return Log2(value) * _log10_2;
		}

		private static Fix Log2(Fix value)
		{
			if (value <= 0)
			{
				throw new ArgumentOutOfRangeException("value", "Value must be positive.");
			}
			uint num = (uint)value.Raw;
			uint num2 = 32768u;
			uint num3 = 0u;
			while (num < 65536)
			{
				num <<= 1;
				num3 -= 65536;
			}
			while (num >= 131072)
			{
				num >>= 1;
				num3 += 65536;
			}
			ulong num4 = num;
			for (int i = 0; i < 16; i++)
			{
				num4 = num4 * num4 >> 16;
				if (num4 >= 131072)
				{
					num4 >>= 1;
					num3 += num2;
				}
				num2 >>= 1;
			}
			return new Fix((int)num3);
		}
	}
}
