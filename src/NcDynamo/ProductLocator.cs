using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace nanoCAD.DynamoApp
{
	// Token: 0x02000006 RID: 6
	internal class ProductLocator
	{
		// Token: 0x0600003B RID: 59 RVA: 0x00002CC9 File Offset: 0x00001CC9
		public static string GetDynamoPath()
		{
			return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}

		// Token: 0x0600003C RID: 60 RVA: 0x00002CDA File Offset: 0x00001CDA
		public static string GetDynamoCorePath()
		{
			return Path.Combine(ProductLocator.GetDynamoPath(), "\\");
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00002CEB File Offset: 0x00001CEB
		public static string GetAutoCADPath()
		{
			return Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
		}
	}
}
