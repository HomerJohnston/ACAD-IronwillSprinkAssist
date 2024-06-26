﻿// (C) Copyright 2020 by  
//
using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using System.Reflection;
using Autodesk.AutoCAD.Colors;

using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using System.Runtime.InteropServices;

// This line is not mandatory, but improves loading performances
[assembly: ExtensionApplication(typeof(Ironwill.SprinkAssist))]

namespace Ironwill
{
	// This class is instantiated by AutoCAD once and kept alive for the 
	// duration of the session. If you don't do any one time initialization 
	// then you should remove this class.
	public partial class SprinkAssist : IExtensionApplication
	{
		public const string CommandMethodPrefix = "SpkAssist";

		void IExtensionApplication.Initialize()
		{
			// Add one time initialization here
			// One common scenario is to setup a callback function here that 
			// unmanaged code can call. 
			// To do this:
			// 1. Export a function from unmanaged code that takes a function
			//    pointer and stores the passed in value in a global variable.
			// 2. Call this exported function in this function passing delegate.
			// 3. When unmanaged code needs the services of this managed module
			//    you simply call acrxLoadApp() and by the time acrxLoadApp 
			//    returns  global function pointer is initialized to point to
			//    the C# delegate.
			// For more info see: 
			// http://msdn2.microsoft.com/en-US/library/5zwkzwf4(VS.80).aspx
			// http://msdn2.microsoft.com/en-us/library/44ey4b32(VS.80).aspx
			// http://msdn2.microsoft.com/en-US/library/7esfatk4.aspx
			// as well as some of the existing AutoCAD managed apps.

			// Initialize your plug-in application here
			//Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("Initializing SprinkAssist");
			DisplayVersion(null, null);
			
			Session.GetDocumentManager().DocumentCreated += DisplayVersion;
			Session.GetDocumentManager().DocumentCreated += DisplaySaveFidelity;

			Commands.SnapOverrule.SprinklerSnapOverruleCmd.Initialize();
			Commands.ToggleXrefLock.ToggleXrefLockCmd.Initialize();
		}

		void IExtensionApplication.Terminate()
		{
			// Do plug-in application clean up here
			Session.GetDocumentManager().DocumentCreated -= DisplayVersion;
			Session.GetDocumentManager().DocumentCreated -= DisplaySaveFidelity;
		}

		private void DisplayVersion(object sender, DocumentCollectionEventArgs e)
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			string assemblyName = AssemblyName.GetAssemblyName(assembly.Location).Version.ToString();

			//Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("Helloe Workd");
			Session.Log(
				"{0}" +
				"-------------------{0}" +
				"SprinkAssist Loaded{0}" +
				"Version: {1}{0}" +
				"-------------------{0}", Environment.NewLine, assemblyName);
		}

		private void DisplaySaveFidelity(object sender, DocumentCollectionEventArgs e)
		{
			short SAVEFIDELITY = (short)AcApplication.GetSystemVariable("SAVEFIDELITY");

			if (SAVEFIDELITY > 0)
			{
				Session.Log(
					"!!!!!!!!!! WARNING !!!!!!!!!!{0}" +
					"SAVEFIDELITY AutoCAD variable is not set to 0 for this drawing.{0}" +
					"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", Environment.NewLine
					);
			}
		}
	}
}
