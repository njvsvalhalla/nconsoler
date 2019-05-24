//      The contents of this file are subject to the Mozilla Public License
//      Version 1.1 (the "License"); you may not use this file except in
//      compliance with the License. You may obtain a copy of the License at
//      https://www.mozilla.org/MPL/

//      Software distributed under the License is distributed on an "AS IS"
//      basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//      License for the specific language governing rights and limitations
//      under the License.
//      The Original Code is located at the nconsoler github:
//      https://github.com/csharpus/nconsoler.

//      The Initial Developer of the Original Code is csharupus.
//      Portions created by Neal Daniel (neal@nealmdaniel.com) are Copyright (C)
//      Neal Daniel (neal@nealmdaniel.com). All Rights Reserved.
//      Contributor(s): Neal Daniel (neal@nealmdaniel.com).

namespace NConsoler
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	public class LinuxNotationStrategy : INotationStrategy
	{
		private readonly string[] _args;
		private IMessenger _messenger;
		private readonly Metadata _metadata;

		public LinuxNotationStrategy(string[] args, IMessenger messenger, Metadata metadata)
		{
			_args = args;
			_messenger = messenger;
			_metadata = metadata;
		}

		public MethodInfo GetCurrentMethod()
		{
			if (!_metadata.IsMulticommand)
			{
				return _metadata.FirstActionMethod();
			}
			return _metadata.GetMethodByName(_args[0].ToLower());
		}

		public void ValidateInput(MethodInfo method)
		{
		}

		public object[] BuildParameterArray(MethodInfo method)
		{
			var optionalValues = new Dictionary<string, string>();
			for (var i = 0; i < _args.Length - _metadata.RequiredParameterCount(method); i += 2)
			{
				optionalValues.Add(_args[i].Substring(1), _args[i + 1]);
			}
			var parameters = method.GetParameters();
			var parameterValues = parameters.Select(p => (object) null).ToList();

			var requiredStartIndex = _args.Length - _metadata.RequiredParameterCount(method);
			var requiredValues = _args.Where((a, i) => i >= requiredStartIndex).ToList();
			for (var i = 0; i < requiredValues.Count; i++)
			{
				parameterValues[i] = StringToObject.ConvertValue(requiredValues[i], parameters[i].ParameterType);
			}
			for (var i = _metadata.RequiredParameterCount(method); i < parameters.Length; i++ )
			{
				var optional = _metadata.GetOptional(parameters[i]);
				if (optionalValues.ContainsKey(parameters[i].Name))
				{
					parameterValues[i] = StringToObject.ConvertValue(optionalValues[parameters[i].Name], parameters[i].ParameterType);
				}
				else
				{
					parameterValues[i] = optional.Default;
				}
			}
			return parameterValues.ToArray();
		}

		public IEnumerable<string> OptionalParameters(MethodInfo method)
		{
			return new string[] {};
		}

		public void PrintUsage()
		{
			throw new System.NotImplementedException();
		}
	}
}