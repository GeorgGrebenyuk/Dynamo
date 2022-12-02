using System;
using System.Collections.Generic;
using HostMgd.ApplicationServices;
using Teigha.DatabaseServices;

namespace nanoCAD.DynamoApp.Services
{
	// Token: 0x02000004 RID: 4
	public class DrawingReactors : IDisposable
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000002 RID: 2 RVA: 0x00002050 File Offset: 0x00001050
		public static DrawingReactors Instance
		{
			get
			{
				if (DrawingReactors.instance == null)
				{
					object obj = DrawingReactors.mutex;
					lock (obj)
					{
						if (DrawingReactors.instance == null)
						{
							DrawingReactors.instance = new DrawingReactors();
						}
					}
				}
				return DrawingReactors.instance;
			}
		}

		// Token: 0x14000001 RID: 1
		// (add) Token: 0x06000003 RID: 3 RVA: 0x000020A8 File Offset: 0x000010A8
		// (remove) Token: 0x06000004 RID: 4 RVA: 0x000020E0 File Offset: 0x000010E0
		public event Action<ObjectUpdateType, DBObject> ObjectUpdated;

		// Token: 0x14000002 RID: 2
		// (add) Token: 0x06000005 RID: 5 RVA: 0x00002118 File Offset: 0x00001118
		// (remove) Token: 0x06000006 RID: 6 RVA: 0x00002150 File Offset: 0x00001150
		public event Action<Document> DocumentActivated;

		// Token: 0x14000003 RID: 3
		// (add) Token: 0x06000007 RID: 7 RVA: 0x00002188 File Offset: 0x00001188
		// (remove) Token: 0x06000008 RID: 8 RVA: 0x000021C0 File Offset: 0x000011C0
		public event Action<Document> DocumentToBeDestroyed;

		// Token: 0x14000004 RID: 4
		// (add) Token: 0x06000009 RID: 9 RVA: 0x000021F8 File Offset: 0x000011F8
		// (remove) Token: 0x0600000A RID: 10 RVA: 0x00002230 File Offset: 0x00001230
		public event Action<Document, string> CommandEnded;

		// Token: 0x14000005 RID: 5
		// (add) Token: 0x0600000B RID: 11 RVA: 0x00002268 File Offset: 0x00001268
		// (remove) Token: 0x0600000C RID: 12 RVA: 0x000022A0 File Offset: 0x000012A0
		public event Action<Document, string> ModelessOperationEnded;

		// Token: 0x0600000D RID: 13 RVA: 0x000022D5 File Offset: 0x000012D5
		private void OnObjectUpdated(ObjectUpdateType type, DBObject obj)
		{
			if (obj is Entity)
			{
				Action<ObjectUpdateType, DBObject> objectUpdated = this.ObjectUpdated;
				if (objectUpdated == null)
				{
					return;
				}
				objectUpdated(type, obj);
			}
		}

