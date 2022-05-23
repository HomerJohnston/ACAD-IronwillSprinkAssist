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

		private object CurrentValue
		{
			get
			{
				DBDictionary lookupDictionary = owningDictionary;

				if (lookupDictionary == null)
				{
					Session.LogDebug("Warning: CommandSetting {0} has no owning dictionary, using global store");
					lookupDictionary = XRecordLibrary.GetGlobalDictionary();
				}

				if (!owningDictionary.Contains(settingName))
				{
					return defaultValue;
				}

				return XRecordLibrary.ReadXRecordData(owningDictionary, settingName);
			}
			set
			{
				switch (value)
				{
					case string s:
					{
						XRecordLibrary.SetXRecordAs(owningDictionary, settingName, s);
						break;
					}
					case int i:
					{
						XRecordLibrary.SetXRecordAs(owningDictionary, settingName, i);
						break;
					}
					case double d:
					{
						XRecordLibrary.SetXRecordAs(owningDictionary, settingName, d);
						break;
					}
					case bool b:
					{
						XRecordLibrary.SetXRecordAs(owningDictionary, settingName, b);
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

		public CommandSetting(string inSettingName, object inDefaultValue, DBDictionary inOwningDictionary)
		{
			settingName = inSettingName;
			defaultValue = inDefaultValue;
			owningDictionary = inOwningDictionary;
		}

		public T Get()
		{
			return (T)CurrentValue;
		}

		public void Set(T newValue)
		{
			CurrentValue = newValue;
		}
	}
}
