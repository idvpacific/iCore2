using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace iCore_Administrator.Modules
{
    public class RawContentReader
    {
        public static async Task<string> Read(HttpRequestMessage req)
        {
            try
            {
                using (var contentStream = await req.Content.ReadAsStreamAsync())
                {
                    contentStream.Seek(0, SeekOrigin.Begin);
                    using (var sr = new StreamReader(contentStream))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}