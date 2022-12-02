using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace nanoCAD.DynamoApp.Resources
{
	// Token: 0x0200000B RID: 11
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resources
	{
		// Token: 0x0600005F RID: 95 RVA: 0x000020FB File Offset: 0x000010FB
		internal Resources()
		{
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000060 RID: 96 RVA: 0x000036DC File Offset: 0x000026DC
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (Resources.resourceMan == null)
				{
					ResourceManager resourceManager = new ResourceManager("nanoCAD.DynamoApp.Resources.Resources", typeof(Resources).Assembly);
					Resources.resourceMan = resourceManager;
				}
				return Resources.resourceMan;
			}
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000061 RID: 97 RVA: 0x00003715 File Offset: 0x00002715
		// (set) Token: 0x06000062 RID: 98 RVA: 0x0000371C File Offset: 0x0000271C
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000063 RID: 99 RVA: 0x00003724 File Offset: 0x00002724
		internal static string DYNAMO_VIEW_NOTIFICATION_MESSAGE_ACTIVE_DOCUMENT_CLOSED
		{
			get
			{
				return Resources.ResourceManager.GetString("DYNAMO_VIEW_NOTIFICATION_MESSAGE_ACTIVE_DOCUMENT_CLOSED", Resources.resourceCulture);
			}
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000064 RID: 100 RVA: 0x0000373A File Offset: 0x0000273A
		internal static string DYNAMO_VIEW_NOTIFICATION_MESSAGE_DOCUMENT_CHANGED
		{
			get
			{
				return Resources.ResourceManager.GetString("DYNAMO_VIEW_NOTIFICATION_MESSAGE_DOCUMENT_CHANGED", Resources.resourceCulture);
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000065 RID: 101 RVA: 0x00003750 File Offset: 0x00002750
		internal static string DYNAMO_VIEW_NOTIFICATION_MESSAGE_DYNAMO_AVAILABLE
		{
			get
			{
				return Resources.ResourceManager.GetString("DYNAMO_VIEW_NOTIFICATION_MESSAGE_DYNAMO_AVAILABLE", Resources.resourceCulture);
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000066 RID: 102 RVA: 0x00003766 File Offset: 0x00002766
		internal static string DYNAMO_VIEW_NOTIFICATION_MESSAGE_DYNAMO_NOT_POINTING_AT_CURRENT_DOCUMENT
		{
			get
			{
				return Resources.ResourceManager.GetString("DYNAMO_VIEW_NOTIFICATION_MESSAGE_DYNAMO_NOT_POINTING_AT_CURRENT_DOCUMENT", Resources.resourceCulture);
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000067 RID: 103 RVA: 0x0000377C File Offset: 0x0000277C
		internal static string EXCEPTION_MESSAGE_DYNAMO_MODEL_NEEDED
		{
			get
			{
				return Resources.ResourceManager.GetString("EXCEPTION_MESSAGE_DYNAMO_MODEL_NEEDED", Resources.resourceCulture);
			}
		}

		// Token: 0x04000027 RID: 39
		private static ResourceManager resourceMan;

		// Token: 0x04000028 RID: 40
		private static CultureInfo resourceCulture;
	}
}
