using System;
using HostMgd.ApplicationServices;
using Teigha.DatabaseServices;
using nanoCAD.DynamoApp.Services;
using Dynamo.Interfaces;
using Dynamo.ViewModels;
using Dynamo.Visualization;
using Dynamo.Wpf.ViewModels.Core;
using Dynamo.Wpf.ViewModels.Watch3D;
using nanoCAD.DynamoApp.Resources;

namespace nanoCAD.DynamoApp
{
	// Token: 0x02000009 RID: 9
	internal class AcDynamoViewModel : DynamoViewModel
	{
		// Token: 0x0600005A RID: 90 RVA: 0x0000351C File Offset: 0x0000251C
		private AcDynamoViewModel(DynamoViewModel.StartConfiguration startConfiguration) : base(startConfiguration)
		{
			AcDynamoModel acDynamoModel = (AcDynamoModel)base.Model;
			acDynamoModel.DocumentEvent += this.Model_DocumentEvent;
			Watch3DViewModelStartupParams watch3DViewModelStartupParams = new Watch3DViewModelStartupParams(acDynamoModel);
			DefaultWatch3DViewModel defaultWatch3DViewModel = new DefaultWatch3DViewModel(null, watch3DViewModelStartupParams);
			base.RegisterWatch3DViewModel(defaultWatch3DViewModel, new DefaultRenderPackageFactory());
		}

		// Token: 0x0600005B RID: 91 RVA: 0x0000356C File Offset: 0x0000256C
		public static AcDynamoViewModel Start(DynamoViewModel.StartConfiguration startConfiguration)
		{
			if (startConfiguration.DynamoModel == null)
			{
				startConfiguration.DynamoModel = AcDynamoModel.Start();
			}
			else if (startConfiguration.DynamoModel.GetType() != typeof(AcDynamoModel))
			{
				//throw new Exception(Resources.EXCEPTION_MESSAGE_DYNAMO_MODEL_NEEDED);
			}
			if (startConfiguration.Watch3DViewModel == null)
			{
				startConfiguration.Watch3DViewModel = HelixWatch3DViewModel.TryCreateHelixWatch3DViewModel(null, new Watch3DViewModelStartupParams(startConfiguration.DynamoModel), startConfiguration.DynamoModel.Logger);
			}
			if (startConfiguration.WatchHandler == null)
			{
				startConfiguration.WatchHandler = new DefaultWatchHandler(startConfiguration.DynamoModel.PreferenceSettings);
			}
			return new AcDynamoViewModel(startConfiguration);
		}

		// Token: 0x0600005C RID: 92 RVA: 0x00003610 File Offset: 0x00002610
		private void Model_DocumentEvent(AcDynamoModel.DocumentEventType eventType)
		{
			HomeWorkspaceViewModel homeWorkspaceViewModel = base.HomeSpaceViewModel as HomeWorkspaceViewModel;
			if (homeWorkspaceViewModel != null)
			{
				NotificationLevel currentNotificationLevel = NotificationLevel.Moderate;
				string currentNotificationMessage = "";
				switch (eventType)
				{
				case AcDynamoModel.DocumentEventType.Available:
					currentNotificationLevel = NotificationLevel.Moderate;
					//currentNotificationMessage = ""Resources.DYNAMO_VIEW_NOTIFICATION_MESSAGE_DYNAMO_AVAILABLE;
					break;
				case AcDynamoModel.DocumentEventType.UnAvailable:
					currentNotificationLevel = NotificationLevel.Error;
					//currentNotificationMessage = Resources.DYNAMO_VIEW_NOTIFICATION_MESSAGE_DYNAMO_NOT_POINTING_AT_CURRENT_DOCUMENT;
					break;
				case AcDynamoModel.DocumentEventType.Changed:
				{
					currentNotificationLevel = NotificationLevel.Moderate;
					Document mdiActiveDocument = DocumentContext.MdiActiveDocument;
					if (mdiActiveDocument != null)
					{
						//currentNotificationMessage = string.Format(Resources.DYNAMO_VIEW_NOTIFICATION_MESSAGE_DOCUMENT_CHANGED, DocumentContext.MdiActiveDocument.Name);
					}
					else
					{
						currentNotificationMessage = string.Empty;
					}
					break;
				}
				case AcDynamoModel.DocumentEventType.Destroyed:
					currentNotificationLevel = NotificationLevel.Error;
					//currentNotificationMessage = Resources.DYNAMO_VIEW_NOTIFICATION_MESSAGE_ACTIVE_DOCUMENT_CLOSED;
					break;
				}
				homeWorkspaceViewModel.CurrentNotificationLevel = currentNotificationLevel;
				homeWorkspaceViewModel.CurrentNotificationMessage = currentNotificationMessage;
			}
		}

		// Token: 0x0600005D RID: 93 RVA: 0x000036A8 File Offset: 0x000026A8
		protected override void UnsubscribeAllEvents()
		{
			AcDynamoModel acDynamoModel = (AcDynamoModel)base.Model;
			acDynamoModel.DocumentEvent -= this.Model_DocumentEvent;
			base.UnsubscribeAllEvents();
		}
	}
}
