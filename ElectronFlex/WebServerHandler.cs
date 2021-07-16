using System.Threading.Tasks;
using WatsonWebserver;

namespace ElectronFlex
{
    public static class WebServerHandler
    {
        public static async Task DefaultRoute(HttpContext ctx)
        { 
            await ctx.Response.Send("No content provided by this route");
        }
    }
}