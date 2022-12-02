using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using HostMgd.ApplicationServices;
using Teigha.DatabaseServices;
using nanoCAD.DynamoApp.Services;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.Interfaces;
using Dynamo.Wpf.ViewModels.Watch3D;
using Greg;
using PythonNodeModels;
using Teigha.Runtime;
namespace nanoCAD.DynamoApp
{
	// Token: 0x02000003 RID: 3
	public class AcDynamo
	{
		// Token: 0x06000004 RID: 4 RVA: 0x000020FB File Offset: 0x000010FB
		private AcDynamo()
		{
		}

		// Token: 0x06000005 RID: 5 RVA: 0x00002103 File Offset: 0x00001103
		static AcDynamo()
		{
			AcDynamo.Instance = new AcDynamo();
		}

		// Token: 0x06000006 RID: 6
		[DllImport("msvcrt.dll")]
		public static extern int _putenv(string env);

		// Token: 0x06000007 RID: 7 RVA: 0x00002120 File Offset: 0x00001120
		private void Initialize()
		{
			if (!AcDynamo.Initialized)
			{
				AcDynamo.Initialized = true;
				StringBuilder stringBuilder = new StringBuilder("LANGUAGE=");
				string text = CultureInfo.CurrentUICulture.ToString();
				stringBuilder.Append(text.Replace("-", "_"));
				AcDynamo._putenv(stringBuilder.ToString());
				AcDynamo.GeometryFactoryPath = GeometryFactoryPathDetector.GetGeometryFactoryPath();
			}
		}

		// Token: 0x06000008 RID: 8 RVA: 0x00002180 File Offset: 0x00001180
		public void CreateDynamo(bool noUI = false)
		{
			this.Initialize();
			AcDynamoRuntime.DynamoModel = this.CreateCoreModel(noUI);
			AcDynamoRuntime.DynamoModel.HostName = AcDynamoRuntime.DynamoHostName;
			AcDynamoRuntime.DynamoModel.HostVersion = AcDynamoRuntime.HostVersion;
			AcDynamoRuntime.ViewModel = this.CreateCoreViewModel(AcDynamoRuntime.DynamoModel);
			AcDynamoRuntime.DynamoModel.ShutdownCompleted += new DynamoModelHandler(this.DynamoModel_ShutdownCompleted);
			DocumentContext.StartupDocument = DocumentContext.MdiActiveDocument;
		}

		// Token: 0x06000009 RID: 9 RVA: 0x000021ED File Offset: 0x000011ED
		private void DynamoModel_ShutdownCompleted(DynamoModel model)
		{
			AcDynamoRuntime.DynamoModel.ShutdownCompleted -= new DynamoModelHandler(this.DynamoModel_ShutdownCompleted);
			AcDynamoRuntime.DynamoState = AcDynamoRuntime.DynamoModelState.NotStarted;
		}

