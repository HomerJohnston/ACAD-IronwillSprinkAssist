using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;

namespace Ironwill
{
	public static class EntityExtensions
	{
		public static void Draw(this Entity entity, WorldDraw draw)
		{
			draw.Geometry.Draw(entity);
		}

		public static bool GetXRecord<T>(this Entity entity, Transaction transaction, string RecordName, ref T outData)
		{
			try
			{
				ObjectId extensionDictionaryId = entity.ExtensionDictionary;

				if (extensionDictionaryId == ObjectId.Null)
				{
					return false;
				}

				DBDictionary entityExtensionDictionary = transaction.GetObject(extensionDictionaryId, OpenMode.ForWrite) as DBDictionary;

				ObjectId recordId = entityExtensionDictionary.GetAt(RecordName);

				if (recordId == ObjectId.Null)
				{
					return false;
				}

				Xrecord xRecord = transaction.GetObject(recordId, OpenMode.ForRead) as Xrecord;

				if (xRecord == null)
				{
					return false;
				}

				Session.LogDebug("Reading xrecord data on xrecord id " + xRecord.ObjectId + " from entity handle " + entity.Handle + entity.GetType().Name);

				if (xRecord.Data == null)
				{
					return false;
				}

				TypedValue[] dataArray = xRecord.Data.AsArray();

				if (dataArray.Length == 0)
				{
					Session.Log("Tried to read XRecord " + RecordName + "but entity had no data!");
					return false;
				}

				if (dataArray.Length > 1)
				{
					Session.Log("Tried to read XRecord " + RecordName + "but entity had multiple data!");
					return false;
				}

				TypedValue typedValue = dataArray[0];

				switch (outData)
				{
					case string s:
						{
							outData = (T)typedValue.Value;
							return true;
						}
					case int i:
						{
							outData = (T)typedValue.Value;
							return true;
						}
					case double d:
						{
							outData = (T)typedValue.Value;
							return true;
						}
					case bool b:
						{
							outData = (T)Convert.ChangeType(typedValue.Value, typeof(T));
							return true;
						}
					case Handle handle:
						{
							long val = Convert.ToInt64((string)typedValue.Value, 16);
							outData = (T)Convert.ChangeType(new Handle(val), typeof(T));
							return true;
						}
					default:
						{
							Session.Log("Unhandled data type! Nothing read.");
							return false;
						}
				}
			}
			catch
			{
				return false;
			}
		}

		public static void SetXRecord<T>(this Entity entity, Transaction transaction, string RecordName, T data)
		{
			entity.UpgradeOpen();

			if (entity.ExtensionDictionary == ObjectId.Null)
			{
				entity.CreateExtensionDictionary();
			}

			DBDictionary entityExtensionDictionary = transaction.GetObject(entity.ExtensionDictionary, OpenMode.ForWrite) as DBDictionary;

			ResultBuffer buffer = new ResultBuffer();

			switch (data)
			{
				case int asInt:
					{
						buffer.Add(new TypedValue((int)DxfCode.Int32, asInt));
						break;
					}
				case Entity asEntity:
					{
						buffer.Add(new TypedValue((int)DxfCode.Handle, asEntity.Handle));
						break;
					}
				case Handle asHandle:
					{
						buffer.Add(new TypedValue((int)DxfCode.Handle, asHandle));
						break;
					}
				default:
					{
						Session.Log("Failed to set xrecord - invalid type " + data.GetType());
						break;
					}
			}

			Xrecord xRecord;

			if (entityExtensionDictionary.Contains(RecordName))
			{
				xRecord = transaction.GetObject(entityExtensionDictionary.GetAt(RecordName), OpenMode.ForWrite) as Xrecord;
			}
			else
			{
				xRecord = new Xrecord();
				entityExtensionDictionary.SetAt(RecordName, xRecord);
				transaction.AddNewlyCreatedDBObject(xRecord, true);
			}

			xRecord.Data = buffer;

			Session.LogDebug("Setting xrecord data on xrecord id " + xRecord.ObjectId + " from entity handle " + entity.Handle);
		}
	}
}
