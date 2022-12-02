using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HostMgd.ApplicationServices;
using Teigha.DatabaseServices;
using nanoCAD.DynamoApp.Services;
using Dynamo.Models;
using Dynamo.ViewModels;

namespace nanoCAD.DynamoApp
{
	// Token: 0x02000005 RID: 5
	public static class AcDynamoRuntime
	{
		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000019 RID: 25 RVA: 0x00002773 File Offset: 0x00001773
		// (set) Token: 0x0600001A RID: 26 RVA: 0x0000277A File Offset: 0x0000177A
		public static DynamoModel DynamoModel { get; set; }

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x0600001B RID: 27 RVA: 0x00002782 File Offset: 0x00001782
		// (set) Token: 0x0600001C RID: 28 RVA: 0x00002789 File Offset: 0x00001789
		public static DynamoViewModel ViewModel { get; set; }

		// Token: 0x14000001 RID: 1
		// (add) Token: 0x0600001D RID: 29 RVA: 0x00002794 File Offset: 0x00001794
		// (remove) Token: 0x0600001E RID: 30 RVA: 0x000027C8 File Offset: 0x000017C8
		internal static event Action DynamoModelEvaluationStarted;

		// Token: 0x14000002 RID: 2
		// (add) Token: 0x0600001F RID: 31 RVA: 0x000027FC File Offset: 0x000017FC
		// (remove) Token: 0x06000020 RID: 32 RVA: 0x00002830 File Offset: 0x00001830
		internal static event Action<IList<string>> DynamoModelRefreshCompleted;

		// Token: 0x14000003 RID: 3
		// (add) Token: 0x06000021 RID: 33 RVA: 0x00002864 File Offset: 0x00001864
		// (remove) Token: 0x06000022 RID: 34 RVA: 0x00002898 File Offset: 0x00001898
		internal static event Action DynamoModelShutdown;

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000023 RID: 35 RVA: 0x000028CB File Offset: 0x000018CB
		// (set) Token: 0x06000024 RID: 36 RVA: 0x000028D2 File Offset: 0x000018D2
		public static AcDynamoRuntime.DynamoModelState DynamoState { get; set; }

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000025 RID: 37 RVA: 0x000028DA File Offset: 0x000018DA
		public static bool DynamoStarted
		{
			get
			{
				return AcDynamoRuntime.DynamoState > AcDynamoRuntime.DynamoModelState.NotStarted;
			}
		}

		// Token: 0x06000026 RID: 38 RVA: 0x000028E4 File Offset: 0x000018E4
		public static void Initialize()
		{
			if (AcDynamoRuntime.IsInitialized)
			{
				return;
			}
			AcDynamoRuntime.IsInitialized = true;
			AcDynamoRuntime.DynamoState = AcDynamoRuntime.DynamoModelState.NotStarted;
			AppDomain.CurrentDomain.AssemblyResolve += AcDynamoRuntime.ResolveAssembly;
			string text = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
			if (!text.Contains(AcDynamoRuntime.DynamoCorePath))
			{
				text = AcDynamoRuntime.DynamoCorePath + ";" + text;
				Environment.SetEnvironmentVariable("PATH", text, EnvironmentVariableTarget.Process);
			}
			AcDynamoRuntime.AddResourceStreamLoader(() => new KeyValuePair<string, Stream>("/icons/Category.AutoCAD.svg", Assembly.GetExecutingAssembly().GetManifestResourceStream("nanoCAD.DynamoApp.Resources.Category.AutoCAD.svg")));
			AcDynamoRuntime.AddLayoutSpecLoader(() => Assembly.GetExecutingAssembly().GetManifestResourceStream("nanoCAD.DynamoApp.Resources.LayoutSpecs.json"));
			AcDynamoRuntime.AddObjectDescriptorsRegister("nanoCAD.DynamoNodes.AutoCADObjectDescriptors, AutoCADNodes");
		}

		// Token: 0x06000027 RID: 39 RVA: 0x000029A3 File Offset: 0x000019A3
		public static void Terminate()
		{
			if (!AcDynamoRuntime.IsInitialized)
			{
				return;
			}
			AppDomain.CurrentDomain.AssemblyResolve -= AcDynamoRuntime.ResolveAssembly;
			Action shutdownDynamoHandler = AcDynamoRuntime.ShutdownDynamoHandler;
			if (shutdownDynamoHandler != null)
			{
				shutdownDynamoHandler();
			}
			AcDynamoRuntime.IsInitialized = false;
		}

