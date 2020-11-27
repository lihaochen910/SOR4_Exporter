using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace CommonLib
{
	public static class profiler
	{
		public enum EntryTypeEnum
		{
			begin = 0,
			end = 1,
			action = 2
		}

		public struct Entry_s
		{
			[Tag(1, null, true)]
			public long time;

			[Tag(2, null, true)]
			public EntryTypeEnum type;

			[Tag(3, null, true)]
			public string info;
		}

		[ProtoRoot(false)]
		public class ProfilerSession
		{
			[Tag(1, null, true)]
			public ConcurrentDictionary<string, Dictionary<string, List<Entry_s>>> entryListByTaskNameByThreadName = new ConcurrentDictionary<string, Dictionary<string, List<Entry_s>>>();
		}

		public static ProfilerSession session = new ProfilerSession();

		public static volatile bool enabled = true;

		private static long mainThreadLastTime;

		private static Func<string, Dictionary<string, List<Entry_s>>> entryListByTaskNameFactoryDelegate = entry_list_by_task_name_factory;

		[ThreadStatic]
		private static Stopwatch stopwatch;

		[ThreadStatic]
		private static long stopwatchStartTime;

		private static long get_time()
		{
			if (stopwatch == null)
			{
				stopwatch = Stopwatch.StartNew();
				stopwatchStartTime = mainThreadLastTime;
			}
			if (utils.is_main_thread())
			{
				return mainThreadLastTime = stopwatch.ElapsedTicks;
			}
			return stopwatch.ElapsedTicks + stopwatchStartTime;
		}

		private static Dictionary<string, List<Entry_s>> entry_list_by_task_name_factory(string key)
		{
			return new Dictionary<string, List<Entry_s>>();
		}

		private static void add(string taskName, EntryTypeEnum type, string info)
		{
			if (enabled)
			{
				Entry_s item = default(Entry_s);
				item.time = get_time();
				item.type = type;
				item.info = info;
				Dictionary<string, List<Entry_s>> orAdd = session.entryListByTaskNameByThreadName.GetOrAdd(Thread.CurrentThread.Name, entryListByTaskNameFactoryDelegate);
				List<Entry_s> list = orAdd.get_or_create(taskName);
				list.Add(item);
			}
		}

		[Conditional("NOT_RETAIL")]
		public static void begin(string taskName, string info = null)
		{
			add(taskName, EntryTypeEnum.begin, info);
		}

		[Conditional("NOT_RETAIL")]
		public static void end(string taskName, string info = null)
		{
			add(taskName, EntryTypeEnum.end, info);
		}

		[Conditional("NOT_RETAIL")]
		public static void action(string taskName, string info = null)
		{
			add(taskName, EntryTypeEnum.action, info);
		}
	}
}
