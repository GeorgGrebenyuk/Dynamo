using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace nanoCAD.DynamoApp.Resources
{
	// Token: 0x02000010 RID: 16
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resources
	{
		// Token: 0x0600008B RID: 139 RVA: 0x00002048 File Offset: 0x00001048
		internal Resources()
		{
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x0600008C RID: 140 RVA: 0x000044E4 File Offset: 0x000034E4
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

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x0600008D RID: 141 RVA: 0x0000451D File Offset: 0x0000351D
		// (set) Token: 0x0600008E RID: 142 RVA: 0x00004524 File Offset: 0x00003524
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

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x0600008F RID: 143 RVA: 0x0000452C File Offset: 0x0000352C
		internal static string EXCEPTION_MESSAGE_DIFFERNT_DOCUMENT
		{
			get
			{
				return Resources.ResourceManager.GetString("EXCEPTION_MESSAGE_DIFFERNT_DOCUMENT", Resources.resourceCulture);
			}
		}

		// Token: 0x04000037 RID: 55
		private static ResourceManager resourceMan;

		// Token: 0x04000038 RID: 56
		private static CultureInfo resourceCulture;
	}
}
