using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCoreDemo.IntergrationTest.DevControllers
{
    public class TestActionTest : MvcControllerTestBase
    {
        [Fact]
        public async Task Test_Get_Test()
        {
            var response = await Client.GetAsync("/dev/test");

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.NotEmpty(responseContent);
        }

        [Fact]
        public async Task Test_Header_Test()
        {
            var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Head, "/dev/test"));

            response.EnsureSuccessStatusCode();

            var contentLength = response.Content.Headers.GetValues("Content-Length");

            Assert.Equal(1, contentLength.Count());
            Assert.Equal("1", contentLength.Single());
        }
    }
}
