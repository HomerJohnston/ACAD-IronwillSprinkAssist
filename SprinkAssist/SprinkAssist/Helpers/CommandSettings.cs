using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill
{
	// ============================================================================================

	internal class CommandSetting<T>
	{
		readonly string settingName;

		DBDictionary owningDictionary;

		readonly object defaultValue;

        public CommandSetting(string inSettingName, object inDefaultValue)
        {
            settingName = inSettingName;
            defaultValue = inDefaultValue;
        }

        public CommandSetting(string inSettingName, object inDefaultValue, DBDictionary inOwningDictionary)
		{
			settingName = inSettingName;
			defaultValue = inDefaultValue;
			owningDictionary = inOwningDictionary;
		}

        public T Get(Transaction transaction)
        {
			T val = (T)defaultValue;

			if (!XRecordLibrary.ReadXRecord(transaction, owningDictionary, settingName, ref val))
			{
				Session.LogDebug($"Value {settingName} not found; using default");
			}

			return val;
        }

		public void Set(Transaction transaction, T newValue)
		{
			switch (newValue)
			{
				case string s:
				{
					XRecordLibrary.WriteXRecord(transaction, owningDictionary, settingName, s);
					break;
				}
				case int i:
				{
					XRecordLibrary.WriteXRecord(transaction, owningDictionary, settingName, i);
					break;
				}
				case double d:
				{
					XRecordLibrary.WriteXRecord(transaction, owningDictionary, settingName, d);
					break;
				}
				case bool b:
				{
					XRecordLibrary.WriteXRecord(transaction, owningDictionary, settingName, b);
					break;
				}
				default:
				{
					Session.Log("Error, tried to set setting {0} with unhandled type!", settingName);
					break;
				}
			}
		}

		public void SetOwningDictionary(DBDictionary dictionary)
		{
			owningDictionary = dictionary;
		}
	}
}
