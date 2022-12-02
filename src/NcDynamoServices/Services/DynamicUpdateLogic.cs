using System;
using System.Linq;
using System.Reflection;
using HostMgd.ApplicationServices;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;

namespace nanoCAD.DynamoApp.Services
{
	// Token: 0x02000005 RID: 5
	public class DynamicUpdateLogic
	{
		// Token: 0x17000002 RID: 2
		// (get) Token: 0x0600001C RID: 28 RVA: 0x000025F8 File Offset: 0x000015F8
		public static DynamicUpdateLogic Instance
		{
			get
			{
				if (DynamicUpdateLogic.theInstance == null)
				{
					object obj = DynamicUpdateLogic.mutex;
					lock (obj)
					{
						if (DynamicUpdateLogic.theInstance == null)
						{
							DynamicUpdateLogic.theInstance = new DynamicUpdateLogic();
						}
					}
				}
				return DynamicUpdateLogic.theInstance;
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600001D RID: 29 RVA: 0x00002650 File Offset: 0x00001650
		// (set) Token: 0x0600001E RID: 30 RVA: 0x00002658 File Offset: 0x00001658
		public bool ForceDisabled { get; set; }

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600001F RID: 31 RVA: 0x00002664 File Offset: 0x00001664
		public bool Available
		{
			get
			{
				if (this.ForceDisabled)
				{
					return false;
				}
				if (DocumentContext.MdiActiveDocument != DocumentContext.StartupDocument)
				{
					return false;
				}
				Document startupDocument = DocumentContext.StartupDocument;
				string text = ((startupDocument != null) ? startupDocument.CommandInProgress : null) ?? "";
				text = text.ToUpper();
				return !(text == "CLOSE") && !(text == "QUIT") && !(text == "QSAVE") && !(text == "SAVEAS") && !(text == "SAVETOCLOUD") && !(text == "PUBLISH");
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000020 RID: 32 RVA: 0x00002701 File Offset: 0x00001701
		// (set) Token: 0x06000021 RID: 33 RVA: 0x00002709 File Offset: 0x00001709
		public bool HasModifiedNodes { get; private set; }

		// Token: 0x06000022 RID: 34 RVA: 0x00002712 File Offset: 0x00001712
		public void MarkNodeAsModified(NodeModel node)
		{
			node.MarkNodeAsModified(true);
			this.HasModifiedNodes = true;
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00002724 File Offset: 0x00001724
		public void CheckAndRun(DynamoModel model)
		{
			if (DocumentContext.MdiActiveDocument != DocumentContext.StartupDocument)
			{
				return;
			}
			if (this.HasModifiedNodes)
			{
				this.HasModifiedNodes = false;
				foreach (HomeWorkspaceModel homeWorkspaceModel in model.Workspaces.OfType<HomeWorkspaceModel>())
				{
					try
					{
						Type type = homeWorkspaceModel.GetType();
						MethodInfo method = type.GetMethod("RequestRun", BindingFlags.Public | BindingFlags.NonPublic);
						method.Invoke(homeWorkspaceModel, new object[0]);
					}
					catch
					{
						if (homeWorkspaceModel.RunSettings.RunType != null)
						{
							homeWorkspaceModel.Run();
						}
					}
				}
			}
		}

		// Token: 0x04000016 RID: 22
		private static object mutex = new object();

		// Token: 0x04000017 RID: 23
		private static DynamicUpdateLogic theInstance;
	}
}
