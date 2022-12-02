using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

using Teigha.DatabaseServices;
using Dynamo.Engine;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Workspaces;
using DynamoServices;
using ProtoCore;

namespace nanoCAD.DynamoApp.Services
{
	// Token: 0x02000007 RID: 7
	public class ElementBinder
	{
		// Token: 0x0600002B RID: 43 RVA: 0x00002850 File Offset: 0x00001850
		public static void CollectInvalidHandlesBeforeFirstRun(Database db, HomeWorkspaceModel hws, EngineController engine)
		{
			ElementBinder.ClearInvalidHandlesBeforeFirstRun();
			try
			{
				PropertyInfo property = hws.GetType().GetProperty("PreloadedTraceData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (property != null)
				{
					IEnumerable<KeyValuePair<Guid, List<CallSite.RawTraceData>>> enumerable = property.GetValue(hws) as IEnumerable<KeyValuePair<Guid, List<CallSite.RawTraceData>>>;
					if (enumerable != null)
					{
						foreach (KeyValuePair<Guid, List<CallSite.RawTraceData>> keyValuePair in enumerable)
						{
							foreach (CallSite.RawTraceData rawTraceData in keyValuePair.Value)
							{
								IList<ISerializable> allSerializablesFromSingleRunTraceData = CallSite.GetAllSerializablesFromSingleRunTraceData(rawTraceData);
								foreach (ISerializable serializable in allSerializablesFromSingleRunTraceData)
								{
									SerializableHandle serializableHandle = serializable as SerializableHandle;
									if (serializableHandle != null)
									{
										string stringID = serializableHandle.stringID;
										ObjectId objectId = DocumentContext.GetObjectId(db, stringID);
										if (!objectId.IsValid || objectId.IsErased)
										{
											ElementBinder.InvalidHandlesBeforeFirstRun.Add(stringID);
										}
									}
								}
							}
						}
					}
				}
			}
			catch
			{
			}
		}

		// Token: 0x0600002C RID: 44 RVA: 0x000029D0 File Offset: 0x000019D0
		public static void ClearInvalidHandlesBeforeFirstRun()
		{
			ElementBinder.InvalidHandlesBeforeFirstRun.Clear();
		}

		// Token: 0x0600002D RID: 45 RVA: 0x000029DC File Offset: 0x000019DC
		public static string GetHandleFromTrace()
		{
			ISerializable traceData = TraceUtils.GetTraceData("{0459D869-0C72-447F-96D8-08A7FB92214B}-REVIT");
			SerializableHandle serializableHandle = traceData as SerializableHandle;
			if (serializableHandle == null)
			{
				return "";
			}
			return serializableHandle.stringID;
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00002A0C File Offset: 0x00001A0C
		public static ObjectId GetObjectIdFromTrace(Database db)
		{
			string handleFromTrace = ElementBinder.GetHandleFromTrace();
			if (ElementBinder.InvalidHandlesBeforeFirstRun.Contains(handleFromTrace))
			{
				return ObjectId.Null;
			}
			return DocumentContext.GetObjectId(db, handleFromTrace);
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00002A3C File Offset: 0x00001A3C
		public static void SetElementForTrace(string handle)
		{
			if (!ElementBinder.IsEnabled)
			{
				return;
			}
			TraceUtils.SetTraceData("{0459D869-0C72-447F-96D8-08A7FB92214B}-REVIT", new SerializableHandle
			{
				stringID = handle
			});
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00002A6C File Offset: 0x00001A6C
		public static void SetObjectIdForTrace(ObjectId id)
		{
			ElementBinder.SetElementForTrace(id.Handle.ToString());
		}

		// Token: 0x06000031 RID: 49 RVA: 0x00002A93 File Offset: 0x00001A93
		public static void CleanupAndSetElementForTrace(string handle)
		{
			if (!ElementBinder.IsEnabled)
			{
				return;
			}
			ElementBinder.SetElementForTrace(handle);
		}

		// Token: 0x06000032 RID: 50 RVA: 0x00002AA3 File Offset: 0x00001AA3
		internal static bool IsZeroTouchNode(NodeModel node)
		{
			return node is DSFunction || node is DSVarArgFunction || node is CodeBlockNodeModel;
		}

		// Token: 0x06000033 RID: 51 RVA: 0x00002AC0 File Offset: 0x00001AC0
		public static IEnumerable<NodeModel> GetNodesFromDBObject(string handle, WorkspaceModel workspace, EngineController engine)
		{
			List<NodeModel> list = new List<NodeModel>();
			if (LifecycleManager<string>.Instance.GetRegisteredCount(handle) <= 0)
			{
				return list;
			}
			RuntimeCore runtimeCore = null;
			if (engine != null && engine.LiveRunnerCore != null)
			{
				runtimeCore = engine.LiveRunnerRuntimeCore;
			}
			if (runtimeCore == null)
			{
				return list;
			}
			IEnumerable<Guid> enumerable = from n in workspace.Nodes
			where ElementBinder.IsZeroTouchNode(n)
			select n.GUID;
			Dictionary<Guid, List<CallSite>> callsitesForNodes = runtimeCore.RuntimeData.GetCallsitesForNodes(enumerable, runtimeCore.DSExecutable);
			foreach (KeyValuePair<Guid, List<CallSite>> keyValuePair in callsitesForNodes)
			{
				bool flag = false;
				Guid guid = keyValuePair.Key;
				List<CallSite> value = keyValuePair.Value;
				//Func<NodeModel, bool> <>9__2;
				foreach (CallSite callSite in value)
				{
					foreach (CallSite.SingleRunTraceData singleRunTraceData in callSite.TraceData)
					{
						List<ISerializable> list2 = singleRunTraceData.RecursiveGetNestedData();
						foreach (ISerializable serializable in list2)
						{
							SerializableHandle serializableHandle = serializable as SerializableHandle;
							if (serializableHandle != null && serializableHandle.stringID == handle)
							{
								flag = true;
								IEnumerable<NodeModel> nodes = workspace.Nodes;
								Func<NodeModel, bool> predicate;
								//if ((predicate = <>9__2) == null)
								//{
								//	predicate = (<>9__2 = ((NodeModel n) => n.GUID == guid));
								//}
								//NodeModel item = nodes.Where(predicate).FirstOrDefault<NodeModel>();
								//list.Add(item);
								break;
							}
						}
						if (flag)
						{
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
			}
			return list;
		}

		// Token: 0x0400001B RID: 27
		private const string REVIT_TRACE_ID = "{0459D869-0C72-447F-96D8-08A7FB92214B}-REVIT";

		// Token: 0x0400001C RID: 28
		private static ISet<string> InvalidHandlesBeforeFirstRun = new SortedSet<string>();

		// Token: 0x0400001D RID: 29
		public static bool IsEnabled = true;
	}
}
