using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DynamoShapeManager;

namespace nanoCAD.DynamoApp
{
	// Token: 0x02000002 RID: 2
	public class GeometryFactoryPathDetector
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002048 File Offset: 0x00001048
		internal static Version GetAcadASMVersion()
		{
			Version result = null;
			try
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(AcDynamoRuntime.AutoCADPath);
				if (directoryInfo.Exists)
				{
					FileInfo[] files = directoryInfo.GetFiles("ASMAHL*.dll");
					if (files.Any<FileInfo>())
					{
						FileInfo fileInfo = files.First<FileInfo>();
						FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(fileInfo.FullName);
						result = new Version(versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart);
					}
				}
			}
			catch
			{
			}
			return result;
		}

		// Token: 0x06000002 RID: 2 RVA: 0x000020C4 File Offset: 0x000010C4
		public static string GetGeometryFactoryPath()
		{
			Version acadASMVersion = GeometryFactoryPathDetector.GetAcadASMVersion();
			if (!(acadASMVersion != null))
			{
				return "";
			}
			return Path.Combine(Utilities.GetLibGPreloaderLocation(acadASMVersion, AcDynamoRuntime.DynamoCorePath), Utilities.GeometryFactoryAssembly);
		}
	}
}
