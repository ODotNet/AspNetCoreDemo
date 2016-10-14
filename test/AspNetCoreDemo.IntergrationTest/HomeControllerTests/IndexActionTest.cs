using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace AspNetCoreDemo.IntergrationTest.HomeControllerTests
{
    public class IndexActionTest : MvcControllerTestBase
    {
        [Fact]
        public async Task Index_Get_Test()
        {
            var response = await Client.GetAsync("/");

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.NotEmpty(responseContent);
        }
    }
}
