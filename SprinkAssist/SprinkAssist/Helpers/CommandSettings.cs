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

			if (!owningDictionary.Contains(settingName))
			{
				return (T)defaultValue;
			}

			return (T)XRecordLibrary.ReadXRecordData(transaction, owningDictionary, settingName);
		}

		public void Set(Transaction transaction, T newValue)
		{
			switch (newValue)
			{
				case string s:
				{
					XRecordLibrary.SetXRecord(transaction, owningDictionary, settingName, s);
					break;
				}
				case int i:
				{
					XRecordLibrary.SetXRecord(transaction, owningDictionary, settingName, i);
					break;
				}
				case double d:
				{
					XRecordLibrary.SetXRecord(transaction, owningDictionary, settingName, d);
					break;
				}
				case bool b:
				{
					XRecordLibrary.SetXRecord(transaction, owningDictionary, settingName, b);
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
