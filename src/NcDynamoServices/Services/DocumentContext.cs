using System;
using System.Collections.Generic;
using System.Linq;
using HostMgd.ApplicationServices;
using Teigha.DatabaseServices;
using nanoCAD.DynamoApp.Resources;

namespace nanoCAD.DynamoApp.Services
{
	// Token: 0x02000008 RID: 8
	public class DocumentContext : IDisposable
	{
		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000036 RID: 54 RVA: 0x00002D36 File Offset: 0x00001D36
		// (set) Token: 0x06000037 RID: 55 RVA: 0x00002D3D File Offset: 0x00001D3D
		public static bool EvaluationInProgress { get; set; }

		// Token: 0x06000038 RID: 56 RVA: 0x00002D45 File Offset: 0x00001D45
		public DocumentContext(Database db) : this(db, false)
		{
		}

		// Token: 0x06000039 RID: 57 RVA: 0x00002D4F File Offset: 0x00001D4F
		public DocumentContext(Database db, bool tearDownOnDispose)
		{
			this.Construct((db != null) ? Application.DocumentManager.GetDocument(db) : null);
			this.tearDownOnDispose = tearDownOnDispose;
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00002D7B File Offset: 0x00001D7B
		public DocumentContext(Document doc) : this(doc, false)
		{
		}

		// Token: 0x0600003B RID: 59 RVA: 0x00002D85 File Offset: 0x00001D85
		public DocumentContext(Document doc, bool tearDownOnDispose)
		{
			this.Construct(doc);
			this.tearDownOnDispose = tearDownOnDispose;
		}

		// Token: 0x0600003C RID: 60 RVA: 0x00002D9C File Offset: 0x00001D9C
		private void Construct(Document doc)
		{
			if (DocumentContext.StartupDocument != null && DocumentContext.StartupDocument != DocumentContext.MdiActiveDocument)
			{
				//throw new InvalidOperationException(Resources.EXCEPTION_MESSAGE_DIFFERNT_DOCUMENT);
			}
			Document doc2 = (doc != null) ? doc : (DocumentContext.StartupDocument ?? DocumentContext.MdiActiveDocument);
			DocumentContext.setupDocument(doc2);
			this.thisDocument = doc2;
		}

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x0600003D RID: 61 RVA: 0x00002DFA File Offset: 0x00001DFA
		// (set) Token: 0x0600003E RID: 62 RVA: 0x00002E01 File Offset: 0x00001E01
		public static Document StartupDocument
		{
			get
			{
				return DocumentContext.startupDocument;
			}
			set
			{
				if (DocumentContext.startupDocument != null)
				{
					DrawingReactors.Instance.teardownDocumentReactors(DocumentContext.startupDocument);
				}
				DocumentContext.startupDocument = value;
				if (DocumentContext.startupDocument != null)
				{
					DrawingReactors.Instance.setupDocumentReactors(DocumentContext.startupDocument);
				}
			}
		}

		// Token: 0x0600003F RID: 63 RVA: 0x00002E44 File Offset: 0x00001E44
		public static ObjectId GetObjectId(Database db, string handle)
		{
			try
			{
				if (!string.IsNullOrEmpty(handle))
				{
					return db.GetObjectId(false, new Handle(Convert.ToInt64(handle, 16)), 0);
				}
			}
			catch
			{
			}
			return ObjectId.Null;
		}

		// Token: 0x06000040 RID: 64 RVA: 0x00002E90 File Offset: 0x00001E90
		public ObjectId GetObjectId(string handle)
		{
			Database database = this.Database;
			if (database != null)
			{
				return DocumentContext.GetObjectId(database, handle);
			}
			return ObjectId.Null;
		}

		// Token: 0x06000041 RID: 65 RVA: 0x00002EBC File Offset: 0x00001EBC
		private static void TearDownAllDocuments()
		{
			while (DocumentContext.documentLocks.Count > 0)
			{
				DocumentContext.tearDownDocument(DocumentContext.documentLocks.Keys.First<Document>());
			}
			while (DocumentContext.transactions.Count > 0)
			{
				DocumentContext.tearDownDocument(DocumentContext.transactions.Keys.First<Document>());
			}
		}

		// Token: 0x06000042 RID: 66 RVA: 0x00002F0F File Offset: 0x00001F0F
		public static void OnModelRefreshCompleted()
		{
			DocumentContext.TearDownAllDocuments();
		}

		// Token: 0x06000043 RID: 67 RVA: 0x00002F16 File Offset: 0x00001F16
		public void Dispose()
		{
			if (this.tearDownOnDispose && this.thisDocument != null)
			{
				DocumentContext.tearDownDocument(this.thisDocument);
			}
		}

		// Token: 0x06000044 RID: 68 RVA: 0x00002F3C File Offset: 0x00001F3C
		internal static void setupDocument(Document doc)
		{
			if (doc != null)
			{
				if (!DocumentContext.documentLocks.ContainsKey(doc))
				{
					DocumentContext.documentLocks.Add(doc, doc.LockDocument());
				}
				if (!DocumentContext.transactions.ContainsKey(doc))
				{
					DocumentContext.transactions.Add(doc, doc.TransactionManager.StartTransaction());
				}
			}
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00002F94 File Offset: 0x00001F94
		internal static void tearDownDocument(Document doc)
		{
			if (doc != null)
			{
				if (DocumentContext.transactions.ContainsKey(doc))
				{
					try
					{
						DocumentContext.transactions[doc].Commit();
					}
					catch
					{
					}
					DocumentContext.transactions.Remove(doc);
				}
				if (DocumentContext.documentLocks.ContainsKey(doc))
				{
					try
					{
						DocumentContext.documentLocks[doc].Dispose();
					}
					catch
					{
					}
					DocumentContext.documentLocks.Remove(doc);
				}
			}
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000046 RID: 70 RVA: 0x00003024 File Offset: 0x00002024
		public Document Document
		{
			get
			{
				return this.thisDocument;
			}
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000047 RID: 71 RVA: 0x0000302C File Offset: 0x0000202C
		public Database Database
		{
			get
			{
				Document document = this.thisDocument;
				if (document == null)
				{
					return null;
				}
				return document.Database;
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000048 RID: 72 RVA: 0x0000303F File Offset: 0x0000203F
		public Transaction Transaction
		{
			get
			{
				return this.GetTransaction(this.Document);
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000049 RID: 73 RVA: 0x0000304D File Offset: 0x0000204D
		public static Document MdiActiveDocument
		{
			get
			{
				return Application.DocumentManager.MdiActiveDocument;
			}
		}

		// Token: 0x0600004A RID: 74 RVA: 0x0000305C File Offset: 0x0000205C
		public Transaction GetTransaction(Document doc = null)
		{
			Document document = (doc != null) ? doc : DocumentContext.MdiActiveDocument;
			if (document != null && DocumentContext.transactions.ContainsKey(document))
			{
				return DocumentContext.transactions[document];
			}
			return null;
		}

		// Token: 0x0400001E RID: 30
		private static Document startupDocument;

		// Token: 0x04000020 RID: 32
		private Document thisDocument;

		// Token: 0x04000021 RID: 33
		private readonly bool tearDownOnDispose;

		// Token: 0x04000022 RID: 34
		private static Dictionary<Document, DocumentLock> documentLocks = new Dictionary<Document, DocumentLock>();

		// Token: 0x04000023 RID: 35
		private static Dictionary<Document, Transaction> transactions = new Dictionary<Document, Transaction>();
	}
}
