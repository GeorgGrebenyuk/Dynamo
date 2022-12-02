using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using HostMgd.ApplicationServices;
using Teigha.DatabaseServices;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using ProtoCore;

namespace nanoCAD.DynamoApp.Services
{
	// Token: 0x0200000F RID: 15
	public static class TraceData
	{
		// Token: 0x06000076 RID: 118 RVA: 0x0000373C File Offset: 0x0000273C
		public static void SaveToDocument(HomeWorkspaceModel hws)
		{
			if (TraceData.SaveState != TraceData.TraceDataState.DynAndDwg)
			{
				return;
			}
			if (hws == null)
			{
				return;
			}
			IDictionary<Guid, List<CallSite.RawTraceData>> traceData = TraceData.GetTraceData(hws);
			if (traceData == null || !traceData.Any<KeyValuePair<Guid, List<CallSite.RawTraceData>>>())
			{
				return;
			}
			TraceData.WriteTraceData(traceData);
		}

		// Token: 0x06000077 RID: 119 RVA: 0x00003770 File Offset: 0x00002770
		public static void LoadFromDocument(HomeWorkspaceModel hws)
		{
			if (TraceData.SaveState != TraceData.TraceDataState.DynAndDwg)
			{
				return;
			}
			if (hws == null)
			{
				return;
			}
			IDictionary<Guid, List<CallSite.RawTraceData>> dictionary = TraceData.ReadTraceData(hws);
			if (dictionary == null || !dictionary.Any<KeyValuePair<Guid, List<CallSite.RawTraceData>>>())
			{
				return;
			}
			RuntimeCore runtimCore = TraceData.GetRuntimCore(hws);
			if (runtimCore == null)
			{
				return;
			}
			RuntimeData runtimeData = runtimCore.RuntimeData;
			IDictionary<Guid, List<CallSite.RawTraceData>> traceData = TraceData.GetTraceData(hws);
			if (traceData != null && traceData.Any<KeyValuePair<Guid, List<CallSite.RawTraceData>>>())
			{
				MethodInfo method = runtimeData.GetType().GetMethod("GetAndRemoveTraceDataForNode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (method != null)
				{
					foreach (KeyValuePair<Guid, List<CallSite.RawTraceData>> keyValuePair in traceData)
					{
						Guid key = keyValuePair.Key;
						foreach (CallSite.RawTraceData rawTraceData in keyValuePair.Value)
						{
							string id = rawTraceData.ID;
							method.Invoke(runtimeData, new object[]
							{
								key,
								id
							});
						}
					}
				}
			}
			try
			{
				runtimeData.SetTraceDataForNodes(dictionary);
			}
			catch
			{
			}
		}

		// Token: 0x06000078 RID: 120 RVA: 0x000038AC File Offset: 0x000028AC
		public static void RemoveFromDocument(HomeWorkspaceModel hws)
		{
			if (hws == null)
			{
				return;
			}
			IEnumerable<Guid> nodeGuids = TraceData.GetNodeGuids(hws);
			if (!nodeGuids.Any<Guid>())
			{
				return;
			}
			TraceData.RemoveTraceData(nodeGuids);
		}

		// Token: 0x06000079 RID: 121 RVA: 0x000038D3 File Offset: 0x000028D3
		public static void RemoveFromDocument(Document document)
		{
			TraceData.RemoveFromDocument(document, ObjectId.Null);
		}

		// Token: 0x0600007A RID: 122 RVA: 0x000038E0 File Offset: 0x000028E0
		public static void RemoveFromDocument(Document document, ObjectId objectId)
		{
			if (document == null)
			{
				return;
			}
			using (DocumentContext documentContext = new DocumentContext(document))
			{
				DBDictionary dbdictionary = documentContext.Transaction.GetObject(
					documentContext.Database.NamedObjectsDictionaryId, OpenMode.ForWrite) as DBDictionary;
				if (!(dbdictionary == null))
				{
					if (dbdictionary.Contains(TraceData.DynamoTraceData))
					{
						DBDictionary dbdictionary2 = documentContext.Transaction.GetObject(dbdictionary.GetAt(TraceData.DynamoTraceData), OpenMode.ForWrite) as DBDictionary;
						if (objectId.IsNull)
						{
							dbdictionary2.Erase();
						}
						else if (objectId.IsValid && !objectId.IsErased)
						{
							string text = string.Empty;
							foreach (DBDictionaryEntry dbdictionaryEntry in dbdictionary2)
							{
								Xrecord xrecord = documentContext.Transaction.GetObject(dbdictionaryEntry.Value, 0) as Xrecord;
								if (!(xrecord == null))
								{
									List<CallSite.RawTraceData> traceData = TraceData.GetTraceData(xrecord.Data);
									if (traceData.Any<CallSite.RawTraceData>())
									{
										foreach (CallSite.RawTraceData rawTraceData in traceData)
										{
											if (TraceData.GetObjectIdsFromRawTraceData(document, rawTraceData).Contains(objectId))
											{
												text = dbdictionaryEntry.Key;
												break;
											}
										}
										if (!string.IsNullOrEmpty(text))
										{
											break;
										}
									}
								}
							}
							if (!string.IsNullOrEmpty(text))
							{
								dbdictionary2.Remove(text);
							}
						}
					}
				}
			}
		}

		// Token: 0x0600007B RID: 123 RVA: 0x00003AB0 File Offset: 0x00002AB0
		public static IDictionary<ObjectId, Guid> GetFromDocument(Document document)
		{
			if (document == null)
			{
				return null;
			}
			IDictionary<Guid, List<CallSite.RawTraceData>> dictionary = TraceData.ReadTraceData(document);
			if (dictionary == null || !dictionary.Any<KeyValuePair<Guid, List<CallSite.RawTraceData>>>())
			{
				return null;
			}
			Dictionary<ObjectId, Guid> dictionary2 = new Dictionary<ObjectId, Guid>();
			foreach (KeyValuePair<Guid, List<CallSite.RawTraceData>> keyValuePair in dictionary)
			{
				foreach (CallSite.RawTraceData rawTraceData in keyValuePair.Value)
				{
					foreach (object obj in TraceData.GetObjectIdsFromRawTraceData(document, rawTraceData))
					{
						if (obj is ObjectId)
						{
							ObjectId key = (ObjectId)obj;
							dictionary2.Add(key, keyValuePair.Key);
						}
					}
				}
			}
			return dictionary2;
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x0600007C RID: 124 RVA: 0x00003BC4 File Offset: 0x00002BC4
		// (set) Token: 0x0600007D RID: 125 RVA: 0x00003BCB File Offset: 0x00002BCB
		public static TraceData.TraceDataState SaveState { get; private set; } = TraceData.TraceDataState.DynAndDwg;

		// Token: 0x0600007E RID: 126 RVA: 0x00003BD3 File Offset: 0x00002BD3
		public static void UpdateSaveState()
		{
			TraceData.SaveState = (TraceData.TraceDataState)TraceData.GetTraceDataSysVarValue();
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00003BE0 File Offset: 0x00002BE0
		private static IEnumerable<Guid> GetNodeGuids(HomeWorkspaceModel hws)
		{
			return from n in hws.Nodes
			where ElementBinder.IsZeroTouchNode(n)
			select n.GUID;
		}

		// Token: 0x06000080 RID: 128 RVA: 0x00003C3D File Offset: 0x00002C3D
		private static RuntimeCore GetRuntimCore(HomeWorkspaceModel hws)
		{
			if (!hws.Nodes.Any<NodeModel>())
			{
				return null;
			}
			if (hws.EngineController == null || hws.EngineController.LiveRunnerCore == null)
			{
				return null;
			}
			return hws.EngineController.LiveRunnerRuntimeCore;
		}

		// Token: 0x06000081 RID: 129 RVA: 0x00003C70 File Offset: 0x00002C70
		private static IDictionary<Guid, List<CallSite.RawTraceData>> GetTraceData(HomeWorkspaceModel hws)
		{
			RuntimeCore runtimCore = TraceData.GetRuntimCore(hws);
			if (runtimCore == null)
			{
				return null;
			}
			IEnumerable<Guid> nodeGuids = TraceData.GetNodeGuids(hws);
			if (!nodeGuids.Any<Guid>())
			{
				return null;
			}
			return runtimCore.RuntimeData.GetTraceDataForNodes(nodeGuids, runtimCore.DSExecutable);
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00003CAC File Offset: 0x00002CAC
		private static ObjectIdCollection GetObjectIdsFromRawTraceData(Document document, CallSite.RawTraceData rawTraceData)
		{
			ObjectIdCollection objectIdCollection = new ObjectIdCollection();
			IList<ISerializable> allSerializablesFromSingleRunTraceData = CallSite.GetAllSerializablesFromSingleRunTraceData(rawTraceData);
			foreach (ISerializable serializable in allSerializablesFromSingleRunTraceData)
			{
				SerializableHandle serializableHandle = serializable as SerializableHandle;
				if (serializableHandle != null)
				{
					string stringID = serializableHandle.stringID;
					ObjectId objectId = DocumentContext.GetObjectId(document.Database, stringID);
					if (objectId.IsValid && !objectId.IsErased)
					{
						objectIdCollection.Add(objectId);
					}
				}
			}
			return objectIdCollection;
		}

		// Token: 0x06000083 RID: 131 RVA: 0x00003D3C File Offset: 0x00002D3C
		private static void WriteTraceData(IDictionary<Guid, List<CallSite.RawTraceData>> traceData)
		{
			using (DocumentContext documentContext = new DocumentContext(DocumentContext.StartupDocument))
			{
				DBDictionary dbdictionary = documentContext.Transaction.GetObject(documentContext.Database.NamedObjectsDictionaryId, OpenMode.ForRead) as DBDictionary;
				if (!(dbdictionary == null))
				{
					DBDictionary dbdictionary2;
					if (dbdictionary.Contains(TraceData.DynamoTraceData))
					{
						dbdictionary2 = (documentContext.Transaction.GetObject(dbdictionary.GetAt(TraceData.DynamoTraceData), OpenMode.ForWrite) as DBDictionary);
					}
					else
					{
						dbdictionary2 = new DBDictionary();
						dbdictionary.UpgradeOpen();
						dbdictionary.SetAt(TraceData.DynamoTraceData, dbdictionary2);
						documentContext.Transaction.AddNewlyCreatedDBObject(dbdictionary2, true);
						dbdictionary.DowngradeOpen();
					}
					if (!(dbdictionary2 == null))
					{
						foreach (KeyValuePair<Guid, List<CallSite.RawTraceData>> keyValuePair in traceData)
						{
							string text = keyValuePair.Key.ToString();
							Xrecord xrecord;
							if (dbdictionary2.Contains(text))
							{
								xrecord = (documentContext.Transaction.GetObject(dbdictionary2.GetAt(text), OpenMode.ForWrite) as Xrecord);
							}
							else
							{
								xrecord = new Xrecord();
								dbdictionary2.SetAt(text, xrecord);
								documentContext.Transaction.AddNewlyCreatedDBObject(xrecord, true);
							}
							using (ResultBuffer resultBuffer = new ResultBuffer())
							{
								foreach (CallSite.RawTraceData traceData2 in keyValuePair.Value)
								{
									foreach (string text2 in TraceData.SplitTraceData(traceData2))
									{
										resultBuffer.Add(new TypedValue(Convert.ToInt32(1), text2));
									}
								}
								xrecord.Data = resultBuffer;
							}
						}
					}
				}
			}
		}

		// Token: 0x06000084 RID: 132 RVA: 0x00003F90 File Offset: 0x00002F90
		private static void RemoveTraceData(IEnumerable<Guid> guids)
		{
			using (DocumentContext documentContext = new DocumentContext(DocumentContext.StartupDocument))
			{
				DBDictionary dbdictionary = documentContext.Transaction.GetObject(documentContext.Database.NamedObjectsDictionaryId, OpenMode.ForRead) as DBDictionary;
				if (!(dbdictionary == null))
				{
					if (dbdictionary.Contains(TraceData.DynamoTraceData))
					{
						DBDictionary dbdictionary2 = documentContext.Transaction.GetObject(dbdictionary.GetAt(TraceData.DynamoTraceData), OpenMode.ForWrite) as DBDictionary;
						foreach (Guid guid in guids)
						{
							string text = guid.ToString();
							if (dbdictionary2.Contains(text))
							{
								dbdictionary2.Remove(text);
							}
						}
					}
				}
			}
		}

		// Token: 0x06000085 RID: 133 RVA: 0x0000406C File Offset: 0x0000306C
		private static IEnumerable<string> SplitTraceData(CallSite.RawTraceData traceData)
		{
			List<string> list = new List<string>();
			string item = TraceData.DynamoTraceDataItemStart + traceData.ID;
			list.Add(item);
			string data = traceData.Data;
			int num = data.Length / 218;
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				num2 = i * 218;
				list.Add(data.Substring(num2, 218));
			}
			int num3 = data.Length % 218;
			if (num3 > 0)
			{
				num2 += 218;
				list.Add(data.Substring(num2, num3));
			}
			string item2 = TraceData.DynamoTraceDataItemEnd + traceData.ID;
			list.Add(item2);
			return list;
		}

		// Token: 0x06000086 RID: 134 RVA: 0x00004124 File Offset: 0x00003124
		private static List<CallSite.RawTraceData> GetTraceData(ResultBuffer resbufTraceData)
		{
			string text = string.Empty;
			List<CallSite.RawTraceData> list = new List<CallSite.RawTraceData>();
			StringBuilder stringBuilder = new StringBuilder();
			foreach (TypedValue typedValue in resbufTraceData)
			{
				string text2 = typedValue.Value.ToString();
				if (text2.StartsWith(TraceData.DynamoTraceDataItemStart))
				{
					text = text2.Substring(TraceData.DynamoTraceDataItemStart.Length);
				}
				else if (text2.StartsWith(TraceData.DynamoTraceDataItemEnd))
				{
					string text3 = stringBuilder.ToString();
					if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text3))
					{
						list.Add(new CallSite.RawTraceData(text, text3));
					}
					text = string.Empty;
					stringBuilder.Clear();
				}
				else
				{
					stringBuilder.Append(text2);
				}
			}
			return list;
		}

		// Token: 0x06000087 RID: 135 RVA: 0x000041E0 File Offset: 0x000031E0
		private static IDictionary<Guid, List<CallSite.RawTraceData>> ReadTraceData(HomeWorkspaceModel hws)
		{
			Dictionary<Guid, List<CallSite.RawTraceData>> dictionary = new Dictionary<Guid, List<CallSite.RawTraceData>>();
			using (DocumentContext documentContext = new DocumentContext(DocumentContext.StartupDocument))
			{
				DBDictionary dbdictionary = documentContext.Transaction.GetObject(documentContext.Database.NamedObjectsDictionaryId, 0) as DBDictionary;
				if (dbdictionary == null)
				{
					return null;
				}
				if (!dbdictionary.Contains(TraceData.DynamoTraceData))
				{
					return null;
				}
				DBDictionary dbdictionary2 = documentContext.Transaction.GetObject(dbdictionary.GetAt(TraceData.DynamoTraceData), 0) as DBDictionary;
				IEnumerable<Guid> nodeGuids = TraceData.GetNodeGuids(hws);
				if (!nodeGuids.Any<Guid>())
				{
					return null;
				}
				foreach (Guid key in nodeGuids)
				{
					if (dbdictionary2.Contains(key.ToString()))
					{
						Xrecord xrecord = documentContext.Transaction.GetObject(dbdictionary2.GetAt(key.ToString()), 0) as Xrecord;
						if (!(xrecord == null))
						{
							List<CallSite.RawTraceData> traceData = TraceData.GetTraceData(xrecord.Data);
							if (traceData.Any<CallSite.RawTraceData>())
							{
								dictionary[key] = traceData;
							}
						}
					}
				}
			}
			return dictionary;
		}

		// Token: 0x06000088 RID: 136 RVA: 0x0000434C File Offset: 0x0000334C
		private static IDictionary<Guid, List<CallSite.RawTraceData>> ReadTraceData(Document document)
		{
			if (document == null)
			{
				return null;
			}
			Dictionary<Guid, List<CallSite.RawTraceData>> dictionary = new Dictionary<Guid, List<CallSite.RawTraceData>>();
			using (DocumentContext documentContext = new DocumentContext(document))
			{
				DBDictionary dbdictionary = documentContext.Transaction.GetObject(documentContext.Database.NamedObjectsDictionaryId, 0) as DBDictionary;
				if (dbdictionary == null)
				{
					return null;
				}
				if (!dbdictionary.Contains(TraceData.DynamoTraceData))
				{
					return null;
				}
				DBDictionary dbdictionary2 = documentContext.Transaction.GetObject(dbdictionary.GetAt(TraceData.DynamoTraceData), 0) as DBDictionary;
				foreach (DBDictionaryEntry dbdictionaryEntry in dbdictionary2)
				{
					Xrecord xrecord = documentContext.Transaction.GetObject(dbdictionaryEntry.Value, 0) as Xrecord;
					if (!(xrecord == null))
					{
						List<CallSite.RawTraceData> traceData = TraceData.GetTraceData(xrecord.Data);
						if (traceData.Any<CallSite.RawTraceData>())
						{
							dictionary[new Guid(dbdictionaryEntry.Key)] = traceData;
						}
					}
				}
			}
			return dictionary;
		}

		// Token: 0x06000089 RID: 137 RVA: 0x00004474 File Offset: 0x00003474
		private static int GetTraceDataSysVarValue()
		{
			int num = 2;
			int result;
			try
			{
				object systemVariable = Application.GetSystemVariable(TraceData.DynamoTraceDataSysVar);
				result = ((systemVariable == null) ? num : Convert.ToInt32(systemVariable));
			}
			catch
			{
				result = num;
			}
			return result;
		}

		// Token: 0x04000033 RID: 51
		private static readonly string DynamoTraceData = "DYNAMO_TRACE_DATA";

		// Token: 0x04000034 RID: 52
		private static readonly string DynamoTraceDataItemStart = "DYNAMO_TRACE_DATA_ITEM_START_";

		// Token: 0x04000035 RID: 53
		private static readonly string DynamoTraceDataItemEnd = "DYNAMO_TRACE_DATA_ITEM_END_";

		// Token: 0x04000036 RID: 54
		private static readonly string DynamoTraceDataSysVar = "AECCDYNAMOTRACEDATASTATE";

		// Token: 0x02000014 RID: 20
		public enum TraceDataState
		{
			// Token: 0x04000041 RID: 65
			WithoutSaving,
			// Token: 0x04000042 RID: 66
			DynOnly,
			// Token: 0x04000043 RID: 67
			DynAndDwg
		}
	}
}
