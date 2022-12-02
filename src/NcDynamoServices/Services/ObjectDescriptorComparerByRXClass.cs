using System;
using System.Collections.Generic;

namespace nanoCAD.DynamoApp.Services
{
	// Token: 0x0200000B RID: 11
	internal class ObjectDescriptorComparerByRXClass : IComparer<ObjectDescriptor>
	{
		// Token: 0x0600005F RID: 95 RVA: 0x00003150 File Offset: 0x00002150
		public int Compare(ObjectDescriptor x, ObjectDescriptor y)
		{
			if (x.Equals(y))
			{
				return 0;
			}
			if (x.RXClass.IsDerivedFrom(y.RXClass))
			{
				return 1;
			}
			if (y.RXClass.IsDerivedFrom(x.RXClass))
			{
				return -1;
			}
			return string.Compare(x.RXClass.Name, y.RXClass.Name);
		}
	}
}