		// Token: 0x0600000E RID: 14 RVA: 0x000022F1 File Offset: 0x000012F1
		internal DrawingReactors()
		{
			Application.DocumentManager.DocumentActivated += new DocumentCollectionEventHandler(this.DocumentManager_DocumentActivated);
			Application.DocumentManager.DocumentToBeDestroyed += new DocumentCollectionEventHandler(this.DocumentManager_DocumentToBeDestroyed);
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002330 File Offset: 0x00001330
		public void Dispose()
		{
			Application.DocumentManager.DocumentActivated -= new DocumentCollectionEventHandler(this.DocumentManager_DocumentActivated);
			Application.DocumentManager.DocumentToBeDestroyed -= new DocumentCollectionEventHandler(this.DocumentManager_DocumentToBeDestroyed);
			while (this.allDocuments.Count > 0)
			{
				this.teardownDocumentReactors(this.allDocuments[0]);
			}
		}

		// Token: 0x06000010 RID: 16 RVA: 0x0000238C File Offset: 0x0000138C
		public void setupDocumentReactors(Document doc)
		{
			try
			{
				doc.Database.ObjectAppended += new ObjectEventHandler(this.Database_ObjectAdded);
				doc.Database.ObjectReappended += new ObjectEventHandler(this.Database_ObjectReappended);
				doc.Database.ObjectModified += new ObjectEventHandler(this.Database_ObjectModified);
				doc.Database.ObjectErased += new ObjectErasedEventHandler(this.Database_ObjectErased);
				doc.Database.ObjectUnappended += new ObjectEventHandler(this.Database_ObjectUnappended);
				doc.CommandEnded += new CommandEventHandler(this.Doc_CommandEnded);
				//doc.ModelessOperationEnded += new ModelessOperationEventHandler(this.Doc_ModelessOperationEnded);
			}
			catch
			{
			}
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00002448 File Offset: 0x00001448
		public void teardownDocumentReactors(Document doc)
		{
			try
			{
				this.allDocuments.Remove(doc);
				doc.Database.ObjectAppended -= new ObjectEventHandler(this.Database_ObjectAdded);
				doc.Database.ObjectReappended -= new ObjectEventHandler(this.Database_ObjectReappended);
				doc.Database.ObjectModified -= new ObjectEventHandler(this.Database_ObjectModified);
				doc.Database.ObjectErased -= new ObjectErasedEventHandler(this.Database_ObjectErased);
				doc.Database.ObjectUnappended -= new ObjectEventHandler(this.Database_ObjectUnappended);
				doc.CommandEnded -= new CommandEventHandler(this.Doc_CommandEnded);
				//doc.ModelessOperationEnded -= new ModelessOperationEventHandler(this.Doc_ModelessOperationEnded);
			}
			catch
			{
			}
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002510 File Offset: 0x00001510
		private void DocumentManager_DocumentActivated(object sender, DocumentCollectionEventArgs e)
		{
			Action<Document> documentActivated = this.DocumentActivated;
			if (documentActivated == null)
			{
				return;
			}
			documentActivated(e.Document);
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002528 File Offset: 0x00001528
		private void DocumentManager_DocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e)
		{
			Action<Document> documentToBeDestroyed = this.DocumentToBeDestroyed;
			if (documentToBeDestroyed != null)
			{
				documentToBeDestroyed(e.Document);
			}
			this.teardownDocumentReactors(e.Document);
			DocumentContext.tearDownDocument(e.Document);
		}

		// Token: 0x06000014 RID: 20 RVA: 0x00002558 File Offset: 0x00001558
		private void Doc_CommandEnded(object sender, CommandEventArgs e)
		{
			Action<Document, string> commandEnded = this.CommandEnded;
			if (commandEnded == null)
			{
				return;
			}
			commandEnded(sender as Document, e.GlobalCommandName);
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00002576 File Offset: 0x00001576
		//private void Doc_ModelessOperationEnded(object sender, ModelessOperationEventArgs e)
		//{
		//	Action<Document, string> modelessOperationEnded = this.ModelessOperationEnded;
		//	if (modelessOperationEnded == null)
		//	{
		//		return;
		//	}
		//	modelessOperationEnded(sender as Document, e.Context);
		//}

		// Token: 0x06000016 RID: 22 RVA: 0x00002594 File Offset: 0x00001594
		private void Database_ObjectAdded(object sender, ObjectEventArgs e)
		{
			this.OnObjectUpdated(ObjectUpdateType.Added, e.DBObject);
		}

		// Token: 0x06000017 RID: 23 RVA: 0x00002594 File Offset: 0x00001594
		private void Database_ObjectReappended(object sender, ObjectEventArgs e)
		{
			this.OnObjectUpdated(ObjectUpdateType.Added, e.DBObject);
		}

		// Token: 0x06000018 RID: 24 RVA: 0x000025A3 File Offset: 0x000015A3
		private void Database_ObjectModified(object sender, ObjectEventArgs e)
		{
			this.OnObjectUpdated(ObjectUpdateType.Modified, e.DBObject);
		}

		// Token: 0x06000019 RID: 25 RVA: 0x000025B2 File Offset: 0x000015B2
		private void Database_ObjectErased(object sender, ObjectErasedEventArgs e)
		{
			if (e.Erased)
			{
				this.OnObjectUpdated(ObjectUpdateType.Erased, e.DBObject);
				return;
			}
			this.OnObjectUpdated(ObjectUpdateType.Added, e.DBObject);
		}

		// Token: 0x0600001A RID: 26 RVA: 0x000025D7 File Offset: 0x000015D7
		private void Database_ObjectUnappended(object sender, ObjectEventArgs e)
		{
			this.OnObjectUpdated(ObjectUpdateType.Erased, e.DBObject);
		}

		// Token: 0x0400000E RID: 14
		private static object mutex = new object();

		// Token: 0x0400000F RID: 15
		private static DrawingReactors instance = null;

		// Token: 0x04000010 RID: 16
		private IList<Document> allDocuments = new List<Document>();
	}
}
