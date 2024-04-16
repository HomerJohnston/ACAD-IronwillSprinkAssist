using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Reflection;
using System.Collections.Specialized;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using System.Security.Cryptography.X509Certificates;

[assembly: CommandClass(typeof(Ironwill.Commands.Help.HelpCmd))]


namespace Ironwill.Commands.Help
{
	internal struct CommandInfo: IComparable<CommandInfo>
	{
		internal CommandMethodAttribute methodAttribute;
		internal CommandDescription description;

		public CommandInfo(CommandMethodAttribute methodAttribute, CommandDescription description)
		{
			this.methodAttribute = methodAttribute;
			this.description = description;
		}

		public int CompareTo(CommandInfo other)
		{
			return methodAttribute.GlobalName.CompareTo(other.methodAttribute.GlobalName);
		}
	}

	internal class HelpCmd
	{
		// TODO: in debug mode list which commands don't have help descriptions added
		[CommandDescription("Lists all SprinkAssist commands.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "ListSprinkAssistCommands", CommandFlags.NoBlockEditor)]
		static public void ListSprinkAssistCommands()
		{
			// Just get the commands for this assembly
			Editor editor = Session.GetEditor();
			Assembly assembly = Assembly.GetExecutingAssembly();

			string[] commands = GetCommands(assembly);

			foreach (string cmd in commands)
			{
				editor.WriteMessage(cmd + "\n");
			}
		}

		private static string[] GetCommands(Assembly asm)
		{
			StringCollection stringCollection = new StringCollection
			{
				$"\n\n-----------------------------------------",
				$"           START COMMANDS LIST",
				$"-----------------------------------------\n"
			};

			List<Type> types;

			object[] commandClassAttributes = asm.GetCustomAttributes(typeof(CommandClassAttribute), true);
			int numTypes = commandClassAttributes.Length;
			
			if (numTypes > 0)
			{
				types = new List<Type>(numTypes);
				
				for (int i = 0; i < numTypes; i++)
				{
					CommandClassAttribute commandClassAttr = commandClassAttributes[i] as CommandClassAttribute;

					if (commandClassAttr != null)
					{
						types.Add(commandClassAttr.Type);
					}
				}
			}
			else
			{
				// If we're only looking for specifically
				// marked CommandClasses, then use an
				// empty list ??????????????????

				types = new List<Type>(asm.GetExportedTypes());
			}

			//types.Sort((x1, x2) => { return x1.Name.CompareTo(x2.Name); });

			List<MethodInfo> methodInfos = new List<MethodInfo>();

			foreach (Type type in types)
			{
				if (type == null)
				{
					continue;
				}

				MethodInfo[] methods = type.GetMethods();

				foreach (MethodInfo method in methods)
				{
					if (method.GetCustomAttributes<CommandMethodAttribute>(true).Count() == 0)
					{
						continue;
					}

					if (method != null)
					{
						methodInfos.Add(method);
					}
				}
			}

			methodInfos.Sort((m1, m2) => { return m1.GetCustomAttributes<CommandMethodAttribute>(true).First().GlobalName.CompareTo(m2.GetCustomAttributes<CommandMethodAttribute>(true).First().GlobalName); });

			foreach (MethodInfo method in methodInfos)
			{
				IEnumerable<CommandMethodAttribute> commandMethods = method.GetCustomAttributes<CommandMethodAttribute>(true);
				IEnumerable<CommandDescription> commandDescriptions = method.GetCustomAttributes<CommandDescription>(true);

				string description = string.Empty;
				List<string> bulletPoints = new List<string>();

				int methodCount = commandMethods.Count();
				int descCount = commandDescriptions.Count();
					
				if (methodCount > 0 && descCount == 0)
				{
					Session.LogDebug($"WARNING: No description provided for command {method.Name} in {method.DeclaringType}");
				}
				else if (methodCount > 0 && descCount > 1)
				{
					Session.Log($"WARNING: Multiple CommandMethodDescription attributes on command {method.Name} in {method.DeclaringType}! Only last one will be used!");
				}

				foreach (CommandDescription commandDescription in commandDescriptions)
				{
					description = commandDescription.Description;

					if (!description.EndsWith("."))
					{
						Session.LogDebug($"Command {method.Name} in {method.DeclaringType} has help info missing an ending period!");
					}

					bulletPoints.Clear();

					foreach (string s in commandDescription.BulletPoints)
					{
						bulletPoints.Add(s);

						if (!s.EndsWith("."))
						{
							Session.LogDebug($"Command {method.Name} in {method.DeclaringType} has help info missing an ending period!");
						}
					}
				}

				if (description == string.Empty)
				{
					if (bulletPoints.Count > 0)
					{
						Session.Log($"WARNING: Command {method.Name} in {method.DeclaringType} had bullet point info, but no description!");
					}

					continue;
				}

				foreach (CommandMethodAttribute commandMethod in commandMethods) 
				{
					stringCollection.Add(commandMethod.GlobalName.ToUpper() + ": " + description);

					foreach (string s in bulletPoints)
					{
						const string bullet = "    \u2022 ";
						stringCollection.Add(bullet + s);
					}
				}

				stringCollection.Add("");

				for (int i = 0; i < 1 - bulletPoints.Count; i++)
				{
					stringCollection.Add("");
				}
			}

			stringCollection.Add($"-----------------------------------------");
			stringCollection.Add($"            END COMMANDS LIST");
			stringCollection.Add($"-----------------------------------------\n");

			if (stringCollection.Count > 400)
			{
				// TODO: figure out how to read this and display this only if it's relevant
				stringCollection.Add("Warning: line count over default limit of 400, run (setenv \"cmdhistlines\" \"5000\") in the command line to increase limit if commands are cut off");
			}

			stringCollection.Add($"Note: all commands start with " + SprinkAssist.CommandMethodPrefix.ToUpper() + " prefix (full name is: " + SprinkAssist.CommandMethodPrefix.ToUpper() + ".COMMANDNAME)");

			string[] ret = new string[stringCollection.Count];
			stringCollection.CopyTo(ret, 0);
			return ret;
		}
	}
}