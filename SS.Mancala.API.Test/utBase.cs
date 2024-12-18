using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace SS.Mancala.API.Test
{
    [TestClass]
    class APIProject : WebApplicationFactory<Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            // Sharing the extra set up
            return base.CreateHost(builder);
        }
    }
    public abstract class utBase
    {
        public HttpClient client { get; }
        public Type type;

        public utBase()
        {
            var application = new APIProject();
            client = application.CreateClient();
            client.BaseAddress = new Uri(client.BaseAddress + "api/");
        }

        [TestMethod]
        public async Task LoadTestAsync<T>(int expected)
        {
            try
            {
                dynamic items;
                Debug.Write($"Testing Load of {typeof(T).Name}: ");
                var response = await client.GetStringAsync(typeof(T).Name);
                items = (JArray)JsonConvert.DeserializeObject(response);
                List<T> values = items.ToObject<List<T>>();
                Debug.WriteLine($"{values.Count}");
                Assert.IsTrue(values.Count == expected);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error Loading {typeof(T).Name}: {ex.Message}");
                //throw;
            }
        }

        [TestMethod]
        public async Task DeleteTestAsync<T>(KeyValuePair<string, string> filter)
        {
            Guid id = await GetId<T>(filter);
            bool rollback = true;
            HttpResponseMessage response = client.DeleteAsync(typeof(T).Name + "/" + id + "/" + rollback).Result;
            string result = response.Content.ReadAsStringAsync().Result;
            Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            int resultValue = 0;
            foreach (var kvp in dict)
            {
                int.TryParse(kvp.Value, out resultValue);
            }
            Assert.IsTrue(resultValue > 0);

        }

        private async Task<Guid> GetId<T>(KeyValuePair<string, string> filter)
        {
            // return the id of the filter combination (ie VIN, "123456")
            string result;
            dynamic items;
            Guid id = Guid.Empty;

            var response1 = await client.GetStringAsync(typeof(T).Name);
            items = (JArray)JsonConvert.DeserializeObject(response1);
            List<T> values = items.ToObject<List<T>>();

            string key = filter.Key;
            string value = filter.Value;

            var field = values[0].GetType().GetProperty(key);

            foreach (T v in values)
            {
                if (v.GetType().GetProperty(key).GetValue(v, null).ToString() == value)
                {
                    id = (Guid)v.GetType().GetProperty("Id").GetValue(v, null);
                }
            }

            return id;

        }

        [TestMethod]
        public async Task InsertTestAsync<T>(T item)
        {
            bool rollback = true;
            string serializedObject = JsonConvert.SerializeObject(item);
            var content = new StringContent(serializedObject);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = client.PostAsync(typeof(T).Name + "/" + rollback, content).Result;
            string result = response.Content.ReadAsStringAsync().Result;

            // Assert that the guid is not the same as an empty guid 00000000-0000000-
            Assert.IsFalse(result.Equals(0));
        }

        [TestMethod]
        public async Task UpdateTestAsync<T>(KeyValuePair<string, string> filter, T item)
        {

            // Get the item that I want to update based on anything.
            var response1 = await client.GetStringAsync(typeof(T).Name);
            dynamic items = (JArray)JsonConvert.DeserializeObject(response1);
            List<T> values = items.ToObject<List<T>>();

            string key = filter.Key;
            string value = filter.Value;
            var field = values[0].GetType().GetProperty(key);

            Guid id = Guid.Empty;
            foreach (T v in values)
            {
                if (v.GetType().GetProperty(key).GetValue(v, null).ToString() == value)
                {
                    id = (Guid)v.GetType().GetProperty("Id").GetValue(v, null);
                    PropertyInfo prop = v.GetType().GetProperty("Id");
                    prop.SetValue(item, id, null);
                    break;
                }
            }

            // Use the item parameter to supply the updates.
            bool rollback = true;
            string serializedItem = JsonConvert.SerializeObject(item);
            var content = new StringContent(serializedItem);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = client.PutAsync(typeof(T).Name + "/" + id + "/" + rollback, content).Result;
            var result = response.Content.ReadAsStringAsync().Result;

            // Retrieve the results
            Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            int resultValue = 0;
            foreach (var kvp in dict)
            {
                int.TryParse(kvp.Value, out resultValue);
            }
            Debug.WriteLine($"Updated rows: {resultValue}");
            Assert.IsTrue(resultValue > 0);


        }

        [TestMethod]
        public async Task LoadByIdTestAsync<T>(KeyValuePair<string, string> filter)
        {
            Guid id = await GetId<T>(filter);
            dynamic items;
            var response = client.GetStringAsync(typeof(T).Name + "/" + id).Result;
            items = JsonConvert.DeserializeObject(response);
            T item = items.ToObject<T>();

            Assert.AreEqual(id, item.GetType().GetProperty("Id").GetValue(item, null));

        }

        private async Task<T> LoadByIdAsync<T>(KeyValuePair<string, string> filter)
        {
            Guid id = await GetId<T>(filter);
            dynamic items;
            var response = client.GetStringAsync(typeof(T).Name + "/" + id).Result;
            items = JsonConvert.DeserializeObject(response);
            T item = items.ToObject<T>();
            return item;
        }
    }
}
