using CloudantDb.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Net;
using System.Net.Http.Headers;
using UtilitiesMaster.ExtMethods.ExtString;

namespace CloudantDb.Services
{
	public class DbService : IDisposable
	{
		private readonly HttpClient client;
		private const string baseAddress = "https://{0}.cloudant.com/{1}/";
		private const string viewPath = "_design/{0}/_view/{1}";

		public DbService(string account, string dbName, string authValue)
		{
			client = new HttpClient
			{
				BaseAddress = new Uri(String.Format(baseAddress, account, dbName))
			};
			client.DefaultRequestHeaders.Add("Authorization", authValue);
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		}

		public async Task<string[]> GetIdsForKey(string designName, string viewName, string key)
		{
			string url = String.Format(viewPath, designName, viewName);
			string js;

			if (key.IsNotEmpty())
				url += "?key=" + System.Net.WebUtility.UrlEncode(@"""" + key + @"""");

			using (var res = await client.GetAsync(url).ConfigureAwait(false))
			{
				res.EnsureSuccessStatusCode();
				js = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
			}

			JObject o = JObject.Parse(js);
			return o["rows"]!.Select(a => a["id"]!.ToString()).ToArray();
		}

		public async Task<List<(string key, string val)>> GetViewKeysValues(string designName, string viewName, bool isGrouped = false)
		{
			string url = String.Format(viewPath, designName, viewName) + (isGrouped ? "?group=true" : "");
			string js;

			using (var res = await client.GetAsync(url).ConfigureAwait(false))
			{
				res.EnsureSuccessStatusCode();
				js = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
			}

			JObject o = JObject.Parse(js);
			return o["rows"]!.Select(a => (a["key"]!.ToString(), a["value"]!.ToString())).ToList();
		}

		public async Task<List<(string id, string val)>> GetViewIdsValues(string designName, string viewName)
		{
			string url = String.Format(viewPath, designName, viewName);
			string js;

			using (var res = await client.GetAsync(url).ConfigureAwait(false))
			{
				res.EnsureSuccessStatusCode();
				js = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
			}

			JObject o = JObject.Parse(js);
			return o["rows"]!.Select(a => (a["id"]!.ToString(), a["value"]!.ToString())).ToList();
		}

		public async Task<string> GetViewJsonAsync(string designName, string viewName, QueryParams queryParams)
		{
			string url = String.Format(viewPath, designName, viewName);

			if (queryParams != null)
				url += "?" + queryParams.RenderParamString();

			using (var res = await client.GetAsync(url).ConfigureAwait(false))
			{
				res.EnsureSuccessStatusCode();
				return await res.Content.ReadAsStringAsync().ConfigureAwait(false);
			}
		}

		public async Task<List<T>> GetViewItemsAsync<T>(string designName, string viewName, QueryParams queryParams) where T : ICloudantObj
		{
			string url = String.Format(viewPath, designName, viewName);

			if (queryParams != null)
				url += "?" + queryParams.RenderParamString();

			string js;

			using (var res = await client.GetAsync(url).ConfigureAwait(false))
			{
				res.EnsureSuccessStatusCode();
				js = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
			}

			JObject o = JObject.Parse(js);
			JArray ja = new JArray(((JArray)o["rows"]!).Select(a => a["doc"]));

			return ja.ToObject<List<T>>()!;
		}

		public async Task<Dictionary<string, string>> GetViewDictionaryAsync(string designName, string viewName, QueryParams queryParams)
		{
			string url = String.Format(viewPath, designName, viewName);

			if (queryParams != null)
				url += "?" + queryParams.RenderParamString();

			string js;

			using (var res = await client.GetAsync(url).ConfigureAwait(false))
			{
				res.EnsureSuccessStatusCode();
				js = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
			}

			JObject o = JObject.Parse(js);
			return ((JArray)o["rows"]!).Select(a => ExtractProp(a)).ToDictionary(a => a.Key, b => b.Value);


			KeyValuePair<string,string> ExtractProp(JToken jt)
			{
				var prop = ((JObject)jt["value"]!).Properties().ToArray()[0];
				return new KeyValuePair<string, string>(prop.Name, prop.Value.ToString());
			}
		}

		public async Task<List<T>> GetViewItemsAsync<T>(string designName, string viewName, IEnumerable<string> keys) where T : ICloudantObj
		{
			string url = String.Format(viewPath, designName, viewName);
			url += "?include_docs=true";

			string requestJson = JsonConvert.SerializeObject(new { keys = keys.ToArray() }, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver(), Formatting = Formatting.None, NullValueHandling = NullValueHandling.Ignore });
			string js;

			using (var msg = new HttpRequestMessage(HttpMethod.Post, url))
			using (var content = new StringContent(requestJson))
			{
				content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
				msg.Content = content;

				using (var res = await client.SendAsync(msg).ConfigureAwait(false))
				{
					res.EnsureSuccessStatusCode();
					js = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
				}
			}

			JObject o = JObject.Parse(js);
			JArray ja = new JArray(((JArray)o["rows"]!).Select(a => a["doc"]));

			return ja.ToObject<List<T>>()!;
		}

		public async Task<T> GetItemAsync<T>(string id) where T : ICloudantObj
		{
			string url = id;
			string js;

			using (var res = await client.GetAsync(url).ConfigureAwait(false))
			{
				if (res.StatusCode == HttpStatusCode.NotFound)
					return default(T)!;

				if ((int)res.StatusCode > 299)
					throw new HttpRequestException(res.StatusCode.ToString());

				js = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
			}

			JObject o = JObject.Parse(js);
			return o.ToObject<T>()!;
		}

