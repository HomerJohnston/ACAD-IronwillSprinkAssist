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
	public struct LayerStruct
	{
		private string Name
		{
			get;
			set;
		}

		private string[] DeprecatedNames
		{
			get;
			set;
		}

		private short ColorIndex
		{
			get;
			set;
		}

		private bool Plot
		{
			get;
			set;
		}

		public LayerStruct(string name, short color, bool plot, params string[] deprecatedNames)
		{
			Name = name;
			ColorIndex = color;
			Plot = plot;
			DeprecatedNames = deprecatedNames;
		}

		public string Get()
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				LayerTable layerTable = Session.GetLayerTable(transaction);

				if (layerTable.Has(Name))
				{
					transaction.Commit();
					return Name;
				}
				else
				{
					foreach (string deprecatedName in DeprecatedNames)
					{
						if (layerTable.Has(deprecatedName))
						{
							transaction.Commit();
							return deprecatedName;
						}
					}

					Session.Log("WARNING: Created missing layer " + Name);

					LayerTableRecord layer = new LayerTableRecord();
					layer.Name = Name;
					layer.Color = Color.FromColorIndex(ColorMethod.ByAci, ColorIndex);

					layerTable.UpgradeOpen();
					layerTable.Add(layer);
					layerTable.DowngradeOpen();

					transaction.AddNewlyCreatedDBObject(layer, true);

					transaction.Commit();
					return Name;
				}
			}
		}
	}
}
