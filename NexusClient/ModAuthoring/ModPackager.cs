using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Nexus.Client.BackgroundTasks;
using Nexus.UI.Controls;
using Nexus.Client.ModManagement.Scripting;
using Nexus.Client.Mods;
using Nexus.Client.Util;

namespace Nexus.Client.ModAuthoring
{
	/// <summary>
	/// This builds a mod file from a mod <see cref="Project"/>.
	/// </summary>
	public class ModPackager : ThreadedBackgroundTask
	{
		#region Properties

		/// <summary>
		/// Gets the file utility class.
		/// </summary>
		/// <value>The file utility class.</value>
		protected FileUtil FileUtilities { get; private set; }

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object with its dependencies.
		/// </summary>
		/// <param name="p_futFileUtilities">The file utility class.</param>
		public ModPackager(FileUtil p_futFileUtilities)
		{
			FileUtilities = p_futFileUtilities;
		}

		#endregion

		/// <summary>
		/// Creates a mod file at the specified location from the given mod <see cref="Project"/>.
		/// </summary>
		/// <param name="p_strFileName">The path of the mod file to build.</param>
		/// <param name="p_prjModProject">The <see cref="Project"/> describing the mod to be built.</param>
		public void PackageMod(string p_strFileName, Project p_prjModProject)
		{
			/*
			 * 1) Create file dictionary
			 * 2) Create info.xml
			 * 3) Create screenshot
			 * 4) Create readme
			 * 5) Create XML Script
			 * 6) Pack mod
			 * 7) Clean up - this step doesn't count
			 * 
			 * Total steps	= 1 + 1 + 1 + 1 + 1 + 1
			 *				= 6
			 */
			OverallProgressMaximum = 6;
			OverallMessage = "Packing Mod...";
			OverallProgressStepSize = 1;
			ItemProgressStepSize = 1;
			ShowItemProgress = true;

			Start(p_strFileName, p_prjModProject);
		}

		/// <summary>
		/// Performs the actual mod preparation work.
		/// </summary>
		/// <param name="args">The task arguments.</param>
		/// <returns>Always <c>null</c>.</returns>
		protected override object DoWork(object[] args)
		{
			string strFileName = (string)args[0];
			Project prjModProject = (Project)args[1];
			/*
			 * 1) Create file dictionary
			 * 2) Create info.xml
			 * 3) Create screenshot
			 * 4) Create readme
			 * 5) Create XML Script
			 * 6) Pack mod
			 * 7) Clean up
			 */

			string strTmpDirectory = null;
			try
			{
				ItemMessage = "Finding Mod Files...";
				ItemProgressMaximum = prjModProject.ModFiles.Count;
				ItemProgress = 0;
				Dictionary<string, string> dicFiles = new Dictionary<string, string>();
				foreach (VirtualFileSystemItem vfiItem in prjModProject.ModFiles)
				{
					StepItemProgress();
					if (vfiItem.IsDirectory)
						continue;
					dicFiles[vfiItem.Path] = vfiItem.Source;
				}

				StepOverallProgress();
				if (Status == TaskStatus.Cancelling)
					return null;

				strTmpDirectory = FileUtilities.CreateTempDirectory();

				ItemMessage = "Generating Info File...";
				ItemProgressMaximum = 1;
				ItemProgress = 0;
				string strInfoFilePath = Path.Combine(strTmpDirectory, "info.xml");
				XmlDocument xmlInfo = new XmlDocument();
				xmlInfo.AppendChild(prjModProject.SaveInfo(xmlInfo, false));
				xmlInfo.Save(strInfoFilePath);
				dicFiles[Path.Combine(NexusMod.MetaFolder, "info.xml")] = strInfoFilePath;

				StepOverallProgress();
				if (Status == TaskStatus.Cancelling)
					return null;

				if (prjModProject.Screenshot != null)
				{
					ItemMessage = "Generating Screenshot...";
					ItemProgressMaximum = 1;
					ItemProgress = 0;

					string strScreenshotPath = Path.Combine(strTmpDirectory, "screenshot.jpg");
					strScreenshotPath = Path.ChangeExtension(strScreenshotPath, prjModProject.Screenshot.GetExtension());
					File.WriteAllBytes(strScreenshotPath, prjModProject.Screenshot.Data);
					dicFiles[Path.Combine(NexusMod.MetaFolder, Path.GetFileName(strScreenshotPath))] = strScreenshotPath;

					StepOverallProgress();
					if (Status == TaskStatus.Cancelling)
						return null;
				}

				if (prjModProject.ModReadme != null)
				{
					ItemMessage = "Generating Readme...";
					ItemProgressMaximum = 1;
					ItemProgress = 0;

					string strReadmePath = Path.Combine(strTmpDirectory, "readme.txt");
					strReadmePath = Path.ChangeExtension(strReadmePath, prjModProject.ModReadme.Extension);
					File.WriteAllText(strReadmePath, prjModProject.ModReadme.Text);
					dicFiles[Path.Combine(NexusMod.MetaFolder, Path.GetFileName(strReadmePath))] = strReadmePath;

					StepOverallProgress();
					if (Status == TaskStatus.Cancelling)
						return null;
				}

				if (prjModProject.InstallScript != null)
				{
					ItemMessage = "Generating Install Script...";
					ItemProgressMaximum = 1;
					ItemProgress = 0;

					XDocument xmlScript = new XDocument();
					IScriptType stpType = prjModProject.InstallScript.Type;
					XElement xelScript = XElement.Parse(stpType.SaveScript(prjModProject.InstallScript));
					xmlScript.Add(xelScript);

					string strScriptPath = Path.Combine(strTmpDirectory, stpType.FileNames[0]);
					xmlScript.Save(strScriptPath);
					dicFiles[Path.Combine(NexusMod.MetaFolder, stpType.FileNames[0])] = strScriptPath;

					StepOverallProgress();
					if (Status == TaskStatus.Cancelling)
						return null;
				}

				ItemMessage = "Compressing Files...";
				ItemProgressMaximum = dicFiles.Count;
				ItemProgress = 0;
				
			}
			finally
			{
				if (!String.IsNullOrEmpty(strTmpDirectory))
				{
					//this try/catch is just in case the GC doesn't go as expected
					// and szcCompressor didn't release its resources
					try
					{
						FileUtil.ForceDelete(strTmpDirectory);
					}
					catch (IOException)
					{
					}
				}
			}
			return null;
		}
		
		/// <summary>
		/// Called when a file has been added to a new mod.
		/// </summary>
		/// <remarks>
		/// This steps the progress of the task.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void Compressor_FileCompressionFinished(object sender, EventArgs e)
		{
			StepItemProgress();
		}
	}
}