		// Token: 0x06000028 RID: 40 RVA: 0x000029D9 File Offset: 0x000019D9
		public static void AddResourceStreamLoader(Func<KeyValuePair<string, Stream>> loader)
		{
			if (loader != null)
			{
				AcDynamoRuntime.ResourceStreamLoaders.Add(loader);
			}
		}

		// Token: 0x06000029 RID: 41 RVA: 0x000029E9 File Offset: 0x000019E9
		public static void RemoveResourceStreamLoader(Func<KeyValuePair<string, Stream>> loader)
		{
			if (loader != null)
			{
				AcDynamoRuntime.ResourceStreamLoaders.Remove(loader);
			}
		}

		// Token: 0x0600002A RID: 42 RVA: 0x000029FA File Offset: 0x000019FA
		public static void AddLayoutSpecLoader(Func<Stream> loader)
		{
			if (loader != null)
			{
				AcDynamoRuntime.LayoutSpecLoaders.Add(loader);
			}
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00002A0A File Offset: 0x00001A0A
		public static void RemoveLayoutSpecLoader(Func<Stream> loader)
		{
			if (loader != null)
			{
				AcDynamoRuntime.LayoutSpecLoaders.Remove(loader);
			}
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00002A1B File Offset: 0x00001A1B
		public static void AddObjectDescriptorsRegister(string typeName)
		{
			ObjectDescriptors.AddRegister(typeName);
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600002D RID: 45 RVA: 0x00002A23 File Offset: 0x00001A23
		public static string Version
		{
			get
			{
				return typeof(DynamoModel).Assembly.GetName().Version.ToString();
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600002E RID: 46 RVA: 0x00002A43 File Offset: 0x00001A43
		public static string HostVersion
		{
			get
			{
				return Assembly.GetExecutingAssembly().GetName().Version.ToString();
			}
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00002A59 File Offset: 0x00001A59
		public static void SetHostName(string hostName)
		{
			AcDynamoRuntime.DynamoHostName = hostName;
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00002A61 File Offset: 0x00001A61
		public static void SetAppDataPaths(string userDataFolder, string commonDataFolder)
		{
			AcDynamoRuntime.DynamoUserDataFolder = userDataFolder;
			AcDynamoRuntime.DynamoCommonDataFolder = commonDataFolder;
		}

		// Token: 0x06000031 RID: 49 RVA: 0x00002A70 File Offset: 0x00001A70
		public static void WriteLine(string message)
		{
			Document mdiActiveDocument = Application.DocumentManager.MdiActiveDocument;
			if (mdiActiveDocument != null)
			{
				mdiActiveDocument.Editor.WriteMessage("\r\n" + message);
			}
		}

		// Token: 0x06000032 RID: 50 RVA: 0x00002AA7 File Offset: 0x00001AA7
		public static object StartDynamo(bool noUI)
		{
			AcDynamo.Instance.Start();
			if (!AcDynamoRuntime.DynamoStarted)
			{
				return null;
			}
			return AcDynamoRuntime.DynamoModel;
		}

		// Token: 0x06000033 RID: 51 RVA: 0x00002AC2 File Offset: 0x00001AC2
		public static void RunDynamoScript(string scriptFileName)
		{
			if (!AcDynamoRuntime.DynamoStarted)
			{
				AcDynamo.Instance.RunDynamoScript(scriptFileName);
			}
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00002AD6 File Offset: 0x00001AD6
		public static void RunPythonScript(string scriptFileName)
		{
			if (!AcDynamoRuntime.DynamoStarted)
			{
				AcDynamo.Instance.RunPythonScript(scriptFileName);
			}
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00002AEC File Offset: 0x00001AEC
		public static void RunDynamoAction(Action action, Action dynamoModelEvaluationStarted, Action<IList<string>> dynamoModelRefreshCompleted, Action dynamoModelShutdown)
		{
			if (dynamoModelEvaluationStarted != null)
			{
				AcDynamoRuntime.DynamoModelEvaluationStarted += dynamoModelEvaluationStarted;
			}
			if (dynamoModelRefreshCompleted != null)
			{
				AcDynamoRuntime.DynamoModelRefreshCompleted += dynamoModelRefreshCompleted;
			}
			Action cleanup = null;
			bool cleanupCalled = false;
			cleanup = delegate()
			{
				Action dynamoModelShutdown2 = dynamoModelShutdown;
				if (dynamoModelShutdown2 != null)
				{
					dynamoModelShutdown2();
				}
				if (dynamoModelEvaluationStarted != null)
				{
					AcDynamoRuntime.DynamoModelEvaluationStarted -= dynamoModelEvaluationStarted;
				}
				if (dynamoModelRefreshCompleted != null)
				{
					AcDynamoRuntime.DynamoModelRefreshCompleted -= dynamoModelRefreshCompleted;
				}
				AcDynamoRuntime.DynamoModelShutdown -= cleanup;
				cleanupCalled = true;
			};
			AcDynamoRuntime.DynamoModelShutdown += cleanup;
			try
			{
				action();
			}
			catch (Exception ex)
			{
				if (!cleanupCalled)
				{
					cleanup();
				}
				throw ex;
			}
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00002B94 File Offset: 0x00001B94
		private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
		{
			string text = string.Empty;
			string path = new AssemblyName(args.Name).Name + ".dll";
			Assembly result;
			try
			{
				text = Path.Combine(AcDynamoRuntime.DynamoCorePath, path);
				if (File.Exists(text))
				{
					result = Assembly.LoadFrom(text);
				}
				else
				{
					result = null;
				}
			}
			catch (Exception innerException)
			{
				throw new Exception(string.Format("The location of the assembly, {0} could not be resolved for loading.", text), innerException);
			}
			return result;
		}

		// Token: 0x06000037 RID: 55 RVA: 0x00002C08 File Offset: 0x00001C08
		internal static void OnDynamoModelEvaluationStarted()
		{
			Action dynamoModelEvaluationStarted = AcDynamoRuntime.DynamoModelEvaluationStarted;
			if (dynamoModelEvaluationStarted == null)
			{
				return;
			}
			dynamoModelEvaluationStarted();
		}

		// Token: 0x06000038 RID: 56 RVA: 0x00002C19 File Offset: 0x00001C19
		internal static void OnDynamoModelRefreshCompleted(IList<string> warnings)
		{
			Action<IList<string>> dynamoModelRefreshCompleted = AcDynamoRuntime.DynamoModelRefreshCompleted;
			if (dynamoModelRefreshCompleted == null)
			{
				return;
			}
			dynamoModelRefreshCompleted(warnings);
		}

		// Token: 0x06000039 RID: 57 RVA: 0x00002C2B File Offset: 0x00001C2B
		internal static void OnDynamoModelShutdown()
		{
			Action dynamoModelShutdown = AcDynamoRuntime.DynamoModelShutdown;
			if (dynamoModelShutdown == null)
			{
				return;
			}
			dynamoModelShutdown();
		}

		// Token: 0x04000009 RID: 9
		private static bool IsInitialized = false;

		// Token: 0x0400000C RID: 12
		internal static Action ShutdownDynamoHandler = null;

		// Token: 0x04000010 RID: 16
		internal static string DynamoHostName = "Dynamo AutoCAD";

		// Token: 0x04000011 RID: 17
		public static string DynamoUserDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dynamo", "Dynamo AutoCAD");

		// Token: 0x04000012 RID: 18
		public static string DynamoCommonDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Dynamo", "Dynamo AutoCAD");

		// Token: 0x04000014 RID: 20
		public static bool DynamoViewFocused = false;

		// Token: 0x04000015 RID: 21
		public static List<Func<KeyValuePair<string, Stream>>> ResourceStreamLoaders = new List<Func<KeyValuePair<string, Stream>>>();

		// Token: 0x04000016 RID: 22
		public static List<Func<Stream>> LayoutSpecLoaders = new List<Func<Stream>>();

		// Token: 0x04000017 RID: 23
		public static Func<object> AuthProvider = null;

		// Token: 0x04000018 RID: 24
		public static string DynamoCorePath = ProductLocator.GetDynamoCorePath();

		// Token: 0x04000019 RID: 25
		public static string AutoCADPath = ProductLocator.GetAutoCADPath();

		// Token: 0x0200000D RID: 13
		public enum DynamoModelState
		{
			// Token: 0x0400002B RID: 43
			NotStarted,
			// Token: 0x0400002C RID: 44
			StartedUIless,
			// Token: 0x0400002D RID: 45
			StartedUI
		}
	}
}
