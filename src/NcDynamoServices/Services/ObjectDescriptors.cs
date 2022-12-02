using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Teigha.Runtime;
using Teigha.DatabaseServices;

namespace nanoCAD.DynamoApp.Services
{
	// Token: 0x0200000D RID: 13
	public static class ObjectDescriptors
	{
		// Token: 0x06000062 RID: 98 RVA: 0x000031AD File Offset: 0x000021AD
		public static void AddRegister(string typeName)
		{
			ObjectDescriptors.AllRegisters.Add(typeName);
		}

		// Token: 0x06000063 RID: 99 RVA: 0x000031BC File Offset: 0x000021BC
		private static void ReloadAllObjectDescriptors(bool force)
		{
			if (ObjectDescriptors.AllObjectDesciptors.Count == 0 || force)
			{
				ObjectDescriptors.AllObjectDesciptors.Clear();
				Type typeFromHandle = typeof(IObjectDescriptorsRegister);
				foreach (string typeName in ObjectDescriptors.AllRegisters)
				{
					try
					{
						Type type = Type.GetType(typeName);
						if (type != null && typeFromHandle.IsAssignableFrom(type))
						{
							MethodInfo method = type.GetMethod("RegisterDescriptors");
							if (method != null)
							{
								ConstructorInfo constructor = type.GetConstructor(new Type[0]);
								if (constructor != null)
								{
									object obj = constructor.Invoke(new object[0]);
									if (obj != null)
									{
										method.Invoke(obj, new object[0]);
									}
								}
							}
						}
					}
					catch
					{
					}
				}
			}
		}

		// Token: 0x06000064 RID: 100 RVA: 0x000032A8 File Offset: 0x000022A8
		public static void RegisterObjectDescriptor(string displayName, Type type, Func<Entity, bool, object> dynamoCreator, bool showInDropDown = true)
		{
			ObjectDescriptors.AllObjectDesciptors[type.Name] = new ObjectDescriptor
			{
				DisplayName = displayName,
				Type = type,
				RXClass = RXObject.GetClass(type),
				DynamoCreator = dynamoCreator,
				ShowInDropDown = showInDropDown
			};
		}

		// Token: 0x06000065 RID: 101 RVA: 0x000032E7 File Offset: 0x000022E7
		public static IEnumerable<KeyValuePair<string, ObjectDescriptor>> GetAllObjectDescriptors()
		{
			if (ObjectDescriptors.AllObjectDesciptors.Count == 0)
			{
				ObjectDescriptors.ReloadAllObjectDescriptors(false);
			}
			return ObjectDescriptors.AllObjectDesciptors.ToList<KeyValuePair<string, ObjectDescriptor>>();
		}

		// Token: 0x06000066 RID: 102 RVA: 0x00003305 File Offset: 0x00002305
		public static ObjectDescriptor GetDescriptorByType(string objectType)
		{
			if (string.IsNullOrWhiteSpace(objectType))
			{
				return null;
			}
			ObjectDescriptors.ReloadAllObjectDescriptors(false);
			if (!ObjectDescriptors.AllObjectDesciptors.ContainsKey(objectType))
			{
				return null;
			}
			return ObjectDescriptors.AllObjectDesciptors[objectType];
		}

		// Token: 0x06000067 RID: 103 RVA: 0x00003334 File Offset: 0x00002334
		public static ObjectDescriptor GetDescriptorByRXClass(RXClass cls)
		{
			ObjectDescriptors.ReloadAllObjectDescriptors(false);
			ObjectDescriptor objectDescriptor = null;
			foreach (KeyValuePair<string, ObjectDescriptor> keyValuePair in ObjectDescriptors.AllObjectDesciptors)
			{
				if (cls.Equals(keyValuePair.Value.RXClass))
				{
					objectDescriptor = keyValuePair.Value;
					break;
				}
			}
			if (objectDescriptor == null)
			{
				List<ObjectDescriptor> list = new List<ObjectDescriptor>();
				foreach (KeyValuePair<string, ObjectDescriptor> keyValuePair2 in ObjectDescriptors.AllObjectDesciptors)
				{
					if (cls.IsDerivedFrom(keyValuePair2.Value.RXClass))
					{
						list.Add(keyValuePair2.Value);
					}
				}
				list.Sort(new ObjectDescriptorComparerByRXClass());
				objectDescriptor = list.LastOrDefault<ObjectDescriptor>();
			}
			return objectDescriptor;
		}

		// Token: 0x06000068 RID: 104 RVA: 0x00003418 File Offset: 0x00002418
		public static ObjectDescriptor GetDescriptorById(ObjectId id)
		{
			return ObjectDescriptors.GetDescriptorByRXClass(id.ObjectClass);
		}

		// Token: 0x0400002C RID: 44
		private static IDictionary<string, ObjectDescriptor> AllObjectDesciptors = new Dictionary<string, ObjectDescriptor>();

		// Token: 0x0400002D RID: 45
		private static IList<string> AllRegisters = new List<string>();
	}
}
