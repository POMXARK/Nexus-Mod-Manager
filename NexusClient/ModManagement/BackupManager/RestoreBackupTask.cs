using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using Nexus.Client.BackgroundTasks;
using Nexus.Client.UI;
using Nexus.Client.Util;


namespace Nexus.Client.ModManagement
{
	public class RestoreBackupTask : ThreadedBackgroundTask
	{
		bool m_booAllowCancel = true;

		#region Fields
			
		private VirtualModActivator VirtualModActivator = null;
		private ConfirmActionMethod m_camConfirm = null;
		private ModManager ModManager = null;
		private ProfileManager ProfileManager = null;
		private IEnvironmentInfo EnvironmentInfo = null;
		private string BackupFile = string.Empty;
		private bool PurgeFolders = false;

		/// <summary>
		/// Gets or sets the file utility class.
		/// </summary>
		/// <value>The file utility class.</value>
		protected FileUtil FileUtility { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object with its dependencies.
		/// </summary>
		public RestoreBackupTask(VirtualModActivator p_vmaActivator, ModManager p_ModManager, ProfileManager p_pmProfileManager, IEnvironmentInfo p_EnvironmentInfo, string p_strBackupFile, bool p_booPurgeFolders, ConfirmActionMethod p_camConfirm)
		{
			m_camConfirm = p_camConfirm;
			VirtualModActivator = p_vmaActivator;
			ModManager = p_ModManager;
			EnvironmentInfo = p_EnvironmentInfo;
			BackupFile = p_strBackupFile;
			PurgeFolders = p_booPurgeFolders;
			ProfileManager = p_pmProfileManager;
		}

		#endregion

		#region Event Raising

		/// <summary>
		/// Raises the <see cref="IBackgroundTask.TaskEnded"/> event.
		/// </summary>
		/// <param name="e">A <see cref="TaskEndedEventArgs"/> describing the event arguments.</param>
		protected override void OnTaskEnded(TaskEndedEventArgs e)
		{
			base.OnTaskEnded(e);
		}
		#endregion

		/// <summary>
		/// Starts the update.
		/// </summary>
		/// <param name="p_camConfirm">The delegate to call to confirm an action.</param>
		public void Update(ConfirmActionMethod p_camConfirm)
		{
			Start(p_camConfirm);
		}

		/// <summary>
		/// Resumes the task.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the task is not paused.</exception>
		public override void Resume()
		{
			Update(m_camConfirm);
		}

		/// <summary>
		/// Cancels the update.
		/// </summary>
		public override void Cancel()
		{
			if (m_booAllowCancel)
				base.Cancel();
		}

		/// <summary>
		/// The method that is called to start the backgound task.
		/// </summary>
		/// <param name="args">Arguments to for the task execution.</param>
		/// <returns>Always <c>null</c>.</returns>
		protected override object DoWork(object[] args)
		{
			OverallMessage = "Restoring Nexus Mod Manager Backup...";
			OverallProgress = 0;
			OverallProgressStepSize = 1;
			ShowItemProgress = true;
			OverallProgressMaximum = 5;

			List<BackupInfo> lstLooseFiles = new List<BackupInfo>();
			List<BackupInfo> lstInstalledModFiles = new List<BackupInfo>();
			List<BackupInfo> lstInstalledNMMLINKFiles = new List<BackupInfo>();
			List<BackupInfo> lstBaseGameFiles = new List<BackupInfo>();
			List<BackupInfo> lstProfileFiles = new List<BackupInfo>();
			List<BackupInfo> lstModArchives = new List<BackupInfo>();
			List<BackupInfo> lstModCacheArchives = new List<BackupInfo>();

			ModProfile mprModProfile = null;

			string installLog = string.Empty;
			string BackupDirectory = string.Empty;
			string bkpArchive = BackupFile;
			
			if (!File.Exists(bkpArchive))
			{
				string strMessage = string.Format("The Backup file {0} is missing.", bkpArchive);
				DialogResult drFormClose = MessageBox.Show(strMessage, "NMM Backup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return null;
			}

			OverallMessage = "Checking the backup archive...";
			StepOverallProgress();
			
			StepOverallProgress();
				
			return mprModProfile;
		}

		/// <summary>
		/// The method that is called to restore the Backup files.
		/// </summary>
		private ModProfile RestoreBackupFiles(List<BackupInfo> p_lstLooseFiles, List<BackupInfo> p_lstInstalledModFiles, List<BackupInfo> p_lstInstalledNMMLINKFiles, List<BackupInfo> p_lstProfileFiles, List<BackupInfo> p_lstModArchives, List<BackupInfo> p_lstModCacheArchives, string p_strBackupDirectory, string p_strInstallLog)
		{
			string FileTo = string.Empty;
			string FileFrom = string.Empty;
			string VIRTUALINSTALLpath = VirtualModActivator.VirtualFoder;
			string NMMLINKpath = VirtualModActivator.HDLinkFolder;
			string ModArchivesPath = ModManager.GameMode.GameModeEnvironmentInfo.ModDirectory;
			string ModCacheArchivesPath = ModManager.GameMode.GameModeEnvironmentInfo.ModCacheDirectory;
			string dir = string.Empty;
			string ProfileId = string.Empty;
			int counter = 0;
			string ModInstallFolder = ModManager.GameMode.PluginDirectory;
			string ModInstallBackup = ModInstallFolder + "_oldbkp";
			string VirtualInstallBackup = VIRTUALINSTALLpath + "_oldbkp";
			bool PurgedModInstall = false;
			bool PurgedVirtualInstall = false;
			ModProfile mprModProfile = null;

			if (PurgeFolders)
			{
				if (p_lstLooseFiles.Count > 0)
					if (Directory.Exists(ModInstallFolder))
					{
						FileUtil.RenameDirectory(ModInstallFolder, ModInstallBackup);
						PurgedModInstall = true;
					}
				if (p_lstInstalledModFiles.Count > 0)
					if (Directory.Exists(VIRTUALINSTALLpath))
					{
						FileUtil.RenameDirectory(VIRTUALINSTALLpath, VirtualInstallBackup);
						PurgedVirtualInstall = true;
					}
			}

			try
			{
				if (p_lstLooseFiles.Count() > 0)
				{
					OverallProgressMaximum = p_lstLooseFiles.Count();

					if (!Directory.Exists(ModManager.GameMode.PluginDirectory))
						Directory.CreateDirectory(ModManager.GameMode.PluginDirectory);

					foreach (BackupInfo bkInfo in p_lstLooseFiles)
					{
						dir = Path.GetDirectoryName(Path.Combine(bkInfo.ModID, bkInfo.VirtualModPath));
						if (!string.IsNullOrEmpty(dir))
							Directory.CreateDirectory(Path.Combine(ModManager.GameMode.PluginDirectory, dir));

						FileFrom = bkInfo.RealModPath;
						FileTo = Path.Combine(ModManager.GameMode.PluginDirectory, bkInfo.ModID, bkInfo.VirtualModPath);

						File.Copy(FileFrom, FileTo, true);

						if (counter < p_lstLooseFiles.Count())
						{
							ItemMessage = bkInfo.VirtualModPath;
							StepItemProgress();
						}

						OverallMessage = string.Format("Copying the " + Path.GetFileName(ModManager.GameMode.PluginDirectory) + " Files...{0}/{1}", counter++, p_lstLooseFiles.Count());
						StepOverallProgress();
					}
				}

				OverallMessage = "Copying the VIRTUAL INSTALL Files.";
				StepOverallProgress();

				if (p_lstInstalledModFiles.Count() > 0)
				{
					OverallProgressMaximum = p_lstInstalledModFiles.Count();

					if (!Directory.Exists(VIRTUALINSTALLpath))
						Directory.CreateDirectory(VIRTUALINSTALLpath);

					counter = 0;
					foreach (BackupInfo bkInfo in p_lstInstalledModFiles)
					{
						dir = Path.GetDirectoryName(Path.Combine(bkInfo.ModID, bkInfo.VirtualModPath));
						if (!string.IsNullOrEmpty(dir))
						{
							if (Directory.Exists(Path.Combine(VIRTUALINSTALLpath, dir)))
								Directory.Delete(Path.Combine(VIRTUALINSTALLpath, dir), true);

							Directory.CreateDirectory(Path.Combine(VIRTUALINSTALLpath, dir));
						}

						FileFrom = bkInfo.RealModPath;
						FileTo = bkInfo.VirtualModPath == "VirtualModConfig.xml" ? Path.Combine(VIRTUALINSTALLpath, bkInfo.VirtualModPath) : Path.Combine(VIRTUALINSTALLpath, bkInfo.ModID, bkInfo.VirtualModPath);

						File.Copy(FileFrom, FileTo, true);

						if (counter < p_lstInstalledModFiles.Count())
						{
							ItemMessage = bkInfo.VirtualModPath;
							StepItemProgress();
						}

						OverallMessage = string.Format("Copying the VIRTUAL INSTALL Files...{0}/{1}", counter++, p_lstInstalledModFiles.Count());
						StepOverallProgress();
					}
				}

				OverallMessage = "Copying the NMMLINK Files.";
				StepOverallProgress();

				if (p_lstInstalledNMMLINKFiles.Count() > 0)
				{
					OverallProgressMaximum = p_lstInstalledNMMLINKFiles.Count();
					
					if (string.IsNullOrEmpty(NMMLINKpath))
						NMMLINKpath = VIRTUALINSTALLpath;

					if (!Directory.Exists(NMMLINKpath))
						Directory.CreateDirectory(NMMLINKpath);
					
					counter = 0;
					foreach (BackupInfo bkInfo in p_lstInstalledNMMLINKFiles)
					{
						if (ModManager.VirtualModActivator.MultiHDMode)
						{
							dir = Path.GetDirectoryName(Path.Combine(bkInfo.ModID, bkInfo.VirtualModPath));
							if (!string.IsNullOrEmpty(dir))
							{
								if (Directory.Exists(Path.Combine(NMMLINKpath, dir)))
									Directory.Delete(Path.Combine(NMMLINKpath, dir), true);

								Directory.CreateDirectory(Path.Combine(NMMLINKpath, dir));
							}
						}

						FileFrom = bkInfo.RealModPath;
						FileTo = Path.Combine(NMMLINKpath, bkInfo.VirtualModPath);

						File.Copy(FileFrom, FileTo, true);


						if (counter < p_lstInstalledNMMLINKFiles.Count())
						{
							ItemMessage = bkInfo.VirtualModPath;
							StepItemProgress();
						}

						OverallMessage = string.Format("Copying the NMMLINK Files...{0}/{1}", counter++, p_lstInstalledNMMLINKFiles.Count());
						StepOverallProgress();
					}
				}

				OverallMessage = "Copying the MOD ARCHIVES.";
				StepOverallProgress();

				if (p_lstModArchives.Count() > 0)
				{
					OverallProgressMaximum = p_lstModArchives.Count();

					if (!Directory.Exists(ModArchivesPath))
						Directory.CreateDirectory(ModArchivesPath);

					counter = 0;
					foreach (BackupInfo bkInfo in p_lstModArchives)
					{
						dir = Path.GetDirectoryName(Path.Combine(bkInfo.ModID, bkInfo.VirtualModPath));
						if (!string.IsNullOrEmpty(dir))
							Directory.CreateDirectory(Path.Combine(ModArchivesPath, dir));

						FileFrom = bkInfo.RealModPath;
						FileTo = Path.Combine(ModArchivesPath,bkInfo.VirtualModPath);

						File.Copy(FileFrom, FileTo, true);

						if (counter < p_lstModArchives.Count())
						{
							ItemMessage = bkInfo.VirtualModPath;
							StepItemProgress();
						}

						OverallMessage = string.Format("Copying the MOD ARCHIVES...{0}/{1}", counter++, p_lstModArchives.Count());
						StepOverallProgress();
					}
				}

				OverallMessage = "Copying the CACHE.";
				StepOverallProgress();

				if (p_lstModCacheArchives.Count() > 0)
				{
					OverallProgressMaximum = p_lstModCacheArchives.Count();

					if (!Directory.Exists(ModCacheArchivesPath))
						Directory.CreateDirectory(ModCacheArchivesPath);

					counter = 0;
					foreach (BackupInfo bkInfo in p_lstModCacheArchives)
					{
						dir = Path.GetDirectoryName(Path.Combine(bkInfo.ModID, bkInfo.VirtualModPath));
						if (!string.IsNullOrEmpty(dir))
							Directory.CreateDirectory(Path.Combine(ModCacheArchivesPath, dir));

						FileFrom = bkInfo.RealModPath;
						FileTo = Path.Combine(ModCacheArchivesPath, bkInfo.VirtualModPath);

						File.Copy(FileFrom, FileTo, true);

						if (counter < p_lstModCacheArchives.Count())
						{
							ItemMessage = bkInfo.VirtualModPath;
							StepItemProgress();
						}

						OverallMessage = string.Format("Copying the CACHE...{0}/{1}", counter++, p_lstModCacheArchives.Count());
						StepOverallProgress();
					}
				}

				if (p_lstProfileFiles.Count() > 0)
				{
					OverallProgressMaximum = p_lstProfileFiles.Count();

					if (!Directory.Exists(ProfileManager.ProfileManagerPath))
						Directory.CreateDirectory(ProfileManager.ProfileManagerPath);
					
					counter = 0;
					foreach (BackupInfo bkInfo in p_lstProfileFiles)
					{
						dir = Path.GetDirectoryName(Path.Combine(bkInfo.ModID, bkInfo.VirtualModPath));
						if (!string.IsNullOrEmpty(dir))
							Directory.CreateDirectory(Path.Combine(ProfileManager.ProfileManagerPath, dir));

						if (string.IsNullOrEmpty(ProfileId))
							ProfileId = dir;

						FileFrom = bkInfo.RealModPath;
						FileTo = Path.Combine(ProfileManager.ProfileManagerPath, bkInfo.ModID, bkInfo.VirtualModPath);

						File.Copy(FileFrom, FileTo, true);

						if (counter < p_lstProfileFiles.Count())
						{
							ItemMessage = bkInfo.VirtualModPath;
							StepItemProgress();
						}

						OverallMessage = string.Format("Copying the PROFILE Files...{0}/{1}", counter++, p_lstProfileFiles.Count());
						StepOverallProgress();
					}
				}
				
				string xmlProfilePath = Path.Combine(ProfileManager.ProfileManagerPath, ProfileId, "profile.xml");
				mprModProfile = CreateProfile(xmlProfilePath);

				if (!string.IsNullOrEmpty(p_strInstallLog))
				{
					File.Copy(p_strInstallLog, Path.Combine(ModManager.GameMode.GameModeEnvironmentInfo.InstallInfoDirectory, "InstallLog.xml"), true);
					ModManager.ReinitializeInstallLog(Path.Combine(ModManager.GameMode.GameModeEnvironmentInfo.InstallInfoDirectory, "InstallLog.xml"));
				}
								
				OverallMessage = "Deleting the leftovers.";
				StepOverallProgress();


				if (Directory.Exists(p_strBackupDirectory))
					FileUtil.ForceDelete(p_strBackupDirectory);

				if (Directory.Exists(ModInstallBackup))
					FileUtil.ForceDelete(ModInstallBackup);

				if (Directory.Exists(VirtualInstallBackup))
					FileUtil.ForceDelete(VirtualInstallBackup);
			}
			catch (Exception e)
			{
				if (PurgeFolders)
				{
					string ModInstallDelete = ModInstallFolder + "_DELETE";
					string VirtualInstallDelete = VIRTUALINSTALLpath + "_DELETE";

					if (PurgedModInstall)
					{
						if (Directory.Exists(ModInstallFolder))
							FileUtil.RenameDirectory(ModInstallFolder, ModInstallDelete);

						if (Directory.Exists(ModInstallBackup))
							FileUtil.RenameDirectory(ModInstallBackup, ModInstallFolder);

						if (Directory.Exists(ModInstallDelete))
							FileUtil.ForceDelete(ModInstallDelete);
					}

					if (PurgedVirtualInstall)
					{
						if (Directory.Exists(VIRTUALINSTALLpath))
							FileUtil.RenameDirectory(VIRTUALINSTALLpath, VirtualInstallDelete);

						if (Directory.Exists(VirtualInstallBackup))
							FileUtil.RenameDirectory(VirtualInstallBackup, VIRTUALINSTALLpath);

						if (Directory.Exists(VirtualInstallDelete))
							FileUtil.ForceDelete(VirtualInstallDelete);
					}
				}

				string ErrorMessage = string.Format("Exception: {0}" + Environment.NewLine + Environment.NewLine + "From: {1}" + Environment.NewLine + "To: {2}", e.Message, FileFrom, FileTo);

				return null;
			}

			return mprModProfile;
		}

		/// <summary>
		/// Creates the new profile.
		/// </summary>
		private ModProfile CreateProfile(string p_strXmlProfilePath)
		{
			Int32 intModCount = 0;
			string strProfileId = string.Empty;
			string strProfileName = string.Empty;
			string strGameModeId = string.Empty;
			
			if (File.Exists(p_strXmlProfilePath))
			{
				XDocument docProfile = XDocument.Load(p_strXmlProfilePath);

				try
				{

					XElement xelProfile = docProfile.Descendants("profile").FirstOrDefault();
					intModCount = Int32.TryParse(xelProfile.Element("modCount").Value, out intModCount) ? intModCount : 0;
					strProfileId = xelProfile.Attribute("profileId").Value;
					strProfileName = xelProfile.Attribute("profileName").Value;
					strGameModeId = xelProfile.Element("gameModeId").Value;
					
				}
				catch { }
			}
			
			ModProfile mprModProfile = new ModProfile(strProfileId, strProfileName, strGameModeId, intModCount);
			mprModProfile.IsDefault = true;
			ProfileManager.ModProfiles.Add(mprModProfile);
			ProfileManager.LoadProfileFileList(mprModProfile);
			ProfileManager.SaveConfig();
			return mprModProfile;
		}

		private Int32 ImportedProfileModCount(string p_strXMLFilePath)
		{
			Int32 intModCount = 0;

			if (File.Exists(p_strXMLFilePath))
			{
				XDocument docProfile = XDocument.Load(p_strXMLFilePath);

				try
				{
					XElement xelProfile = docProfile.Descendants("profile").FirstOrDefault();
					intModCount = Int32.TryParse(xelProfile.Element("modCount").Value, out intModCount) ? intModCount : 0;
				}
				catch { }
			}

			return intModCount;
		}
	}
}
