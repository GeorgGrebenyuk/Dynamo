using System;

namespace nanoCAD.DynamoApp.Services
{
	// Token: 0x02000009 RID: 9
	public class DisposeLogic
	{
		// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600004C RID: 76 RVA: 0x000030B4 File Offset: 0x000020B4
		// (set) Token: 0x0600004D RID: 77 RVA: 0x000030BB File Offset: 0x000020BB
		public static bool IsShuttingDown { get; set; }

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x0600004E RID: 78 RVA: 0x000030C3 File Offset: 0x000020C3
		// (set) Token: 0x0600004F RID: 79 RVA: 0x000030CA File Offset: 0x000020CA
		public static bool IsClosingHomeworkspace { get; set; }

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000050 RID: 80 RVA: 0x000030D2 File Offset: 0x000020D2
		// (set) Token: 0x06000051 RID: 81 RVA: 0x000030D9 File Offset: 0x000020D9
		public static bool IsDestroyingDocument { get; set; }

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000052 RID: 82 RVA: 0x000030E1 File Offset: 0x000020E1
		public static bool DisableDispose
		{
			get
			{
				return DisposeLogic.IsShuttingDown || DisposeLogic.IsClosingHomeworkspace || DisposeLogic.IsDestroyingDocument;
			}
		}
	}
}
