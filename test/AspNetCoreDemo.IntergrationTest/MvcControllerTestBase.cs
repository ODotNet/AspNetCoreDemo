using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspNetCoreDemo.IntergrationTest
{
    public class MvcControllerTestBase
    {
        protected TestServer Server { get; private set; }
        protected HttpClient Client { get { return this.Server.CreateClient(); } }

        public MvcControllerTestBase()
        {
            string contentPath = GetMvcContentPath();
            Server = new TestServer(new WebHostBuilder().UseStartup<Startup>().UseContentRoot(contentPath));
        }

        private static string GetMvcContentPath()
        {
            var strDSC = Path.DirectorySeparatorChar;
            var currentPath = Directory.GetCurrentDirectory();
            var contentPath = Path.Combine(currentPath.Substring(0, currentPath.IndexOf($"{strDSC}test{strDSC}")), $"src{strDSC}AspNetCoreDemo");
            return contentPath;
        }
    }
}
