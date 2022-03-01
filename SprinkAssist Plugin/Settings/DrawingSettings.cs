using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace Ironwill
{
	public class Setting
	{
		protected DictionaryPath path;
		protected string name;
		protected object defaultValue;

		protected Setting(DictionaryPath path, string name, object defaultValue)
		{
			this.path = path;
			this.name = name;
			this.defaultValue = defaultValue;
		}
		
		protected object objectValue
		{
			get
			{
				object data = DataStore.GetXrecordDataObject(path, name);
				return (data == null) ? defaultValue : data;
			}
		}
	}

	public class StringSetting : Setting
	{
		public StringSetting(DictionaryPath path, string name, string defaultValue) : base(path, name, defaultValue) { }

		public string stringValue
		{
			get
			{
				return objectValue as string;
			}
			private set
			{
				if (value == null)
					return;

				DataStore.SetXrecordString(path, name, value);
			}
		}

		public static implicit operator string(StringSetting s) => s.stringValue;

		public void Set(string val)
		{
			stringValue = val;
		}
	}

	public class IntSetting : Setting
	{
		public IntSetting(DictionaryPath path, string name, int defaultValue) : base(path, name, defaultValue) { }

		public int? intValue
		{
			get
			{
				return objectValue as int?;
			}
			private set
			{
				if (value == null)
					return;

				DataStore.SetXrecordInt(path, name, (int)value);
			}
		}

		public static implicit operator int?(IntSetting s) => s.intValue;

		public void Set(int val)
		{
			intValue = val;
		}
	}

	public class DoubleSetting : Setting
	{
		public DoubleSetting(DictionaryPath path, string name, double defaultValue) : base(path, name, defaultValue) { }

		public double? doubleValue
		{
			get
			{
				return objectValue as double?;
			}
			private set
			{
				if (value == null)
					return;

				DataStore.SetXrecordDouble(path, name, (double)value);
			}
		}

		public static implicit operator double?(DoubleSetting s) => s.doubleValue;

		public void Set(double val)
		{
			doubleValue = val;
		}
	}

	public class BoolSetting : Setting
	{
		public BoolSetting(DictionaryPath path, string name, bool defaultValue) : base(path, name, defaultValue) { }

		public bool? boolValue
		{
			get
			{
				return objectValue as bool?;
			}
			private set
			{
				if (value == null)
					return;

				DataStore.SetXrecordBool(path, name, (bool)value);
			}
		}

		public static implicit operator bool?(BoolSetting s) => s.boolValue;

		public void Set(bool val)
		{
			boolValue = val;
		}
	}
}
