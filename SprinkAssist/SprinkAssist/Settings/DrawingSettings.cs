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
		protected OBSOLETEDictionaryPath path;
		protected string name;
		protected object defaultValue;

		protected Setting(OBSOLETEDictionaryPath path, string name, object defaultValue)
		{
			this.path = path;
			this.name = name;
			this.defaultValue = defaultValue;
		}
		
		protected object objectValue
		{
			get
			{
				object data = OBSOLETEDataStore.GetXrecordDataObject(path, name);
				return (data == null) ? defaultValue : data;
			}
		}
	}

	public class OBSOLETEStringSetting : Setting
	{
		public OBSOLETEStringSetting(OBSOLETEDictionaryPath path, string name, string defaultValue) : base(path, name, defaultValue) { }

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

				OBSOLETEDataStore.SetXrecordString(path, name, value);
			}
		}

		public static implicit operator string(OBSOLETEStringSetting s) => s.stringValue;

		public void Set(string val)
		{
			stringValue = val;
		}
	}

	public class IntSetting : Setting
	{
		public IntSetting(OBSOLETEDictionaryPath path, string name, int defaultValue) : base(path, name, defaultValue) { }

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

				OBSOLETEDataStore.SetXrecordInt(path, name, (int)value);
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
		public DoubleSetting(OBSOLETEDictionaryPath path, string name, double defaultValue) : base(path, name, defaultValue) { }

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

				OBSOLETEDataStore.SetXrecordDouble(path, name, (double)value);
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
		public BoolSetting(OBSOLETEDictionaryPath path, string name, bool defaultValue) : base(path, name, defaultValue) { }

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

				OBSOLETEDataStore.SetXrecordBool(path, name, (bool)value);
			}
		}

		public static implicit operator bool?(BoolSetting s) => s.boolValue;

		public void Set(bool val)
		{
			boolValue = val;
		}
	}
}
