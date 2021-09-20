using System.Collections.Generic;

namespace Discord.Serverless
{
    public class ApplicationCommandHandlerList
    {
        private readonly Dictionary<string, IApplicationCommandHandler> _commandHandlers = new();

        public bool RegisterHandler(string commandName, IApplicationCommandHandler handler) =>
            _commandHandlers.TryAdd(commandName.ToLower(), handler);

        public bool TryGetHandler(string commandName, out IApplicationCommandHandler handler) =>
            _commandHandlers.TryGetValue(commandName.ToLower(), out handler);
    }
}