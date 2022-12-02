using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;


using HostMgd.ApplicationServices;
using Teigha.DatabaseServices;
using nanoCAD.DynamoApp.Services;
using DSIronPython;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Scheduler;
using Microsoft.Scripting.Hosting;
using ProtoCore;
using ProtoCore.Runtime;

namespace nanoCAD.DynamoApp
{
	// Token: 0x02000007 RID: 7
	public class AcDynamoModel : DynamoModel
	{
		// Token: 0x14000004 RID: 4
		// (add) Token: 0x0600003F RID: 63 RVA: 0x00002D04 File Offset: 0x00001D04
		// (remove) Token: 0x06000040 RID: 64 RVA: 0x00002D3C File Offset: 0x00001D3C
		public event Action<AcDynamoModel.DocumentEventType> DocumentEvent;

		// Token: 0x06000041 RID: 65 RVA: 0x00002D74 File Offset: 0x00001D74
		public static AcDynamoModel Start()
		{
			return AcDynamoModel.Start(default(DynamoModel.DefaultStartConfiguration));
		}

		// Token: 0x06000042 RID: 66 RVA: 0x00002D94 File Offset: 0x00001D94
		public static AcDynamoModel Start(DynamoModel.IStartConfiguration configuration)
		{
			if (string.IsNullOrEmpty(configuration.Context))
			{
				configuration.Context = "Civil3D";
			}
			if (string.IsNullOrEmpty(configuration.DynamoCorePath))
			{
				string location = Assembly.GetExecutingAssembly().Location;
				configuration.DynamoCorePath = Path.GetDirectoryName(location);
			}
			return new AcDynamoModel(configuration);
		}

		// Token: 0x06000043 RID: 67 RVA: 0x00002DE4 File Offset: 0x00001DE4
		private AcDynamoModel(DynamoModel.IStartConfiguration configuration) : base(configuration)
		{
			DisposeLogic.IsShuttingDown = false;
			DrawingReactors instance = DrawingReactors.Instance;
			instance.ObjectUpdated += this.OnDrawingObjectsUpdated;
			instance.DocumentActivated += this.OnDocumentActivated;
			instance.DocumentToBeDestroyed += this.OnDocumentToBeDestroyed;
			instance.CommandEnded += this.OnCommandEnded;
			instance.ModelessOperationEnded += this.OnModelessOperationEnded;
			this.SetupPython();
		}

