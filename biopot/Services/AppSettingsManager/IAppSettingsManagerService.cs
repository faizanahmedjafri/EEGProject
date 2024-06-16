using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace biopot.Services
{
	public interface IAppSettingsManagerService
	{
		Task<T> GetObjectAsync<T>(string key);
		Task InsertObjectAsync<T>(string key, T value);
	}
}