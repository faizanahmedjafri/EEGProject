using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.Settings.Abstractions;

namespace biopot.Services
{
	public class AppSettingsManagerService : IAppSettingsManagerService
	{
		private readonly IStorageService _storageService;

		public AppSettingsManagerService(IStorageService storageService)
		{
			_storageService = storageService;
		}

		#region -- ISettingsManagerService implementation --

		public async Task InsertObjectAsync<T>(string key, T value)
		{
			await _storageService.SaveAsync<T>(key, value);
		}

		public async Task<T> GetObjectAsync<T>(string key)
		{
			try
			{
				return await _storageService.LoadAsync<T>(key);
			}
			catch (KeyNotFoundException ex)
			{
				return default(T);
			}
		}

		#endregion
	}
}
