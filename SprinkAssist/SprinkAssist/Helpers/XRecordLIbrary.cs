using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill
{
	internal class XRecordLibrary
	{
		protected const string sprinkAssistDictionaryName = "SprinkAssist";

		// ==============================================================================================
		// DICTIONARY MANAGEMENT
		// ==============================================================================================
		public static DBDictionary GetNamedDictionary(string dictionaryName, DBDictionary parent = null)
		{
			if (parent == null)
			{
				parent = GetGlobalDictionary();
			}

			using (Transaction transaction = Session.StartTransaction())
			{
				try
				{
					return transaction.GetObject(parent.GetAt(dictionaryName), OpenMode.ForRead) as DBDictionary;
				}
				catch
				{
					DBDictionary parentMutable = transaction.GetObject(parent.ObjectId, OpenMode.ForWrite) as DBDictionary;

					DBDictionary namedDictionary = new DBDictionary();

					parentMutable.SetAt(dictionaryName, namedDictionary);
					transaction.AddNewlyCreatedDBObject(namedDictionary, true);

					transaction.Commit();

					return namedDictionary;
				}
			}
		}

		public static DBDictionary GetGlobalDictionary()
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				ObjectId masterDictionaryId = Session.GetDatabase().NamedObjectsDictionaryId;
				DBDictionary masterDictionary = transaction.GetObject(masterDictionaryId, OpenMode.ForRead) as DBDictionary;

				try
				{
					ObjectId PluginDictionaryId = masterDictionary.GetAt(sprinkAssistDictionaryName);
					return transaction.GetObject(PluginDictionaryId, OpenMode.ForRead) as DBDictionary;
				}
				catch
				{
					DBDictionary pluginDictionary = new DBDictionary();
					
					masterDictionary.UpgradeOpen();
					masterDictionary.SetAt(sprinkAssistDictionaryName, pluginDictionary);
					transaction.AddNewlyCreatedDBObject(pluginDictionary, true);
					masterDictionary.DowngradeOpen();

					transaction.Commit();

					return pluginDictionary;
				}
			}
		}

		public static DBDictionary GetCommandSettingsDictionary()
		{
			return GetNamedDictionary("CommandSettings");
		}

		public static DBDictionary GetCommandDictionaryForClass(Type command)
		{
			DBDictionary commandSettingsDictionary = GetCommandSettingsDictionary();

			string commandName = command.Name;

			return GetNamedDictionary(commandName, commandSettingsDictionary);
		}

		public static bool DestroyNamedDictionary(string dictionaryName, DBDictionary parent = null)
		{
			if (parent == null)
			{
				parent = GetGlobalDictionary();
			}

			using (Transaction transaction = Session.StartTransaction())
			{
				if (parent.Remove(dictionaryName) != ObjectId.Null)
				{
					transaction.Commit();
					return true;
				}

				return false;
			}
		}

		// ==============================================================================================
		// GETTING DATA
		// ==============================================================================================
		public static object ReadXRecordData(DBDictionary dictionary, string xrecordName)
		{
			using (Session.LockDocument())
			{
				using (Transaction transaction = Session.StartTransaction())
				{
					Xrecord xrecord = GetXRecord(transaction, dictionary, xrecordName);

					TypedValue typedValue = xrecord.Data.AsArray()[0];
					return typedValue.Value;
				}
			}
		}

		static Xrecord GetXRecord(Transaction transaction, DBDictionary dictionary, string name)
		{
			try
			{
				ObjectId xrecordId = dictionary.GetAt(name);
				return transaction.GetObject(xrecordId, OpenMode.ForRead) as Xrecord;
			}
			catch
			{
				Xrecord xrecord = new Xrecord();

				dictionary.SetAt(name, xrecord);
				transaction.AddNewlyCreatedDBObject(xrecord, true);
				
				return xrecord;
			}
		}

		// ==============================================================================================
		// SETTING DATA
		// ==============================================================================================
		public static bool SetXRecordAs<T>(DBDictionary dictionary, string xrecordName, T val)
		{
			using (Session.LockDocument())
			{
				using (Transaction transaction = Session.StartTransaction())
				{
					DBDictionary dictionaryMutable = transaction.GetObject(dictionary.ObjectId, OpenMode.ForWrite) as DBDictionary;

					Xrecord xrecord = GetXRecord(transaction, dictionaryMutable, xrecordName);

					xrecord.UpgradeOpen();

					DxfCode code = DxfCode.End;

					switch (val)
					{
						case string s:
						{
							code = DxfCode.XTextString;
							break;
						}
						case int i:
						{
							code = DxfCode.Int32;
							break;
						}
						case double d:
						{
							code = DxfCode.Real;
							break;
						}
						case bool b:
						{
							code = DxfCode.Bool;
							break;
						}
						default:
						{
							return false;
						}
					}

					ResultBuffer buffer = new ResultBuffer();
					buffer.Add(new TypedValue((int)code, val));
					xrecord.Data = buffer;

					transaction.Commit();
					return true;
				}
			}
		}

		// ==============================================================================================
		// DESTROYING DATA
		// ==============================================================================================
		public static bool DestroyXRecord(DBDictionary dictionary, string xrecordName)
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				if (dictionary.Remove(xrecordName) != ObjectId.Null)
				{
					transaction.Commit();
					return true;
				}

				return false;
			}
		}
	}
}
