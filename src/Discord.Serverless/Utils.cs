using Discord.Protocol.Data;
using Discord.Protocol.Response;

namespace Discord.Serverless
{
    public static class Utils
    {
        public static InteractionResponse CreateBasicResponse(string message)
        {
            return new()
            {
                Type = InteractionType.ChannelMessageWithSource,
                Data = new()
                {
                    Content = message
                }
            };
        }
    }
}