//#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.DatabaseServices;

namespace Ironwill
{
	public struct DictionaryPath
	{
		List<string> path;

		public DictionaryPath(params string[] inPath)
		{
			path = new List<string>(inPath);
		}

		public DictionaryPath(DictionaryPath parent, params string[] child)
		{
			path = new List<string>(parent.Get);
			path.AddRange(child);
		}

		public List<string> Get
		{
			get { return path; }
		}
	}

	class DataStore
	{
		protected const string PluginRoot = "SprinkAssist";

		/** Retrieve the base DBDictionary that contains all things for my plugin */
		protected static DBDictionary GetPluginDictionary(Transaction transaction)
		{
			ObjectId MasterDictionaryId = Session.GetDatabase().NamedObjectsDictionaryId;
			DBDictionary MasterDictionary = transaction.GetObject(MasterDictionaryId, OpenMode.ForRead) as DBDictionary;

			if (MasterDictionary.Contains(PluginRoot))
			{
				ObjectId PluginDictionaryId = MasterDictionary.GetAt(PluginRoot);

				return transaction.GetObject(PluginDictionaryId, OpenMode.ForRead) as DBDictionary;
			}
			else
			{
				DBDictionary pluginDictionary = new DBDictionary();
				MasterDictionary.UpgradeOpen();
				MasterDictionary.SetAt(PluginRoot, pluginDictionary);
				transaction.AddNewlyCreatedDBObject(pluginDictionary, true);

				return pluginDictionary;
			}
		}

		/** Retrieve a given subdictionary for the plugin */
		public static DBDictionary GetDictionary(Transaction transaction, DictionaryPath path, bool create = false)
		{
			DBDictionary pluginDictionary = GetPluginDictionary(transaction);

			DBDictionary nestedDictionary = pluginDictionary;

			for (int i = 0; i < path.Get.Count; i++)
			{
				string name = path.Get[i];
				
				if (name == string.Empty)
				{
					continue;
				}

				if (nestedDictionary.Contains(name))
				{
					nestedDictionary = transaction.GetObject(nestedDictionary.GetAt(path.Get[i]), OpenMode.ForRead) as DBDictionary;
				}
				else
				{
					if (create)
					{
						DBDictionary newDictionary = new DBDictionary();
						nestedDictionary.UpgradeOpen();
						nestedDictionary.SetAt(name, newDictionary);
						//nestedDictionary[name] = newDictionary;
						transaction.AddNewlyCreatedDBObject(newDictionary, true);
						nestedDictionary = newDictionary;
					}
					else
					{
						return null;
					}
				}
			}

			return nestedDictionary;
		}

		protected static Xrecord GetXRecord(Transaction transaction, DictionaryPath path, string name, bool create = false)
		{
			DBDictionary dictionary = GetDictionary(transaction, path, create);

			if (dictionary == null)
			{
				return null;
			}

			if (!dictionary.Contains(name))
			{
				if (create)
				{
					string m = "Creating new XRecord: ";
					foreach (string s in path.Get) { m += "\\" + s; }
					Session.Log(m);

					Xrecord xrecord = new Xrecord();
					dictionary.UpgradeOpen();
					dictionary.SetAt(name, xrecord);
					transaction.AddNewlyCreatedDBObject(xrecord, true);
					return xrecord;
				}
				else
				{
					return null;
				}
			}


			ObjectId xrecordId = dictionary.GetAt(name);
			return transaction.GetObject(xrecordId, OpenMode.ForRead) as Xrecord;
		}

		protected static ResultBuffer GetXrecordData(DictionaryPath path, string name)
		{
			using (Session.LockDocument())
			{
				using (Transaction transaction = Session.StartTransaction())
				{
					Xrecord xrecord = GetXRecord(transaction, path, name, false);

					if (xrecord == null)
					{
						return null;
					}

					return xrecord.Data;
				}
			}
		}

		// ================================================================================================
		// Public Getters
		public static object GetXrecordDataObject(DictionaryPath path, string name)
		{
			ResultBuffer data = GetXrecordData(path, name);

			if (data == null)
			{
				return null;
			}

			TypedValue typedValue = data.AsArray()[0];

			return typedValue.Value;
		}

		public static string GetXrecordString(DictionaryPath path, string name, string defaultValue = "")
		{
			string val = GetXrecordDataObject(path, name) as string;// data.AsArray()[0];

			if (val == null)
			{
				return defaultValue;
			}

			return val;
		}

		public static int GetXrecordInt(DictionaryPath path, string name, int defaultValue = 0)
		{
			int? val = GetXrecordDataObject(path, name) as int?;

			if (val == null)
			{
				return defaultValue;
			}

			return (int)val;
		}

		public static double GetXrecordDouble(DictionaryPath path, string name, double defaultValue = 0.0)
		{
			double? val = GetXrecordDataObject(path, name) as double?;

			if (val == null)
			{
				return defaultValue;
			}

			return (double)val;
		}


		// ================================================================================================
		protected static void SetXrecordData(DictionaryPath path, string name, ResultBuffer data)
		{
			using (Session.LockDocument())
			{
				using (Transaction transaction = Session.StartTransaction())
				{
					Xrecord xrecord = GetXRecord(transaction, path, name, true);

					xrecord.UpgradeOpen();

					xrecord.Data = data;
					transaction.Commit();

					xrecord.DowngradeOpen();
				}
			}
		}

		public static void SetXrecordString(DictionaryPath path, string name, string value)
		{
			string m = "Setting XRecord string: ";
			foreach (string s in path.Get) { m += "\\" + s; }
			Session.Log(m);

			ResultBuffer data = new ResultBuffer();
			data.Add(new TypedValue((int)DxfCode.XTextString, value));
			SetXrecordData(path, name, data);
		}

		public static void SetXrecordInt(DictionaryPath path, string name, int value)
		{
			ResultBuffer data = new ResultBuffer();
			data.Add(new TypedValue((int)DxfCode.Int32, value));
			SetXrecordData(path, name, data);
		}

		public static void SetXrecordDouble(DictionaryPath path, string name, double value)
		{
			ResultBuffer data = new ResultBuffer();
			data.Add(new TypedValue((int)DxfCode.Real, value));
			SetXrecordData(path, name, data);
		}

		public static void SetXrecordBool(DictionaryPath path, string name, bool value)
		{
			ResultBuffer data = new ResultBuffer();
			data.Add(new TypedValue((int)DxfCode.Bool, value));
			SetXrecordData(path, name, data);
		}
	}
}
