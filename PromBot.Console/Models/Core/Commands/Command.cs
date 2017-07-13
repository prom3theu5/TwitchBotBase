using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PromBot.Commands
{
    public class Command
    {
        private string[] _aliases;
        internal CommandParameter[] _parameters;
        private Func<CommandEventArgs, Task> _runFunc;
        internal readonly Dictionary<string, CommandParameter> _parametersByName;

        public string Text { get; }
		public string Category { get; internal set; }
        public bool IsHidden { get; internal set; }
        public string Description { get; internal set; }

		public IEnumerable<string> Aliases => _aliases;
		public IEnumerable<CommandParameter> Parameters => _parameters;
        public CommandParameter this[string name] => _parametersByName[name];

        internal Command(string text)
		{
			Text = text;
            IsHidden = false;
			_aliases = new string[0];
			_parameters = new CommandParameter[0];
			_parametersByName = new Dictionary<string, CommandParameter>();
        }


		internal void SetAliases(string[] aliases)
		{
			_aliases = aliases;
		}
		internal void SetParameters(CommandParameter[] parameters)
		{
			_parametersByName.Clear();
			for (int i = 0; i < parameters.Length; i++)
			{
				parameters[i].Id = i;
				_parametersByName[parameters[i].Name] = parameters[i];
            }
			_parameters = parameters;
        }

        internal void SetRunFunc(Func<CommandEventArgs, Task> func)
		{
			_runFunc = func;
		}

        internal void SetRunFunc(Action<CommandEventArgs> func)
		{
            _runFunc = TaskHelper.ToAsync(func);
		}

        internal Task Run(CommandEventArgs args)
		{
			var task = _runFunc(args);
			if (task != null)
				return task;
			else
				return TaskHelper.CompletedTask;
		}
	}
}
