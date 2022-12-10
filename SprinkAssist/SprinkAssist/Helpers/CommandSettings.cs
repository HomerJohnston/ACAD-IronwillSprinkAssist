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

		readonly DBDictionary owningDictionary;

		readonly object defaultValue;

		public CommandSetting(string inSettingName, object inDefaultValue, DBDictionary inOwningDictionary)
		{
			settingName = inSettingName;
			defaultValue = inDefaultValue;
			owningDictionary = inOwningDictionary;
		}

		public T Get(Transaction transaction)
		{
			DBDictionary lookupDictionary = owningDictionary;

			if (lookupDictionary == null)
			{
				Session.LogDebug("Warning: CommandSetting {0} has no owning dictionary, using global store");
				lookupDictionary = XRecordLibrary.GetGlobalDictionary(transaction);
			}

			T val = (T)defaultValue;

			XRecordLibrary.ReadXRecord<T>(transaction, owningDictionary, settingName, ref val);

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
	}
}
