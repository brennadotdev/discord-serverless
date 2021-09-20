using System.Threading.Tasks;
using Discord.Protocol.Request;
using Discord.Protocol.Response;

namespace Discord.Serverless
{
    public interface IApplicationCommandHandler
    {   
        public Task<InteractionResponse> HandleCommandAsync(InteractionRequest request);
    }
}