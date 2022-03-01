using System;
using System.IO;
using System.Collections.Generic;

using System.Windows.Forms;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

using AcRx = Autodesk.AutoCAD.Runtime;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(Ironwill.MyCommands))]

namespace Ironwill
{
	// This class is instantiated by AutoCAD for each document when
	// a command is called by the user the first time in the context
	// of a given document. In other words, non static data in this class
	// is implicitly per-document!
	public class MyCommands
	{
		// The CommandMethod attribute can be applied to any public  member 
		// function of any public class.
		// The function should take no arguments and return nothing.
		// If the method is an intance member then the enclosing class is 
		// intantiated for each document. If the member is a static member then
		// the enclosing class is NOT intantiated.
		//
		// NOTE: CommandMethod has overloads where you can provide helpid and
		// context menu.

		// Modal Command with localized name
		[CommandMethod("MyGroup", "MyCommand", "MyCommandLocal", CommandFlags.Modal)]
		public void MyCommand() // This method can have any name
		{
			// Put your command code here
			Document doc = AcApplication.DocumentManager.MdiActiveDocument;
			Editor ed;
			if (doc != null)
			{
				ed = doc.Editor;
				ed.WriteMessage("Hello, this is your first command.");

			}
		}

		// Modal Command with pickfirst selection
		[CommandMethod("MyGroup", "MyPickFirst", "MyPickFirstLocal", CommandFlags.Modal | CommandFlags.UsePickSet)]
		public void MyPickFirst() // This method can have any name
		{
			Document doc = AcApplication.DocumentManager.MdiActiveDocument;
			Database database = doc.Database;
			Editor editor = doc.Editor;

			TypedValue[] filter = {
				new TypedValue((int)DxfCode.Operator, "<or"),
				new TypedValue((int)DxfCode.LayerName, "SpkPipe_Armover"),
				new TypedValue((int)DxfCode.LayerName, "SpkPipe_Branchline"),
				new TypedValue((int)DxfCode.LayerName, "SpkPipe_Main"),
				new TypedValue((int)DxfCode.LayerName, "SpkPipe_Drain"),
				new TypedValue((int)DxfCode.Operator, "or>"),
			};

			PromptSelectionResult result = editor.GetSelection(new SelectionFilter(filter));

			//PromptSelectionResult result = AcApplication.DocumentManager.MdiActiveDocument.Editor.GetSelection();
			if (result.Status == PromptStatus.OK)
			{
				editor.WriteMessage("There are selected entities");
				// Put your command using pickfirst set code here
			}
			else
			{
				editor.WriteMessage("There are no selected entities");
				// There are no selected entities
				// Put your command code here
			}
		}

		// Application Session Command with localized name
		[CommandMethod("MyGroup", "MySessionCmd", "MySessionCmdLocal", CommandFlags.Modal | CommandFlags.Session)]
		public void MySessionCmd() // This method can have any name
		{
			// Put your command code here
		}

		// LispFunction is similar to CommandMethod but it creates a lisp 
		// callable function. Many return types are supported not just string
		// or integer.
		[LispFunction("MyLispFunction", "MyLispFunctionLocal")]
		public int MyLispFunction(ResultBuffer args) // This method can have any name
		{
			// Put your command code here

			// Return a value to the AutoCAD Lisp Interpreter
			return 1;
		}


	}

}
