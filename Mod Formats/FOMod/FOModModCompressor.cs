using System;
using System.ComponentModel;
using System.IO;

namespace Nexus.Client.Mods.Formats.FOMod
{
	/// <summary>
	/// This class is subclassed to compress a source folder into a FOMod.
	/// </summary>
	public class FOModModCompressor : ModCompressorBase
	{
		#region Constructors

		/// <summary>
		/// A simple constructor that initializes with its dependencies.
		/// </summary>
		/// <param name="p_eifEnvironmentInfo">The application's envrionment info.</param>
		public FOModModCompressor(IEnvironmentInfo p_eifEnvironmentInfo)
			: base(p_eifEnvironmentInfo)
		{
		}

		#endregion

		/// <summary>
		/// Compresses the specified source folder into a mod file at the specified destination.
		/// </summary>
		/// <remarks>
		/// If the desitnation file exists, it will be overwritten.
		/// </remarks>
		/// <param name="p_strSourcePath">The folder to compress into a mod file.</param>
		/// <param name="p_strDestinationPath">The path of the mod file to create.</param>
		public override void Compress(string p_strSourcePath, string p_strDestinationPath)
		{
		}
	}
}
