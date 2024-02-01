using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RFC.Example
{
    public class GPTAPI
    {
        public static async Task<string> SendMessage(string message, string systemmessage, string model)
        {
            using (var httpc = new HttpClient())
            {
                var objToPost = new RequestObject
                {
                    model = model,
                    messages = new RequestMessage[]
                    {
                        new RequestMessage
                        {
                            role = "system",
                            content = systemmessage
                        },
                        new RequestMessage
                        {
                            role = "user",
                            content = message
                        }
                    }
                };

                var jsonToPost = JsonConvert.SerializeObject(objToPost);
                var contentToPost = new StringContent(jsonToPost, Encoding.UTF8, "application/json");

                using (var mypostjsonrequest = await httpc.PostAsync("http://localhost:6789", contentToPost))
                {
                    var myrep = await mypostjsonrequest.Content.ReadAsStringAsync();

                    var mymess = JsonConvert.DeserializeObject<Resposeobject>(myrep);

                    var actmes = mymess.choices[0].message.content;

                    return actmes;
                }
            }
        }
    }

    public class RequestObject
    {
        public string model { get; set; }
        public RequestMessage[] messages { get; set; }
    }

    public class RequestMessage
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    public class Resposeobject
    {
        public string id { get; set; }
        public string _object { get; set; }
        public int created { get; set; }
        public string model { get; set; }
        public string system_fingerprint { get; set; }
        public Choice[] choices { get; set; }
        public Usage usage { get; set; }
    }

    public class Usage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }

    public class Choice
    {
        public int index { get; set; }
        public Message message { get; set; }
        public object logprobs { get; set; }
        public string finish_reason { get; set; }
    }

    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }
}
