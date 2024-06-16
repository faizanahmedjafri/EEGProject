using System;
using System.Threading.Tasks;

namespace biopot.Services
{
	public interface IStorageService
	{
		Task<T> LoadAsync<T>(string key);
		Task SaveAsync<T>(string key, T val);
	}
}
