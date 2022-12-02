using System;
using System.Linq;
using HostMgd.ApplicationServices;
using Teigha.DatabaseServices;
using nanoCAD.DynamoApp.Services;
using Dynamo.Scheduler;

namespace nanoCAD.DynamoApp
{
	// Token: 0x02000008 RID: 8
	internal class SchedulerThread : ISchedulerThread
	{
		// Token: 0x06000055 RID: 85 RVA: 0x00003434 File Offset: 0x00002434
		public void Initialize(IScheduler owningScheduler)
		{
			this.scheduler = owningScheduler;
			
			//Application.Idle += this.Application_Idle;
		}

		// Token: 0x06000056 RID: 86 RVA: 0x0000344E File Offset: 0x0000244E
		private void Application_Idle(object sender, EventArgs e)
		{
			if (DocumentContext.StartupDocument == null && DocumentContext.MdiActiveDocument == null)
			{
				this.ExecuteInApplicationContextCallback(null);
				return;
			}
			Application.DocumentManager.ExecuteInApplicationContext(new ExecuteInApplicationContextCallback(this.ExecuteInApplicationContextCallback), null);
		}

		// Token: 0x06000057 RID: 87 RVA: 0x0000348C File Offset: 0x0000248C
		public void ExecuteInApplicationContextCallback(object userData)
		{
			try
			{
				if (!this.scheduler.Tasks.Any<AsyncTask>())
				{
					Document mdiActiveDocument = Application.DocumentManager.MdiActiveDocument;
					if (string.IsNullOrWhiteSpace((mdiActiveDocument != null) ? mdiActiveDocument.CommandInProgress : null) && AcDynamoRuntime.DynamoStarted && AcDynamoRuntime.DynamoModel != null)
					{
						DynamicUpdateLogic.Instance.CheckAndRun(AcDynamoRuntime.DynamoModel);
					}
				}
			}
			catch
			{
			}
			while (this.scheduler.ProcessNextTask(false))
			{
			}
		}

		// Token: 0x06000058 RID: 88 RVA: 0x00003508 File Offset: 0x00002508
		public void Shutdown()
		{
			//Application.Idle -= this.Application_Idle;
		}

		// Token: 0x0400001D RID: 29
		private IScheduler scheduler;
	}
}
