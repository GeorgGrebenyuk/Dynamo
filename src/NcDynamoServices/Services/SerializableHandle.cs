using System;
using System.Runtime.Serialization;

namespace nanoCAD.DynamoApp.Services
{
	// Token: 0x02000006 RID: 6
	[Serializable]
	public class SerializableHandle : ISerializable
	{
		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000026 RID: 38 RVA: 0x000027E4 File Offset: 0x000017E4
		// (set) Token: 0x06000027 RID: 39 RVA: 0x000027EC File Offset: 0x000017EC
		public string stringID { get; set; }

		// Token: 0x06000028 RID: 40 RVA: 0x000027F5 File Offset: 0x000017F5
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("stringID", this.stringID, typeof(string));
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00002812 File Offset: 0x00001812
		public SerializableHandle()
		{
			this.stringID = "";
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00002825 File Offset: 0x00001825
		public SerializableHandle(SerializationInfo info, StreamingContext context)
		{
			this.stringID = (string)info.GetValue("stringID", typeof(string));
		}
	}
}
