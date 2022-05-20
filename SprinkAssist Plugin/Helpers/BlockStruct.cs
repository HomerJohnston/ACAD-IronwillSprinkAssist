using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Colors;

namespace Ironwill
{
	public class BlockStruct
	{
		private string Name
		{
			get;
			set;
		}

		private string DefaultLayer
		{
			get;
			set;
		}

		private string[] DeprecatedNames
		{
			get;
			set;
		}

		public BlockStruct(string name, string defaultLayer, params string[] deprecatedNames)
		{
			Name = name;
			DefaultLayer = defaultLayer;
			DeprecatedNames = deprecatedNames;
		}

		public string Get()
		{
			var database = Session.GetDatabase();

			using (Transaction transaction = Session.StartTransaction())
			{
				BlockTable blockTable = transaction.GetObject(database.BlockTableId, OpenMode.ForWrite) as BlockTable;

				if (blockTable == null)
				{
					transaction.Abort();
					return "";
				}

				if (blockTable.Has(Name))
				{
					transaction.Commit();
					return Name;
				}
				else
				{
					foreach (string deprecatedName in DeprecatedNames)
					{
						if (blockTable.Has(deprecatedName))
						{
							transaction.Commit();
							return deprecatedName;
						}
					}

					Session.Log("WARNING: Required block " + Name + " was not found");
					return "";
				}
			}
		}
	}
}
