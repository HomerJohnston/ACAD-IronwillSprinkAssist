using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill
{
	class Layers
	{
		const int White = 7;
		const int DarkPurple = 180;
		const int DarkGrey = 8;
		const int Orange = 30;
		const int DarkOrange = 22;
		const int LightRed = 240;
		const int OrangeRed = 242;
		const int PinkRed = 232;
		const int DarkRed = 248;
		const int LightTurqoise = 123;
		const int Turqoise = 144;
		const int DarkTurqoise = 148;
		const int LightGreen = 60;

		// --------------------------------------------------------------------	Name ------------------------------	Color ---------	Plot --	Deprecated Names
		public static LayerStruct Default					= new LayerStruct(	"0",								White,			true);
		
		public static LayerStruct Area_CalcBackground		= new LayerStruct(	"Spk_Area_CalcBackground",			White,			true,	"SpkCalc");
		public static LayerStruct Calculation				= new LayerStruct(	"Spk_Calculation",					White,			true,	"SpkCalc");
		public static LayerStruct Detail					= new LayerStruct(	"Spk_Detail",						White,			true,	"SpkDetail", "Spk_Detail", "Sprk_Detail");
		public static LayerStruct Dimension					= new LayerStruct(	"Spk_Dimension",					White,			true,	"SpkDimension");
		public static LayerStruct DraftAid					= new LayerStruct(	"Spk_DraftAid",						DarkPurple,		false,	"SpkDraftAid");
		public static LayerStruct Extinguisher				= new LayerStruct(	"Spk_Extinguisher",					White,			true,	"SpkExtinguishers");
		public static LayerStruct HeadCoverage				= new LayerStruct(	"Spk_HeadCoverage",					Turqoise,		false,	"SpkSystem_Head_Coverage");
		public static LayerStruct HeadLegend				= new LayerStruct(	"Spk_HeadLegend",					White,			true);
		public static LayerStruct Note						= new LayerStruct(	"Spk_Note",							White,			true,	"SpkNote");
		public static LayerStruct PersonalNote				= new LayerStruct(	"Spk_PersonalNote",					LightGreen,		true);

		public static LayerStruct PipeLabel					= new LayerStruct(	"Spk_PipeLabel",					White,			true,	"SpkPipeLabel");
		public static LayerStruct PipeLabel_Dia				= new LayerStruct(	"Spk_PipeLabel_Diamater",			White,			true,	"SpkPipeLabel_Dia");
		public static LayerStruct PipeLabel_Group			= new LayerStruct(	"Spk_PipeLabel_Group",				White,			true,	"SpkPipeLabel_Group");
		public static LayerStruct PipeLabel_Length			= new LayerStruct(	"Spk_PipeLabel_Length",				White,			true,	"SpkPipeLabel_Lgth");
		public static LayerStruct PipeLabel_Slope			= new LayerStruct(	"Spk_PipeLabel_Slope",				White,			true);

		public static LayerStruct Rev						= new LayerStruct(	"Spk_Rev",							DarkOrange,		true);

		public static LayerStruct SystemDevice				= new LayerStruct(	"Spk_System_Device",				DarkGrey,		true,	"SpkSystem_Device");
		public static LayerStruct SystemFitting				= new LayerStruct(	"Spk_System_Fitting",				Orange,			true,	"SpkSystem_Fitting");
		public static LayerStruct SystemHead				= new LayerStruct(	"Spk_System_Head",					LightRed,		true,	"SpkSystem_Head");

		public static LayerStruct SystemPipe_Armover		= new LayerStruct(	"Spk_System_Pipe_Armover",			DarkRed,		true,	"SpkPipe_Armover");
		public static LayerStruct SystemPipe_AuxDrain		= new LayerStruct(	"Spk_System_Pipe_AuxDrain",			DarkRed,		true,	"SpkPipe_AuxDrain");
		public static LayerStruct SystemPipe_Branchline		= new LayerStruct(	"Spk_System_Pipe_Branchline",		OrangeRed,		true,	"SpkPipe_Branchline");
		public static LayerStruct SystemPipe_Main			= new LayerStruct(	"Spk_System_Pipe_Main",				PinkRed,		true,	"SpkPipe_Main");

		public static LayerStruct Viewport					= new LayerStruct(	"Spk_Viewport",						White,			true,	"SpkViewport");
		public static LayerStruct Viewport_Hidden			= new LayerStruct(	"Spk_Viewport_Hidden",				DarkTurqoise,	true,	"SpkViewport_NoPlot");

		public static LayerStruct Wipeout					= new LayerStruct(	"Spk_Wipeout",						0,				true);

		public static LayerStruct XREF						= new LayerStruct(	"Spk_XREF",							LightTurqoise,	false,	"SpkXREF");
	}
}