		// Token: 0x0600000A RID: 10 RVA: 0x0000220C File Offset: 0x0000120C
		public void RunDynamoScript(string dynPath)
		{
			this.CreateDynamo(true);
			DynamoModel dynamoModel = AcDynamoRuntime.DynamoModel;
			if (dynamoModel != null)
			{
				dynamoModel.OpenFileFromPath(dynPath, true);
				dynamoModel.ExecuteCommand(new DynamoModel.ForceRunCancelCommand(false, false));
				dynamoModel.ShutDown(true);
			}
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002248 File Offset: 0x00001248
		public void RunPythonScript(string pyPath)
		{
			using (StreamReader streamReader = new StreamReader(pyPath))
			{
				string text = streamReader.ReadToEnd();
				if (!string.IsNullOrWhiteSpace(text))
				{
					this.CreateDynamo(true);
					DynamoModel dynamoModel = AcDynamoRuntime.DynamoModel;
					if (dynamoModel != null)
					{
						dynamoModel.CurrentWorkspace = dynamoModel.Workspaces.FirstOrDefault<WorkspaceModel>();
						dynamoModel.ClearCurrentWorkspace();
						HomeWorkspaceModel homeWorkspaceModel = dynamoModel.CurrentWorkspace as HomeWorkspaceModel;
						if (homeWorkspaceModel != null)
						{
							homeWorkspaceModel.RunSettings.RunType = 0;
							dynamoModel.ExecuteCommand(new DynamoModel.CreateNodeCommand(Guid.NewGuid().ToString(),
								"PythonNodeModels.PythonNode", -1.0, -1.0, true, false));
							PythonNode pythonNode = homeWorkspaceModel.Nodes.FirstOrDefault<NodeModel>() as PythonNode;
							if (pythonNode != null)
							{
								pythonNode.Script = text;
								dynamoModel.ExecuteCommand(new DynamoModel.ForceRunCancelCommand(false, false));
								dynamoModel.ShutDown(true);
							}
						}
					}
				}
			}
		}


		[CommandMethod("_run_dynamo2")]
		public void Start2()
        {
			Start();

		}
		[CommandMethod("_run_dynamo3")]
		public void Start3()
		{
			
		}
		[CommandMethod("_run_dynamo1")]
		public void Start()
		{
			bool noUI = true;
			if (!AcDynamoRuntime.DynamoStarted)
			{
				this.CreateDynamo(noUI);
			}
			if (!noUI)
			{
				if (AcDynamoRuntime.DynamoState != AcDynamoRuntime.DynamoModelState.StartedUI)
				{
					HostMgd.ApplicationServices.Application.ShowModelessWindow(this.CreateCoreView(AcDynamoRuntime.ViewModel));
					AcDynamoRuntime.DynamoState = AcDynamoRuntime.DynamoModelState.StartedUI;
					return;
				}
			}
			else
			{
				AcDynamoRuntime.DynamoState = AcDynamoRuntime.DynamoModelState.StartedUIless;
			}
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002374 File Offset: 0x00001374
		private DynamoModel CreateCoreModel(bool noUI)
		{
			DynamoModel.DefaultStartConfiguration defaultStartConfiguration = default(DynamoModel.DefaultStartConfiguration);
			defaultStartConfiguration.GeometryFactoryPath = AcDynamo.GeometryFactoryPath;
			defaultStartConfiguration.DynamoCorePath = AcDynamoRuntime.DynamoCorePath;
			defaultStartConfiguration.PythonTemplatePath = Path.Combine(AcDynamoRuntime.DynamoCommonDataFolder, "PythonTemplate.py");
			defaultStartConfiguration.SchedulerThread = new SchedulerThread();
			defaultStartConfiguration.PathResolver = new AcPathResolver(AcDynamoRuntime.DynamoUserDataFolder,
				AcDynamoRuntime.DynamoCommonDataFolder);
			defaultStartConfiguration.ProcessMode = ((Dynamo.Scheduler.TaskProcessMode)(noUI ? 0 : 1));
			Func<object> authProvider = AcDynamoRuntime.AuthProvider;
			defaultStartConfiguration.AuthProvider = (((authProvider != null) ? authProvider() : null) as IAuthProvider);
			return AcDynamoModel.Start(defaultStartConfiguration);
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002410 File Offset: 0x00001410
		private DynamoViewModel CreateCoreViewModel(DynamoModel model)
		{
			DynamoViewModel.StartConfiguration startConfiguration = default(DynamoViewModel.StartConfiguration);
			startConfiguration.DynamoModel = model;
			startConfiguration.Watch3DViewModel = HelixWatch3DViewModel.TryCreateHelixWatch3DViewModel(null, new Watch3DViewModelStartupParams(model), model.Logger);
			return AcDynamoViewModel.Start(startConfiguration);
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002450 File Offset: 0x00001450
		public DynamoView CreateCoreView(DynamoViewModel viewModel)
		{
			DynamoView dynamoView = new DynamoView(viewModel);
			IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
			new WindowInteropHelper(dynamoView).Owner = mainWindowHandle;
			dynamoView.Loaded += this.DynamoView_Loaded;
			dynamoView.Activated += this.DynamoView_Activated;
			dynamoView.Deactivated += this.DynamoView_Deactivated;
			return dynamoView;
		}

		// Token: 0x06000010 RID: 16 RVA: 0x000024B2 File Offset: 0x000014B2
		private void DynamoView_Activated(object sender, EventArgs e)
		{
			AcDynamoRuntime.DynamoViewFocused = true;
		}

		// Token: 0x06000011 RID: 17 RVA: 0x000024BA File Offset: 0x000014BA
		private void DynamoView_Deactivated(object sender, EventArgs e)
		{
			AcDynamoRuntime.DynamoViewFocused = false;
		}

		// Token: 0x06000012 RID: 18 RVA: 0x000024C4 File Offset: 0x000014C4
		private void DynamoView_Loaded(object sender, RoutedEventArgs e)
		{
			ILibraryViewCustomization customization = AcDynamoRuntime.DynamoModel.ExtensionManager.Service<ILibraryViewCustomization>();
			if (customization == null)
			{
				return;
			}
			AcDynamoRuntime.ShutdownDynamoHandler = delegate()
			{
				customization.OnAppShutdown();
			};
			foreach (Func<KeyValuePair<string, Stream>> func in AcDynamoRuntime.ResourceStreamLoaders)
			{
				try
				{
					KeyValuePair<string, Stream> keyValuePair = func();
					customization.RegisterResourceStream(keyValuePair.Key, keyValuePair.Value);
				}
				catch
				{
				}
			}
			foreach (Func<Stream> func2 in AcDynamoRuntime.LayoutSpecLoaders)
			{
				try
				{
					LayoutSpecification layoutSpecification;
					using (Stream stream = func2())
					{
						layoutSpecification = LayoutSpecification.FromJSONStream(stream);
					}
					customization.AddElements(layoutSpecification.sections.First<LayoutSection>().childElements, "");
				}
				catch
				{
				}
			}
		}

		// Token: 0x04000001 RID: 1
		public static string GeometryFactoryPath = "";

		// Token: 0x04000002 RID: 2
		public static AcDynamo Instance;

		// Token: 0x04000003 RID: 3
		private static bool Initialized = false;
	}
}
