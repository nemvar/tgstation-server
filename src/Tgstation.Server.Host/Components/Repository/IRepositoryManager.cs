﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tgstation.Server.Host.Components.Repository
{
	/// <summary>
	/// Factory for creating and loading <see cref="IRepository"/>s
	/// </summary>
    public interface IRepositoryManager : IDisposable
	{
		/// <summary>
		/// Attempt to load the <see cref="IRepository"/> from the default location
		/// </summary>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> for the operation</param>
		/// <returns>The loaded <see cref="IRepository"/> if it exists, <see langword="null"/> otherwise</returns>
		Task<IRepository> LoadRepository(CancellationToken cancellationToken);

		/// <summary>
		/// Clone the repository at <paramref name="url"/>
		/// </summary>
		/// <param name="url">The <see cref="Uri"/> of the remote repository to clone</param>
		/// <param name="initialBranch">The branch to clone</param>
		/// <param name="accessString">The access string to clone from <paramref name="url"/></param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> for the operation</param>
		/// <returns>The newly cloned <see cref="IRepository"/>, <see langword="null"/> if one already exists</returns>
		Task<IRepository> CloneRepository(Uri url, string initialBranch, string accessString, CancellationToken cancellationToken);

		/// <summary>
		/// Delete the current repository
		/// </summary>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> for the operation</param>
		/// <returns>A <see cref="Task"/> representing the running operation</returns>
		Task DeleteRepository(CancellationToken cancellationToken);
    }
}
