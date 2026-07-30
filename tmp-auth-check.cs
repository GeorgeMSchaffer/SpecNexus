using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

var client = new HttpClient();
var payload = new { email = "siteadmin@sargentnexus.local", password = "Abc123!Demo" };
var response = await client.PostAsJsonAsync("http://127.0.0.1:5027/api/v1/auth/login", payload);
Console.WriteLine((int)response.StatusCode);
Console.WriteLine(await response.Content.ReadAsStringAsync());
