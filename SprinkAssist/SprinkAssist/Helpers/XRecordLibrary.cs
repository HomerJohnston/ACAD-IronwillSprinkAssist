using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill
{
    /**
     * The plugin has a dictionary structure like this:
     * Drawing's NamedObjectDictionary
     *  -
     */
	internal class XRecordLibrary
	{
		protected const string sprinkAssistDictionaryName = "SprinkAssist";

		// ==============================================================================================
		// DICTIONARY MANAGEMENT
		// ==============================================================================================
        
        /** Master dictionary - very top level */
        private static DBDictionary GetSprinkAssistMasterDictionary(Transaction transaction)
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

                return pluginDictionary;
            }
        }
        
        /** General dictionaries - these will reside under the Master dictionary and are intended to hold things like global drawing settings */
        public static DBDictionary GetDrawingDictionary(Transaction transaction, string dictionaryName)
        {
            DBDictionary parent = GetSprinkAssistMasterDictionary(transaction);

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

                return namedDictionary;
            }
        }
        
        /** Commands dictionary - this resides under Master and is intended to hold sub-dictionaries for each command */
        private static DBDictionary GetCommandsDictionary(Transaction transaction)
        {
            return GetSubDictionary(transaction, "IFE_SA_Command", GetSprinkAssistMasterDictionary(transaction));
        }
        
        /** Dictionary for an individual command, intended to hold settings directly and/or subdictionaries */
		public static DBDictionary GetCommandDictionary(Transaction transaction, Type command)
		{
			DBDictionary commandSettingsDictionary = GetCommandsDictionary(transaction);

			string commandName = command.Name;

			return GetSubDictionary(transaction, commandName, commandSettingsDictionary);
		}

        /** Generic helper to dig to any subdictionary */
        public static DBDictionary GetSubDictionary(Transaction transaction, string dictionaryName, DBDictionary parent)
        {
            if (parent == null)
            {
                Session.Log("Tried to get child dictionary but parent was not specified!");
                return null;
            }

            if (dictionaryName == string.Empty)
            {
                Session.Log("Tried to get child dictionary but dictionaryName was not specified!");
                return null;
            }

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

                return namedDictionary;
            }
        }
        
		public static void DestroyDictionary(Transaction transaction, DBDictionary dictionaryToDestroy, DBDictionary parent)
        {
            parent.Remove(dictionaryToDestroy.ObjectId);
		}
        
        public static bool DestroyDictionary(Transaction transaction, string dictionaryName, DBDictionary parent)
        {
            if (parent.Remove(dictionaryName) != ObjectId.Null)
            {
                return true;
            }

            return false;
        }
        
		// ==============================================================================================
		// GETTING DATA
		// ==============================================================================================
		public static bool ReadXRecord<T>(Transaction transaction, DBDictionary dictionary, string xrecordName, ref T recordValue)
		{
			using (Session.LockDocument())
			{
				Xrecord xrecord = GetXRecord(transaction, dictionary, xrecordName);

				if (xrecord == null)
				{
					return false;
				}

				TypedValue typedValue = xrecord.Data.AsArray()[0];
				recordValue = (T)typedValue.Value;

				return true;
			}
		}

		private static Xrecord GetXRecord(Transaction transaction, DBDictionary dictionary, string name)
		{
			try
			{
				ObjectId xrecordId = dictionary.GetAt(name);
				return transaction.GetObject(xrecordId, OpenMode.ForRead) as Xrecord;
			}
			catch
			{
				return null;
			}
		}

		// ==============================================================================================
		// SETTING DATA
		// ==============================================================================================
		public static bool WriteXRecord<T>(Transaction transaction, DBDictionary dictionary, string xrecordName, T val)
		{
			using (Session.LockDocument())
			{
				DBDictionary dictionaryMutable = transaction.GetObject(dictionary.ObjectId, OpenMode.ForWrite) as DBDictionary;

				Xrecord xrecord = GetXRecord(transaction, dictionaryMutable, xrecordName);

                if (xrecord == null)
				{
					xrecord = CreateXRecord(transaction, dictionaryMutable, xrecordName);
				}

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
                        Session.Log("Unhandled data type! Nothing written.");
						return false;
					}
				}

				ResultBuffer buffer = new ResultBuffer();
				buffer.Add(new TypedValue((int)code, val));
				xrecord.Data = buffer;

				xrecord.DowngradeOpen();

				return true;
			}
		}

		private static Xrecord CreateXRecord(Transaction transaction, DBDictionary dictionary, string xrecordName)
		{
			Xrecord xrecord = new Xrecord();

			dictionary.SetAt(xrecordName, xrecord);
			transaction.AddNewlyCreatedDBObject(xrecord, true);

			return xrecord;
		}

		// ==============================================================================================
		// DESTROYING DATA
		// ==============================================================================================
		public static bool DestroyXRecord(Transaction transaction, DBDictionary dictionary, string xrecordName)
		{
			if (dictionary.Remove(xrecordName) != ObjectId.Null)
			{
				return true;
			}

			return false;
		}
	}
}
