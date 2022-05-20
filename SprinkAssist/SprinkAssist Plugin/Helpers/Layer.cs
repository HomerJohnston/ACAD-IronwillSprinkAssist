using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill
{
	public class Layer
	{
		public const string Pfix = "Spk_";
		// --------------------------------------------------------------------	Name ------------------------------	Color -----------------	Plot --	Deprecated Names
		public static LayerStruct Default					= new LayerStruct(	"0",								Colors.White,			true);
		
		public static LayerStruct Area_CalcBackground		= new LayerStruct(	Pfix + "Area_CalcBackground",		Colors.White,			true,	"SpkCalc");
		public static LayerStruct Calculation				= new LayerStruct(	Pfix + "Calculation",				Colors.White,			true,	"SpkCalc", "Spk_Calculations");
		public static LayerStruct Detail					= new LayerStruct(	Pfix + "Detail",					Colors.White,			true,	"SpkDetail", "Spk_Detail", "Sprk_Detail");
		public static LayerStruct Dimension					= new LayerStruct(	Pfix + "Dimension",					Colors.White,			true,	"SpkDimension");
		public static LayerStruct DraftAid					= new LayerStruct(	Pfix + "DraftAid",					Colors.DarkPurple,		false,	"SpkDraftAid");
		public static LayerStruct Extinguisher				= new LayerStruct(	Pfix + "Extinguisher",				Colors.White,			true,	"SpkExtinguishers");
		public static LayerStruct HeadCoverage				= new LayerStruct(	Pfix + "HeadCoverage",				Colors.Turqoise,		false,	"SpkSystem_Head_Coverage");
		public static LayerStruct HeadLegend				= new LayerStruct(	Pfix + "HeadLegend",				Colors.White,			true);
		public static LayerStruct Note						= new LayerStruct(	Pfix + "Note",						Colors.White,			true,	"SpkNote");
		public static LayerStruct PersonalNote				= new LayerStruct(	Pfix + "PersonalNote",				Colors.LightGreen,		true);

		public static LayerStruct PipeLabel					= new LayerStruct(	Pfix + "PipeLabel",					Colors.White,			true,	"SpkPipeLabel");
		public static LayerStruct PipeLabel_Dia				= new LayerStruct(	Pfix + "PipeLabel_Diamater",		Colors.White,			true,	"SpkPipeLabel_Dia");
		public static LayerStruct PipeLabel_Group			= new LayerStruct(	Pfix + "PipeLabel_Group",			Colors.White,			true,	"SpkPipeLabel_Group");
		public static LayerStruct PipeLabel_Length			= new LayerStruct(	Pfix + "PipeLabel_Length",			Colors.White,			true,	"SpkPipeLabel_Lgth");
		public static LayerStruct PipeLabel_Slope			= new LayerStruct(	Pfix + "PipeLabel_Slope",			Colors.White,			true);

		public static LayerStruct Rev						= new LayerStruct(	Pfix + "Rev",						Colors.DarkOrange,		true);

		public static LayerStruct SystemDevice				= new LayerStruct(	Pfix + "System_Device",				Colors.DarkGrey,		true,	"SpkSystem_Device");
		public static LayerStruct SystemFitting				= new LayerStruct(	Pfix + "System_Fitting",			Colors.Orange,			true,	"SpkSystem_Fitting");
		public static LayerStruct SystemHead				= new LayerStruct(	Pfix + "System_Head",				Colors.LightRed,		true,	"SpkSystem_Head");

		public static LayerStruct SystemPipe_Armover		= new LayerStruct(	Pfix + "System_Pipe_Armover",		Colors.DarkRed,			true,	"SpkPipe_Armover");
		public static LayerStruct SystemPipe_AuxDrain		= new LayerStruct(	Pfix + "System_Pipe_AuxDrain",		Colors.DarkRed,			true,	"SpkPipe_AuxDrain");
		public static LayerStruct SystemPipe_Branchline		= new LayerStruct(	Pfix + "System_Pipe_Branchline",	Colors.OrangeRed,		true,	"SpkPipe_Branchline");
		public static LayerStruct SystemPipe_Main			= new LayerStruct(	Pfix + "System_Pipe_Main",			Colors.PinkRed,			true,	"SpkPipe_Main");

		public static LayerStruct Viewport					= new LayerStruct(	Pfix + "Viewport",					Colors.White,			true,	"SpkViewport");
		public static LayerStruct Viewport_Hidden			= new LayerStruct(	Pfix + "Viewport_Hidden",			Colors.DarkTurqoise,	true,	"SpkViewport_NoPlot");

		public static LayerStruct Wipeout					= new LayerStruct(	Pfix + "Wipeout",					Colors.ByLayer,			true);

		public static LayerStruct XREF						= new LayerStruct(	Pfix + "XREF",						Colors.LightTurqoise,	false,	"SpkXREF");
	}
}
