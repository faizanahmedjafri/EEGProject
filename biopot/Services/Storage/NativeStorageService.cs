using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugin.Settings.Abstractions;

namespace biopot.Services
{
	public sealed class NativeStorageService : IStorageService
	{
		private readonly ISettings _settings;

		public NativeStorageService(ISettings settings)
		{
			_settings = settings;
		}

		public Task<T> LoadAsync<T>(string key)
		{

			string str = _settings.GetValueOrDefault(key, default(string));

			if (string.IsNullOrWhiteSpace(str))
			{
				return Task.FromResult(default(T));
			}
			else
			{
                try
                {
                    return Task.FromResult(JsonConvert.DeserializeObject<T>(str));
                }
                catch(Exception)
                {
                    return Task.FromResult(default(T));

                }
            }

		}

		public Task SaveAsync<T>(string key, T val)
		{
			_settings.AddOrUpdateValue(key, JsonConvert.SerializeObject(val));
			return Task.FromResult<bool>(true);
		}
	}
}
