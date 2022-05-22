using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill
{
	/**
	 * 
	 */
	internal class CommandSettingsContainer
	{
		/**  */
		protected DBDictionary parent = null;

		/**  */
		protected string className = string.Empty;

		/**  */
		protected DBDictionary commandSettingsDictionary = null;

		/**  */
		protected Dictionary<string, CommandSetting> settings = new Dictionary<string, CommandSetting>();

		/**  */
		protected Dictionary<string, CommandSettingsContainer> nestedCommandSettingsContainers = new Dictionary<string, CommandSettingsContainer>();

		/**  */
		public CommandSettingsContainer(string inClassName, DBDictionary inParent = null)
		{
			className = inClassName;
			parent = inParent;
		}

		/**  */
		public void AddSetting<T>(string settingName, T defaultValue)
		{
			if (!EnsureValidDictionary(className))
			{
				Session.Log("Warning: could not create setting {0}!", settingName);
				return;
			}

			CommandSetting newSetting = new CommandSetting(settingName, defaultValue, commandSettingsDictionary);

			settings.Add(settingName, newSetting);
		}

		/**  */
		bool EnsureValidDictionary(string dictionaryName)
		{
			if (commandSettingsDictionary == null)
			{
				commandSettingsDictionary = XRecordHelper.GetNamedDictionary(className, parent);
			}

			return commandSettingsDictionary != null;
		}

		public T Get<T>(string settingName)
		{
			CommandSetting setting;

			if (settings.TryGetValue(settingName, out setting))
			{
				return setting.GetAs<T>();
			}

			Session.Log("Warning: could not get valid value for setting {0}", settingName);
			return default(T);
		}

		public void Set<T>(string settingName, T newValue)
		{
			CommandSetting setting;

			if (settings.TryGetValue(settingName, out setting))
			{
				setting.SetTo(newValue);
			}
			else
			{
				Session.Log("Warning: could not set setting {0}", settingName);
			}
		}

		public CommandSettingsContainer AddNestedContainer(string nestedContainerName)
		{
			CommandSettingsContainer nestedCommandSettingsContainer = new CommandSettingsContainer(className);
			nestedCommandSettingsContainers.Add(nestedContainerName, nestedCommandSettingsContainer);
			return nestedCommandSettingsContainer;
		}

		public CommandSetting this[string settingName]
		{
			get => settings[settingName];
		}
	}

	internal class CommandSetting
	{
		string settingName;

		DBDictionary owningDictionary;

		protected object defaultValue;

		protected object currentValue
		{
			get
			{
				if (!owningDictionary.Contains(settingName))
				{
					return defaultValue;
				}

				return XRecordHelper.ReadXRecordData(owningDictionary, settingName);
			}
			set
			{
				switch (value)
				{
					case string s:
					{
						XRecordHelper.SetXRecordAs(owningDictionary, settingName, s);
						break;
					}
					case int i:
					{
						XRecordHelper.SetXRecordAs(owningDictionary, settingName, i);
						break;
					}
					case double d:
					{
						XRecordHelper.SetXRecordAs(owningDictionary, settingName, d);
						break;
					}
					case bool b:
					{
						XRecordHelper.SetXRecordAs(owningDictionary, settingName, b);
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

		public T GetAs<T>()
		{
			return (T)currentValue;
		}

		public void SetTo<T>(T newValue)
		{
			currentValue = newValue;
		}
	}
}
