using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace nanoCAD.DynamoApp.Services
{
	// Token: 0x0200000E RID: 14
	public class LifecycleManager<T>
	{
		// Token: 0x0600006A RID: 106 RVA: 0x0000343C File Offset: 0x0000243C
		private LifecycleManager()
		{
			this.wrappers = new Dictionary<T, List<KeyValuePair<object, bool>>>();
			this.autocadDeleted = new Dictionary<T, bool>();
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x0600006B RID: 107 RVA: 0x0000345C File Offset: 0x0000245C
		public static LifecycleManager<T> Instance
		{
			get
			{
				if (LifecycleManager<T>.manager == null)
				{
					object obj = LifecycleManager<T>.singletonMutex;
					lock (obj)
					{
						if (LifecycleManager<T>.manager == null)
						{
							LifecycleManager<T>.manager = new LifecycleManager<T>();
						}
					}
				}
				return LifecycleManager<T>.manager;
			}
		}

		// Token: 0x0600006C RID: 108 RVA: 0x000034B4 File Offset: 0x000024B4
		public void ClearAll()
		{
			this.wrappers.Clear();
			this.autocadDeleted.Clear();
		}

		// Token: 0x0600006D RID: 109 RVA: 0x000034CC File Offset: 0x000024CC
		public void UpdateAllObjectsForEvaluation()
		{
			try
			{
				foreach (List<KeyValuePair<object, bool>> list in this.wrappers.Values)
				{
					foreach (KeyValuePair<object, bool> keyValuePair in list)
					{
						object key = keyValuePair.Key;
						MethodInfo method = key.GetType().GetMethod("UpdateForEvaluation");
						if (method != null)
						{
							method.Invoke(key, new object[0]);
						}
					}
				}
			}
			catch
			{
			}
		}

		// Token: 0x0600006E RID: 110 RVA: 0x00003594 File Offset: 0x00002594
		public void RegisterAsssociation(T elementHandle, object wrapper, bool isDynamoOwned)
		{
			KeyValuePair<object, bool> item = new KeyValuePair<object, bool>(wrapper, isDynamoOwned);
			List<KeyValuePair<object, bool>> list;
			if (this.wrappers.TryGetValue(elementHandle, out list))
			{
				if (list.Contains(item))
				{
					Trace.WriteLine("Lifecycle manager alert: registering the same Element Wrapper twice");
				}
			}
			else
			{
				list = new List<KeyValuePair<object, bool>>();
				this.wrappers.Add(elementHandle, list);
			}
			list.Add(item);
			if (!this.autocadDeleted.ContainsKey(elementHandle))
			{
				this.autocadDeleted.Add(elementHandle, false);
			}
		}

		// Token: 0x0600006F RID: 111 RVA: 0x00003604 File Offset: 0x00002604
		public void UnRegisterAssociation(T elementHandle, object wrapper, bool isDynamoOwned)
		{
			List<KeyValuePair<object, bool>> list;
			if (this.wrappers.TryGetValue(elementHandle, out list))
			{
				KeyValuePair<object, bool> item = new KeyValuePair<object, bool>(wrapper, isDynamoOwned);
				if (!list.Contains(item))
				{
					Trace.WriteLine("Attempting to remove a wrapper that wasn't there registered");
					return;
				}
				list.Remove(item);
				if (list.Count == 0)
				{
					this.wrappers.Remove(elementHandle);
					this.autocadDeleted.Remove(elementHandle);
					return;
				}
			}
			else
			{
				Trace.WriteLine("Attempting to remove a wrapper, but there were no ids registered");
			}
		}

		// Token: 0x06000070 RID: 112 RVA: 0x00003673 File Offset: 0x00002673
		public int GetRegisteredCount(T handle)
		{
			if (!this.wrappers.ContainsKey(handle))
			{
				return 0;
			}
			return this.wrappers[handle].Count;
		}

		// Token: 0x06000071 RID: 113 RVA: 0x00003696 File Offset: 0x00002696
		public bool IsEmpty()
		{
			return this.wrappers.Count <= 0;
		}

		// Token: 0x06000072 RID: 114 RVA: 0x000036AC File Offset: 0x000026AC
		public int GetDynamoOwnedRegisterCount(T handle)
		{
			if (!this.wrappers.ContainsKey(handle))
			{
				return 0;
			}
			return this.wrappers[handle].Count((KeyValuePair<object, bool> item) => item.Value);
		}

		// Token: 0x06000073 RID: 115 RVA: 0x000036F9 File Offset: 0x000026F9
		public bool IsAutoCADDeleted(T handle)
		{
			if (!this.autocadDeleted.ContainsKey(handle))
			{
				Trace.WriteLine("Element is not registered");
				return false;
			}
			return this.autocadDeleted[handle];
		}

		// Token: 0x06000074 RID: 116 RVA: 0x00003721 File Offset: 0x00002721
		public void NotifyOfDeletion(T handle)
		{
			this.autocadDeleted[handle] = true;
		}

		// Token: 0x0400002E RID: 46
		private static object singletonMutex = new object();

		// Token: 0x0400002F RID: 47
		private static LifecycleManager<T> manager;

		// Token: 0x04000030 RID: 48
		private Dictionary<T, List<KeyValuePair<object, bool>>> wrappers;

		// Token: 0x04000031 RID: 49
		private Dictionary<T, bool> autocadDeleted;
	}
}
