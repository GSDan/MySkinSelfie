using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SkinSelfie.AppModels
{
    public class ServerResponse<T>
    {
        public HttpResponseMessage Response { get; set; }
        public T Data { get; set; }
        public BadRequest BadRequest { get; set; }
    }

    public class BadRequest
    {
        public string Message { get; set; }
        public IDictionary<string, string[]> ModelState { get; set; }
    }
}
