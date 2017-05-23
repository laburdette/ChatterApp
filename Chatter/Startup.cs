using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Chatter.Startup))]
namespace Chatter
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