		// Token: 0x06000044 RID: 68 RVA: 0x00002E64 File Offset: 0x00001E64
		private void SetupPython()
		{
			if (this.setupPython)
			{
				return;
			}
			IronPythonEvaluator.EvaluationBegin += delegate(EvaluationState a, ScriptEngine b, ScriptScope c, string d, IList e)
			{
				ElementBinder.IsEnabled = false;
			};
			IronPythonEvaluator.EvaluationEnd += delegate(EvaluationState a, ScriptEngine b, ScriptScope c, string d, IList e)
			{
				ElementBinder.IsEnabled = true;
			};
			this.setupPython = true;
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00002ECC File Offset: 0x00001ECC
		public void ExecuteOnIdleAsync(Action p, AsyncTaskCompletedHandler completionHandler = null)
		{
			DelegateBasedAsyncTask delegateBasedAsyncTask = new DelegateBasedAsyncTask(base.Scheduler, p);
			if (completionHandler != null)
			{
				delegateBasedAsyncTask.Completed += completionHandler;
			}
			base.Scheduler.ScheduleForExecution(delegateBasedAsyncTask);
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00002EFC File Offset: 0x00001EFC
		protected override void ShutDownCore(bool shutdownHost)
		{
			DisposeLogic.IsShuttingDown = true;
			base.ShutDownCore(shutdownHost);
			DocumentContext.StartupDocument = null;
			DrawingReactors instance = DrawingReactors.Instance;
			instance.ObjectUpdated -= this.OnDrawingObjectsUpdated;
			instance.DocumentActivated -= this.OnDocumentActivated;
			instance.DocumentToBeDestroyed -= this.OnDocumentToBeDestroyed;
			instance.CommandEnded -= this.OnCommandEnded;
			instance.ModelessOperationEnded -= this.OnModelessOperationEnded;
			LifecycleManager<string>.Instance.ClearAll();
			AcDynamoRuntime.OnDynamoModelShutdown();
		}

		// Token: 0x06000047 RID: 71 RVA: 0x00002F8C File Offset: 0x00001F8C
		protected override void OnWorkspaceAdded(WorkspaceModel workspace)
		{
			base.OnWorkspaceAdded(workspace);
			HomeWorkspaceModel homeWorkspaceModel = workspace as HomeWorkspaceModel;
			if (homeWorkspaceModel != null)
			{
				homeWorkspaceModel.EvaluationStarted += this.OnEvaluationStarted;
				homeWorkspaceModel.RefreshCompleted += this.OnRefreshCompleted;
				if (DocumentContext.StartupDocument != null)
				{
					ElementBinder.CollectInvalidHandlesBeforeFirstRun(DocumentContext.StartupDocument.Database, homeWorkspaceModel, base.EngineController);
				}
				this.IsHomeWorkspaceFirstRun = true;
			}
			if (DocumentContext.StartupDocument != null)
			{
				bool flag = DocumentContext.StartupDocument.Equals(DocumentContext.MdiActiveDocument);
				this.SetRunEnabled(flag, flag ? AcDynamoModel.DocumentEventType.Available : AcDynamoModel.DocumentEventType.UnAvailable);
				if (!flag)
				{
					this.SetRunType(0);
					return;
				}
			}
			else if (DocumentContext.MdiActiveDocument == null)
			{
				this.SetRunEnabled(false, AcDynamoModel.DocumentEventType.Destroyed);
				this.SetRunType(0);
			}
		}

		// Token: 0x06000048 RID: 72 RVA: 0x0000304C File Offset: 0x0000204C
		protected override void OnWorkspaceRemoveStarted(WorkspaceModel workspace)
		{
			base.OnWorkspaceRemoveStarted(workspace);
			HomeWorkspaceModel homeWorkspaceModel = workspace as HomeWorkspaceModel;
			if (homeWorkspaceModel != null)
			{
				DisposeLogic.IsClosingHomeworkspace = true;
				homeWorkspaceModel.EvaluationStarted -= this.OnEvaluationStarted;
				homeWorkspaceModel.RefreshCompleted -= this.OnRefreshCompleted;
			}
		}

		// Token: 0x06000049 RID: 73 RVA: 0x00003094 File Offset: 0x00002094
		protected override void OnWorkspaceRemoved(WorkspaceModel workspace)
		{
			base.OnWorkspaceRemoved(workspace);
			if (workspace is HomeWorkspaceModel)
			{
				DisposeLogic.IsClosingHomeworkspace = false;
			}
		}

		// Token: 0x0600004A RID: 74 RVA: 0x000030AB File Offset: 0x000020AB
		public override void OnWorkspaceCleared(WorkspaceModel workspace)
		{
			base.OnWorkspaceCleared(workspace);
			if (workspace is HomeWorkspaceModel)
			{
				this.IsHomeWorkspaceFirstRun = false;
				LifecycleManager<string>.Instance.ClearAll();
			}
		}

		// Token: 0x0600004B RID: 75 RVA: 0x000030D0 File Offset: 0x000020D0
		private void MarkNodesAsDirty(DBObject obj)
		{
			DynamicUpdateLogic instance = DynamicUpdateLogic.Instance;
			if (instance.Available)
			{
				IEnumerable<NodeModel> nodesFromDBObject = ElementBinder.GetNodesFromDBObject(obj.Handle.ToString(), base.CurrentWorkspace, base.EngineController);
				foreach (NodeModel node in nodesFromDBObject)
				{
					instance.MarkNodeAsModified(node);
				}
			}
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00003150 File Offset: 0x00002150
		private void OnDrawingObjectsUpdated(ObjectUpdateType type, DBObject obj)
		{
			if (type == ObjectUpdateType.Added)
			{
				return;
			}
			this.MarkNodesAsDirty(obj);
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00003160 File Offset: 0x00002160
		private void SetRunEnabled(bool enabled, AcDynamoModel.DocumentEventType eventType)
		{
			foreach (HomeWorkspaceModel homeWorkspaceModel in base.Workspaces.OfType<HomeWorkspaceModel>())
			{
				homeWorkspaceModel.RunSettings.RunEnabled = enabled;
			}
			Action<AcDynamoModel.DocumentEventType> documentEvent = this.DocumentEvent;
			if (documentEvent == null)
			{
				return;
			}
			documentEvent(eventType);
		}

		// Token: 0x0600004E RID: 78 RVA: 0x000031C8 File Offset: 0x000021C8
		private void SetRunType(RunType type)
		{
			foreach (HomeWorkspaceModel homeWorkspaceModel in base.Workspaces.OfType<HomeWorkspaceModel>())
			{
				homeWorkspaceModel.RunSettings.RunType = type;
			}
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00003220 File Offset: 0x00002220
		private void OnDocumentActivated(Document document)
		{
			if (DocumentContext.StartupDocument != null)
			{
				bool flag = DocumentContext.StartupDocument.Equals(document);
				this.SetRunEnabled(flag, flag ? AcDynamoModel.DocumentEventType.Available : AcDynamoModel.DocumentEventType.UnAvailable);
				return;
			}
			if (document != null)
			{
				DocumentContext.StartupDocument = document;
				if (this.IsHomeWorkspaceFirstRun)
				{
					WorkspaceModel currentWorkspace = base.CurrentWorkspace;
					HomeWorkspaceModel homeWorkspaceModel = currentWorkspace as HomeWorkspaceModel;
					if (homeWorkspaceModel != null)
					{
						ElementBinder.CollectInvalidHandlesBeforeFirstRun(DocumentContext.StartupDocument.Database, homeWorkspaceModel, base.EngineController);
					}
				}
				this.SetRunEnabled(true, AcDynamoModel.DocumentEventType.Changed);
			}
		}

		// Token: 0x06000050 RID: 80 RVA: 0x0000329C File Offset: 0x0000229C
		private void OnDocumentToBeDestroyed(Document document)
		{
			DisposeLogic.IsDestroyingDocument = true;
			if (DocumentContext.StartupDocument != null && DocumentContext.StartupDocument.Equals(document))
			{
				DocumentContext.StartupDocument = null;
				this.SetRunEnabled(false, AcDynamoModel.DocumentEventType.Destroyed);
				this.SetRunType(0);
				this.ResetEngine(true);
				LifecycleManager<string>.Instance.ClearAll();
				if (DocumentContext.MdiActiveDocument != null && document != DocumentContext.MdiActiveDocument)
				{
					this.OnDocumentActivated(DocumentContext.MdiActiveDocument);
				}
			}
			DisposeLogic.IsDestroyingDocument = false;
		}

		// Token: 0x06000051 RID: 81 RVA: 0x0000331A File Offset: 0x0000231A
		private void OnCommandEnded(Document document, string commandName)
		{
			DynamicUpdateLogic.Instance.CheckAndRun(this);
		}

		// Token: 0x06000052 RID: 82 RVA: 0x0000331A File Offset: 0x0000231A
		private void OnModelessOperationEnded(Document document, string context)
		{
			DynamicUpdateLogic.Instance.CheckAndRun(this);
		}

		// Token: 0x06000053 RID: 83 RVA: 0x00003327 File Offset: 0x00002327
		private void OnEvaluationStarted(object sender, EventArgs e)
		{
			DocumentContext.EvaluationInProgress = true;
			TraceData.UpdateSaveState();
			TraceData.LoadFromDocument(sender as HomeWorkspaceModel);
			LifecycleManager<string>.Instance.UpdateAllObjectsForEvaluation();
			AcDynamoRuntime.OnDynamoModelEvaluationStarted();
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00003350 File Offset: 0x00002350
		private void OnRefreshCompleted(object sender, EvaluationCompletedEventArgs e)
		{
			ElementBinder.ClearInvalidHandlesBeforeFirstRun();
			this.IsHomeWorkspaceFirstRun = false;
			TraceData.SaveToDocument(sender as HomeWorkspaceModel);
			DynamicUpdateLogic instance = DynamicUpdateLogic.Instance;
			instance.ForceDisabled = true;
			DocumentContext.OnModelRefreshCompleted();
			instance.ForceDisabled = false;
			RuntimeCore liveRunnerRuntimeCore = base.EngineController.LiveRunnerRuntimeCore;
			IEnumerable<WarningEntry> warnings = base.EngineController.LiveRunnerRuntimeCore.RuntimeStatus.Warnings;
			List<string> list = new List<string>();
			foreach (WarningEntry warningEntry in warnings)
			{
				list.Add(warningEntry.Message);
			}
			AcDynamoRuntime.OnDynamoModelRefreshCompleted(list);
			Document startupDocument = DocumentContext.StartupDocument;
			if (startupDocument != null && startupDocument == DocumentContext.MdiActiveDocument)
			{
				startupDocument.Editor.UpdateScreen();
			}
			DocumentContext.EvaluationInProgress = false;
		}

		// Token: 0x0400001B RID: 27
		private bool setupPython;

		// Token: 0x0400001C RID: 28
		private bool IsHomeWorkspaceFirstRun;

		// Token: 0x02000010 RID: 16
		public enum DocumentEventType
		{
			// Token: 0x04000037 RID: 55
			Available,
			// Token: 0x04000038 RID: 56
			UnAvailable,
			// Token: 0x04000039 RID: 57
			Changed,
			// Token: 0x0400003A RID: 58
			Destroyed
		}
	}
}
