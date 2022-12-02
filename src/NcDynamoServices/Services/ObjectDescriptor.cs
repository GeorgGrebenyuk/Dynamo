using System;

using Teigha.Runtime;
using Teigha.DatabaseServices;

namespace nanoCAD.DynamoApp.Services
{
	// Token: 0x0200000A RID: 10
	public class ObjectDescriptor
	{
		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000054 RID: 84 RVA: 0x000030F8 File Offset: 0x000020F8
		// (set) Token: 0x06000055 RID: 85 RVA: 0x00003100 File Offset: 0x00002100
		public string DisplayName { get; set; }

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000056 RID: 86 RVA: 0x00003109 File Offset: 0x00002109
		// (set) Token: 0x06000057 RID: 87 RVA: 0x00003111 File Offset: 0x00002111
		public Type Type { get; set; }

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000058 RID: 88 RVA: 0x0000311A File Offset: 0x0000211A
		// (set) Token: 0x06000059 RID: 89 RVA: 0x00003122 File Offset: 0x00002122
		public RXClass RXClass { get; set; }

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x0600005A RID: 90 RVA: 0x0000312B File Offset: 0x0000212B
		// (set) Token: 0x0600005B RID: 91 RVA: 0x00003133 File Offset: 0x00002133
		public Func<Entity, bool, object> DynamoCreator { get; set; }

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x0600005C RID: 92 RVA: 0x0000313C File Offset: 0x0000213C
		// (set) Token: 0x0600005D RID: 93 RVA: 0x00003144 File Offset: 0x00002144
		public bool ShowInDropDown { get; set; }
	}
}
