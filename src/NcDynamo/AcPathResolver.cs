using System;
using System.Collections.Generic;
using System.IO;
using Dynamo.Interfaces;

namespace nanoCAD.DynamoApp
{
	// Token: 0x02000004 RID: 4
	internal class AcPathResolver : IPathResolver
	{
		// Token: 0x06000013 RID: 19 RVA: 0x00002614 File Offset: 0x00001614
		internal AcPathResolver(string userDataFolder, string commonDataFolder)
		{
			this.userDataRootFolder = userDataFolder;
			this.commonDataRootFolder = commonDataFolder;
			this.preloadLibraryPaths = new List<string>
			{
				"VMDataBridge.dll",
				"ProtoGeometry.dll",
				"DesignScriptBuiltin.dll",
				"DSCoreNodes.dll",
				"DSOffice.dll",
				"DSIronPython.dll",
				"FunctionObject.ds",
				"BuiltIn.ds",
				"DynamoConversions.dll",
				"DynamoUnits.dll",
				"Tessellation.dll",
				"Analysis.dll",
				"GeometryColor.dll"
			};
			this.additionalNodeDirectories = new List<string>();
			this.additionalResolutionPaths = new List<string>();
			string dynamoPath = ProductLocator.GetDynamoPath();
			string text = Path.Combine(dynamoPath, "nodes");
			if (Directory.Exists(text))
			{
				this.additionalNodeDirectories.Add(text);
			}
			string[] files = Directory.GetFiles(dynamoPath, "*Nodes.dll", SearchOption.TopDirectoryOnly);
			foreach (string item in files)
			{
				this.preloadLibraryPaths.Add(item);
			}
			this.additionalResolutionPaths.Add(dynamoPath);
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000014 RID: 20 RVA: 0x0000274B File Offset: 0x0000174B
		public IEnumerable<string> AdditionalNodeDirectories
		{
			get
			{
				return this.additionalNodeDirectories;
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000015 RID: 21 RVA: 0x00002753 File Offset: 0x00001753
		public IEnumerable<string> AdditionalResolutionPaths
		{
			get
			{
				return this.additionalResolutionPaths;
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000016 RID: 22 RVA: 0x0000275B File Offset: 0x0000175B
		public IEnumerable<string> PreloadedLibraryPaths
		{
			get
			{
				return this.preloadLibraryPaths;
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000017 RID: 23 RVA: 0x00002763 File Offset: 0x00001763
		public string UserDataRootFolder
		{
			get
			{
				return this.userDataRootFolder;
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000018 RID: 24 RVA: 0x0000276B File Offset: 0x0000176B
		public string CommonDataRootFolder
		{
			get
			{
				return this.commonDataRootFolder;
			}
		}

		// Token: 0x04000004 RID: 4
		private readonly List<string> preloadLibraryPaths;

		// Token: 0x04000005 RID: 5
		private readonly List<string> additionalNodeDirectories;

		// Token: 0x04000006 RID: 6
		private readonly List<string> additionalResolutionPaths;

		// Token: 0x04000007 RID: 7
		private readonly string userDataRootFolder;

		// Token: 0x04000008 RID: 8
		private readonly string commonDataRootFolder;
	}
}
