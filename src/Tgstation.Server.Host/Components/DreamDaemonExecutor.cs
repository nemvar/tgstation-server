﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Tgstation.Server.Api.Models;
using Tgstation.Server.Api.Models.Internal;
using Tgstation.Server.Host.Core;

namespace Tgstation.Server.Host.Components
{
	/// <inheritdoc />
	sealed class DreamDaemonExecutor : IDreamDaemonExecutor
	{
		/// <summary>
		/// Change a given <paramref name="securityLevel"/> into the appropriate DreamDaemon command line word
		/// </summary>
		/// <param name="securityLevel">The <see cref="DreamDaemonSecurity"/> level to change</param>
		/// <returns>A <see cref="string"/> representation of the command line parameter</returns>
		static string SecurityWord(DreamDaemonSecurity securityLevel)
		{
			switch (securityLevel)
			{
				case DreamDaemonSecurity.Safe:
					return "safe";
				case DreamDaemonSecurity.Trusted:
					return "trusted";
				case DreamDaemonSecurity.Ultrasafe:
					return "ultrasafe";
				default:
					throw new ArgumentOutOfRangeException(nameof(securityLevel), securityLevel, String.Format(CultureInfo.InvariantCulture, "Bad DreamDaemon security level: {0}", securityLevel));
			}
		}

		/// <summary>
		/// The <see cref="IInstanceShutdownMethod"/> for the <see cref="DreamDaemonExecutor"/>
		/// </summary>
		readonly IInstanceShutdownMethod instanceShutdownMethod;
		/// <summary>
		/// The <see cref="IIOManager"/> for the <see cref="DreamDaemonExecutor"/>
		/// </summary>
		readonly IIOManager ioManager;

		/// <summary>
		/// Construct a <see cref="DreamDaemonExecutor"/>
		/// </summary>
		/// <param name="instanceShutdownMethod">The value of <see cref="instanceShutdownMethod"/></param>
		/// <param name="ioManager">The value of <see cref="ioManager"/></param>
		public DreamDaemonExecutor(IInstanceShutdownMethod instanceShutdownMethod, IIOManager ioManager)
		{
			this.instanceShutdownMethod = instanceShutdownMethod ?? throw new ArgumentNullException(nameof(instanceShutdownMethod));
			this.ioManager = ioManager ?? throw new ArgumentNullException(nameof(ioManager));
		}

		/// <inheritdoc />
		public async Task<int> RunDreamDaemon(DreamDaemonLaunchParameters launchParameters, TaskCompletionSource<object> onSuccessfulStartup, string dreamDaemonPath, string dmbPath, string accessToken, bool usePrimaryPort, CancellationToken cancellationToken)
		{
			using (var proc = new Process())
			{
				proc.StartInfo.FileName = dreamDaemonPath;
				proc.StartInfo.WorkingDirectory = ioManager.GetDirectoryName(dmbPath);
				proc.StartInfo.Arguments = String.Format(CultureInfo.InvariantCulture, "{0} -port {1} {2}-close -{3} -verbose -public -params \"{4}={5}&{6}={7}&{8}={9}&{10}={11}&{12}={13}\"", dmbPath, usePrimaryPort ? launchParameters.PrimaryPort : launchParameters.SecondaryPort, launchParameters.AllowWebClient ? "-webclient " : String.Empty, SecurityWord(launchParameters.SecurityLevel),
					DreamDaemonParameters.AccessToken, accessToken,
					DreamDaemonParameters.HostVersion, Application.Version,
					DreamDaemonParameters.IsSecondaryServer, !usePrimaryPort,
					DreamDaemonParameters.PrimaryPort, launchParameters.PrimaryPort,
					DreamDaemonParameters.SecondaryPort, launchParameters.SecondaryPort);

				proc.EnableRaisingEvents = true;
				var tcs = new TaskCompletionSource<object>();
				proc.Exited += (a, b) => tcs.SetResult(null);

				try
				{
					proc.Start();

					await Task.Factory.StartNew(() => proc.WaitForInputIdle(), cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Current).ConfigureAwait(false);

					if (onSuccessfulStartup != null)
					{
						onSuccessfulStartup.SetResult(null);
						onSuccessfulStartup = null;
					}

					try
					{
						using (cancellationToken.Register(() => tcs.SetCanceled()))
							await tcs.Task.ConfigureAwait(false);
					}
					finally
					{
						if (!instanceShutdownMethod.GracefulShutdown)
						{
							proc.Kill();
							proc.WaitForExit();
						}
					}

					return proc.ExitCode;
				}
				catch (Exception e)
				{
					onSuccessfulStartup.SetException(e);
					throw;
				}
			}
		}

	}
}
