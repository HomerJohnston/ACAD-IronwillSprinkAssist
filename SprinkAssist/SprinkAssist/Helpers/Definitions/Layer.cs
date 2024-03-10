using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill
{
	// TODO move this to file data
	public class Layer
	{
		public const string Pfix = "Spk_";
		// --------------------------------------------------------------------	Name ------------------------------	Color -----------------	Plot --	Deprecated Names
		public static LayerStruct Default					= new LayerStruct(	"0",								Colors.White,			true	);
		
		public static LayerStruct Area_CalcBackground		= new LayerStruct(	Pfix + "Area_CalcBackground",		Colors.White,			true,	"SpkCalc");
		public static LayerStruct Calculation				= new LayerStruct(	Pfix + "Calculation",				Colors.White,			true,	"SpkCalc", "Spk_Calculations");
		public static LayerStruct Detail					= new LayerStruct(	Pfix + "Detail",					Colors.White,			true,	"SpkDetail", "Spk_Detail", "Sprk_Detail");
		public static LayerStruct Dimension					= new LayerStruct(	Pfix + "Dimension",					Colors.White,			true,	"SpkDimension");
		public static LayerStruct DraftAid					= new LayerStruct(	Pfix + "DraftAid",					Colors.DarkPurple,		false,	"SpkDraftAid");
		public static LayerStruct Extinguisher				= new LayerStruct(	Pfix + "Extinguisher",				Colors.White,			true,	"SpkExtinguishers");
		public static LayerStruct HeadCoverage				= new LayerStruct(	Pfix + "HeadCoverage",				Colors.Turqoise,		false,	"SpkSystem_Head_Coverage");
		public static LayerStruct HeadCoverage_Fill			= new LayerStruct(	Pfix + "HeadCoverage_Fill",			Colors.Turqoise,		false	);
		public static LayerStruct HeadLegend				= new LayerStruct(	Pfix + "HeadLegend",				Colors.White,			true	);
		public static LayerStruct Note						= new LayerStruct(	Pfix + "Note",						Colors.White,			true,	"SpkNote");
		public static LayerStruct PersonalNote				= new LayerStruct(	Pfix + "PersonalNote",				Colors.LightGreen,		true	);

		public static LayerStruct PipeLabel					= new LayerStruct(	Pfix + "PipeLabel",					Colors.White,			true,	"SpkPipeLabel");
		public static LayerStruct PipeLabel_Dia				= new LayerStruct(	Pfix + "PipeLabel_Diamater",		Colors.White,			true,	"SpkPipeLabel_Dia");
		public static LayerStruct PipeLabel_Group			= new LayerStruct(	Pfix + "PipeLabel_Group",			Colors.White,			true,	"SpkPipeLabel_Group");
		public static LayerStruct PipeLabel_Length			= new LayerStruct(	Pfix + "PipeLabel_Length",			Colors.White,			true,	"SpkPipeLabel_Lgth");
		public static LayerStruct PipeLabel_Slope			= new LayerStruct(	Pfix + "PipeLabel_Slope",			Colors.White,			true	);

		public static LayerStruct Rev						= new LayerStruct(	Pfix + "Rev",						Colors.DarkOrange,		true	);

		public static LayerStruct SystemDevice				= new LayerStruct(	Pfix + "System_Device",				Colors.DarkGrey,		true,	"SpkSystem_Device");
		public static LayerStruct SystemFitting				= new LayerStruct(	Pfix + "System_Fitting",			Colors.Orange,			true,	"SpkSystem_Fitting");
		public static LayerStruct SystemHead				= new LayerStruct(	Pfix + "System_Head",				Colors.LightRed,		true,	"SpkSystem_Head");

		public static LayerStruct SystemPipe				= new LayerStruct(	Pfix + "System_Pipe",				Colors.OrangeRed,		true,	"SpkPipe");
		public static LayerStruct SystemPipe_Armover		= new LayerStruct(	Pfix + "System_Pipe_Armover",		Colors.DarkRed,			true,	"SpkPipe_Armover");
		public static LayerStruct SystemPipe_AuxDrain		= new LayerStruct(	Pfix + "System_Pipe_AuxDrain",		Colors.DarkRed,			true,	"SpkPipe_AuxDrain");
		public static LayerStruct SystemPipe_Branchline		= new LayerStruct(	Pfix + "System_Pipe_Branchline",	Colors.OrangeRed,		true,	"SpkPipe_Branchline");
		public static LayerStruct SystemPipe_Main			= new LayerStruct(	Pfix + "System_Pipe_Main",			Colors.PinkRed,			true,	"SpkPipe_Main");

		public static LayerStruct Viewport					= new LayerStruct(	Pfix + "Viewport",					Colors.White,			true,	"SpkViewport");
		public static LayerStruct Viewport_Hidden			= new LayerStruct(	Pfix + "Viewport_Hidden",			Colors.DarkTurqoise,	true,	"SpkViewport_NoPlot");

		public static LayerStruct Wipeout					= new LayerStruct(	Pfix + "Wipeout",					Colors.ByLayer,			true	);

		public static LayerStruct XREF						= new LayerStruct(	Pfix + "XREF",						Colors.LightTurqoise,	false,	"SpkXREF");
	}
	
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

				// TODO this isn't robust. I should throw errors if multiple layers exist. I should also have a layer fixer upper that merges old layers to new.
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

		static public bool operator !=(string other, LayerStruct layerStruct)
		{
			return layerStruct != other;
		}

		static public bool operator ==(string other, LayerStruct layerStruct)
		{
			return layerStruct == other;
		}

		static public bool operator !=(LayerStruct layerStruct, string other)
		{
			return !(layerStruct == other);
		}

		static public bool operator ==(LayerStruct layerStruct, string other)
		{
			if (other == layerStruct.Name)
			{
				return true;
			}

			if (layerStruct.DeprecatedNames.Contains(other))
			{
				return true;
			}

			return false;
		}

		static public bool Equals(LayerStruct layerStruct1, LayerStruct layerStruct2)
		{
			return layerStruct1.Get() == layerStruct2.Get();
		}

		public override bool Equals(object obj)
		{
			return obj is LayerStruct @struct &&
				   Get() == @struct.Get();
		}

		public override int GetHashCode()
		{
			return 539060726 + EqualityComparer<string>.Default.GetHashCode(Get());
		}

		public static implicit operator string(LayerStruct layerStruct)
		{
			return layerStruct.Get();
		}
	}
}
