using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using SkinSelfie.AppModels;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SkinSelfie
{
    public static class NetworkUtils
    {
        private static HttpClient CreateClient()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(ConfidentialData.SiteUrl);

            if (App.user != null && App.user.Token != null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.user.Token.access_token);
            }

            return client;
        }

        public static async Task<ServerResponse<T>> GetRequest<T>(string route)
        {
            HttpClient client = CreateClient();
            ServerResponse<T> toReturn = new ServerResponse<T>();

            try
            {
                toReturn.Response = await client.GetAsync(route);
                string jsonString = await toReturn.Response.Content.ReadAsStringAsync();

                if (toReturn.Response.IsSuccessStatusCode)
                {
                    if (typeof(T) == typeof(string))
                    {
                        toReturn.Data = (T)(object)jsonString;
                    }

                    try
                    {
                        toReturn.Data = JsonConvert.DeserializeObject<T>(jsonString);
                    }
                    catch (Exception e)
                    {
                        toReturn.Data = default(T);
                    }
                }
                else
                {
                    toReturn.Data = default(T);
                }
            }
            catch (Exception ex)
            {
				toReturn.Data = default(T);
            }
            return toReturn;
        }

        public static async Task<ServerResponse<T>> PostRequest<T>(string route, string jsonData)
        {
			ServerResponse<T> toReturn = new ServerResponse<T>();

			try
			{
				HttpClient client = CreateClient();
				HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

				toReturn.Response = await client.PostAsync(route, content);
				string jsonString = await toReturn.Response.Content.ReadAsStringAsync();

				if (toReturn.Response != null && toReturn.Response.IsSuccessStatusCode)
				{
					if (typeof(T) == typeof(string))
					{
						toReturn.Data = (T)(object)jsonString;
					}
					toReturn.Data = JsonConvert.DeserializeObject<T>(jsonString);
				}
				else
				{
					toReturn.Data = default(T);
					toReturn.BadRequest = JsonConvert.DeserializeObject<BadRequest>(jsonString);
				}
			}
            catch(Exception e) 
			{
				toReturn.BadRequest = new BadRequest
				{
					Message = AppResources.Error_connection
				};
			}

			return toReturn;
        }

        public static async Task<ServerResponse<T>> PutRequest<T>(string route, string jsonData)
        {
            HttpClient client = CreateClient();
            HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            ServerResponse<T> toReturn = new ServerResponse<T>();

			try
			{
				toReturn.Response = await client.PutAsync(route, content);
				string jsonString = await toReturn.Response.Content.ReadAsStringAsync();

				if (toReturn.Response.IsSuccessStatusCode)
				{
					if (typeof(T) == typeof(string))
					{
						toReturn.Data = (T)(object)jsonString;
					}
					toReturn.Data = JsonConvert.DeserializeObject<T>(jsonString);
				}
				else
				{
					toReturn.Data = default(T);
					toReturn.BadRequest = JsonConvert.DeserializeObject<BadRequest>(jsonString);
				}
			}
			catch
			{
				toReturn.BadRequest = new BadRequest
				{
					Message = AppResources.Error_connection
				};
			}

            return toReturn;
        }

        public static async Task<bool> DeleteRequest(string route)
        {
			try
			{
				HttpClient client = CreateClient();

				HttpResponseMessage response = await client.DeleteAsync(route);

				return response.IsSuccessStatusCode;
			}
			catch
			{
				return false;
			}	
        }

        public static async Task<ServerResponse<AccessToken>> FetchToken(UserCreate data)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(ConfidentialData.SiteUrl);

            HttpContent content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Username", data.Username),
                new KeyValuePair<string, string>("Password", data.Password),
                new KeyValuePair<string, string>("Grant_type", data.Grant_type)
            });

            ServerResponse<AccessToken> toReturn = new ServerResponse<AccessToken>();

			try
			{
				toReturn.Response = await client.PostAsync("Token", content);
				string jsonString = await toReturn.Response.Content.ReadAsStringAsync();

				if (!toReturn.Response.IsSuccessStatusCode)
				{
					toReturn.BadRequest = JsonConvert.DeserializeObject<BadRequest>(jsonString);
					return toReturn;
				}

				toReturn.Data = JsonConvert.DeserializeObject<AccessToken>(jsonString);
			}
			catch 
			{
				toReturn.BadRequest = new BadRequest
				{
					Message = AppResources.Error_connection
				};
			}

            return toReturn;
        }

        public static async Task<ServerResponse<string[]>> UploadFile(byte[] data, string path, string filename)
        {
            using (var content = new MultipartFormDataContent())
            {
                var fileContent = new ByteArrayContent(data);
                fileContent.Headers.Add("creationdate", DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"));
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = filename
                };
                content.Add(fileContent);

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.user.Token.access_token);
                    ServerResponse<string[]> resp = new ServerResponse<string[]>();

					try
					{
						resp.Response = await client.PutAsync(ConfidentialData.SiteUrl + "api/upload?filepath=" + path, content);

						string json = await resp.Response.Content.ReadAsStringAsync();
						resp.Data = JsonConvert.DeserializeObject<string[]>(json);
					}
					catch(Exception e)
					{
						resp.BadRequest = new BadRequest
						{
							Message =  AppResources.Error_connection + "\n"+ e.Message
						};
					}
   
                    return resp;
                }
            }
        }
    }
}