		public async Task<List<T>> GetItemsAsync<T>(IEnumerable<string> ids) where T : ICloudantObj
		{
			string url = "_all_docs?include_docs=true";

			string requestJson = JsonConvert.SerializeObject(new { keys = ids.ToArray() }, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver(), Formatting = Formatting.None, NullValueHandling = NullValueHandling.Ignore });
			string js;

			using (var msg = new HttpRequestMessage(HttpMethod.Post, url))
			using (var content = new StringContent(requestJson))
			{
				content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
				msg.Content = content;

				using (var res = await client.SendAsync(msg).ConfigureAwait(false))
				{
					res.EnsureSuccessStatusCode();
					js = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
				}
			}

			JObject o = JObject.Parse(js);
			JArray ja = new JArray(((JArray)o["rows"]!).Select(a => a["doc"]));

			return ja.ToObject<List<T>>()!;
		}


		public async Task<string> CreateItemAsync(string itemJson)
		{
			using (var msg = new HttpRequestMessage(HttpMethod.Post, ""))
			using (var content = new StringContent(itemJson))
			{
				content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
				msg.Content = content;

				using (var res = await client.SendAsync(msg).ConfigureAwait(false))
				{
					res.EnsureSuccessStatusCode();
					return await res.Content.ReadAsStringAsync().ConfigureAwait(false);
				}
			}
		}

		public async Task<T> CreateItemAsync<T>(T item) where T : ICloudantObj
		{
			// updates _id and _rev of item.

			string itemJson = JsonConvert.SerializeObject(item, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver(), Formatting = Formatting.None, NullValueHandling = NullValueHandling.Ignore });
			string resp;

			using (var msg = new HttpRequestMessage(HttpMethod.Post, ""))
			using (var content = new StringContent(itemJson))
			{
				content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
				msg.Content = content;

				using (var res = await client.SendAsync(msg).ConfigureAwait(false))
				{
					res.EnsureSuccessStatusCode();
					resp = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
				}
			}

			JObject o = JObject.Parse(resp);
			item._id = o["id"]!.ToString();
			item._rev = o["rev"]!.ToString();
			return item;
		}


		public async Task<string> UpdateItemAsync(string id, string itemJson)
		{
			using (var msg = new HttpRequestMessage(HttpMethod.Put, id))
			using (var content = new StringContent(itemJson))
			{
				content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
				msg.Content = content;

				using (var res = await client.SendAsync(msg).ConfigureAwait(false))
				{
					res.EnsureSuccessStatusCode();
					return await res.Content.ReadAsStringAsync().ConfigureAwait(false);
				}
			}
		}

		public async Task<T> UpdateItemAsync<T>(T item, bool ignoreConflict = false) where T : ICloudantObj
		{
			if (ignoreConflict)
			{
				string js;

				using (var res = await client.GetAsync(item._id).ConfigureAwait(false))
				{
					res.EnsureSuccessStatusCode();
					js = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
				}

				item._rev = (JObject.Parse(js))["_rev"]!.ToString();
			}

			string itemJson = JsonConvert.SerializeObject(item, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver(), Formatting = Formatting.None, NullValueHandling = NullValueHandling.Ignore });
			string resp;

			using (var msg = new HttpRequestMessage(HttpMethod.Put, item._id))
			using (var content = new StringContent(itemJson))
			{
				content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
				msg.Content = content;

				using (var res = await client.SendAsync(msg).ConfigureAwait(false))
				{
					res.EnsureSuccessStatusCode();
					resp = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
				}
			}

			JObject o = JObject.Parse(resp);
			item._rev = o["rev"]!.ToString();
			return item;
		}


		public async Task<string> DeleteItemAsync(string id, string rev)
		{
			using (var res = await client.DeleteAsync($"{id}?rev={rev}").ConfigureAwait(false))
			{
				res.EnsureSuccessStatusCode();
				return await res.Content.ReadAsStringAsync().ConfigureAwait(false);
			}
		}


		public async Task<string> SaveItemBatchAsync(string itemArrayJson)
		{
			var obj = new JObject(
				new JProperty("docs", JArray.Parse(itemArrayJson))
			);

			using (var msg = new HttpRequestMessage(HttpMethod.Post, "_bulk_docs"))
			using (var content = new StringContent(obj.ToString()))
			{
				content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
				msg.Content = content;

				using (var res = await client.SendAsync(msg).ConfigureAwait(false))
				{
					res.EnsureSuccessStatusCode();
					return await res.Content.ReadAsStringAsync().ConfigureAwait(false);
				}
			}
		}

		public async Task<string> SaveItemAsync(ICloudantObj item)
		{
			return await SaveItemBatchAsync(new List<ICloudantObj> { item }).ConfigureAwait(false);
		}

		public async Task<string> SaveItemBatchAsync(IEnumerable<ICloudantObj> items)
		{
			var itemArrayJson = JsonConvert.SerializeObject(items, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver(), Formatting = Formatting.None, NullValueHandling = NullValueHandling.Ignore });
			return await SaveItemBatchAsync(itemArrayJson).ConfigureAwait(false);
		}


		// IDisposable Implementation

		bool _disposed = false;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~DbService()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				// free other managed objects that implement IDisposable only
				client.Dispose();
			}

			// release any unmanaged objects
			// set the object references to null

			_disposed = true;
		}

	}
}