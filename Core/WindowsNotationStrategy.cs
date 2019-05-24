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
	using System.Reflection;
	using System;
	using System.Linq;

	public class WindowsNotationStrategy : INotationStrategy
	{
		private readonly string[] _args;
		private readonly IMessenger _messenger;
		private readonly Metadata _metadata;
		private readonly Type _targetType;
		private readonly List<MethodInfo> _actionMethods;

		public WindowsNotationStrategy(string[] args, IMessenger messenger, Metadata metadata, Type targetType, List<MethodInfo> actionMethods)
		{
			this._args = args;
			this._messenger = messenger;
			this._metadata = metadata;
			this._targetType = targetType;
			this._actionMethods = actionMethods;
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
			CheckAllRequiredParametersAreSet(method);
			CheckOptionalParametersAreNotDuplicated(method);
			CheckUnknownParametersAreNotPassed(method);
		}

		private void CheckAllRequiredParametersAreSet(MethodInfo method)
		{
			var minimumArgsLengh = _metadata.RequiredParameterCount(method);
			if (_metadata.IsMulticommand)
			{
				minimumArgsLengh++;
			}
			if (_args.Length < minimumArgsLengh)
			{
				PrintUsage();
				throw new NConsolerException("Error: Not all required parameters are set");
			}
		}

		private void CheckOptionalParametersAreNotDuplicated(MethodInfo method)
		{
			var passedParameters = new List<string>();
			foreach (var optionalParameter in OptionalParameters(method))
			{
				if (!optionalParameter.StartsWith("/"))
				{
					throw new NConsolerException("Unknown parameter {0}", optionalParameter);
				}
				var name = ParameterName(optionalParameter);
				if (passedParameters.Contains(name))
				{
					throw new NConsolerException("Parameter with name {0} passed two times", name);
				}
				passedParameters.Add(name);
			}
		}

		private void CheckUnknownParametersAreNotPassed(MethodInfo method)
		{
			var parameterNames = new List<string>();
			foreach (var parameter in method.GetParameters())
			{
				if (_metadata.IsRequired(parameter))
				{
					continue;
				}
				parameterNames.Add(parameter.Name.ToLower());
				var optional = _metadata.GetOptional(parameter);
				foreach (var altName in optional.AltNames)
				{
					parameterNames.Add(altName.ToLower());
				}
			}
			foreach (var optionalParameter in OptionalParameters(method))
			{
				var name = ParameterName(optionalParameter);
				if (!parameterNames.Contains(name.ToLower()))
				{
					throw new NConsolerException("Unknown parameter name {0}", optionalParameter);
				}
			}
		}

		private static string ParameterName(string parameter)
		{
			if (parameter.StartsWith("/-"))
			{
				return parameter.Substring(2).ToLower();
			}
			if (parameter.Contains(":"))
			{
				return parameter.Substring(1, parameter.IndexOf(":") - 1).ToLower();
			}
			return parameter.Substring(1).ToLower();
		}

		public object[] BuildParameterArray(MethodInfo method)
		{
			var argumentIndex = _metadata.IsMulticommand ? 1 : 0;
			var parameterValues = new List<object>();
			var aliases = new Dictionary<string, Consolery.ParameterData>();
			foreach (var info in method.GetParameters())
			{
				if (_metadata.IsRequired(info))
				{
					parameterValues.Add(StringToObject.ConvertValue(_args[argumentIndex], info.ParameterType));
				}
				else
				{
					var optional = _metadata.GetOptional(info);

					foreach (var altName in optional.AltNames)
					{
						aliases.Add(altName.ToLower(),
									new Consolery.ParameterData(parameterValues.Count, info.ParameterType));
					}
					aliases.Add(info.Name.ToLower(),
								new Consolery.ParameterData(parameterValues.Count, info.ParameterType));
					parameterValues.Add(optional.Default);
				}
				argumentIndex++;
			}
			foreach (var optionalParameter in OptionalParameters(method))
			{
				var name = ParameterName(optionalParameter);
				var value = ParameterValue(optionalParameter);
				parameterValues[aliases[name].Position] = StringToObject.ConvertValue(value, aliases[name].Type);
			}
			return parameterValues.ToArray();
		}

		private static string ParameterValue(string parameter)
		{
			if (parameter.StartsWith("/-"))
			{
				return "false";
			}
			if (parameter.Contains(":"))
			{
				return parameter.Substring(parameter.IndexOf(":") + 1);
			}
			return "true";
		}

		public IEnumerable<string> OptionalParameters(MethodInfo method)
		{
			var firstOptionalParameterIndex = _metadata.RequiredParameterCount(method);
			if (_metadata.IsMulticommand)
			{
				firstOptionalParameterIndex++;
			}
			for (var i = firstOptionalParameterIndex; i < _args.Length; i++)
			{
				yield return _args[i];
			}
		}

		public void PrintUsage()
		{
			if (_metadata.IsMulticommand && !IsSubcommandHelpRequested())
			{
				PrintGeneralMulticommandUsage();
			}
			else if (_metadata.IsMulticommand && IsSubcommandHelpRequested())
			{
				PrintSubcommandUsage();
			}
			else
			{
				PrintUsage(_actionMethods[0]);
			}
		}
		
		private void PrintSubcommandUsage()
		{
			var method = _metadata.GetMethodByName(_args[1].ToLower());
			if (method == null)
			{
				PrintGeneralMulticommandUsage();
				throw new NConsolerException("Unknown subcommand \"{0}\"", _args[0].ToLower());
			}
			PrintUsage(method);
		}

		#region Usage

		private void PrintUsage(MethodInfo method)
		{
			PrintMethodDescription(method);
			var parameters = GetParametersMetadata(method);
			PrintUsageExample(method, parameters);
			PrintParameterUsage(parameters);
		}

		private void PrintUsageExample(MethodInfo method, IList<ParameterMetadata> parameterList)
		{
			var subcommand = _metadata.IsMulticommand ? method.Name.ToLower() + " " : string.Empty;

			var parameters = string.Join(" ", parameterList.Select(p => p.Name).ToArray());
			_messenger.Write("usage: " + ProgramName() + " " + subcommand + parameters);
		}

		private void PrintMethodDescription(MethodInfo method)
		{
			var description = GetMethodDescription(method);
			if (description == string.Empty) return;
			_messenger.Write(description);
		}

		public string GetMethodDescription(MethodInfo method)
		{
			var attributes = method.GetCustomAttributes(true);
			foreach (var attribute in attributes.OfType<ActionAttribute>())
			{
				return attribute.Description;
			}
			throw new NConsolerException("Method is not marked with an Action attribute");
		}

		private IList<ParameterMetadata> GetParametersMetadata(MethodInfo method)
		{
			var result = new List<ParameterMetadata>();
			foreach (var parameter in method.GetParameters())
			{
				var parameterAttributes =
					parameter.GetCustomAttributes(typeof(ParameterAttribute), false);
				var parameterMetadata = new ParameterMetadata { Name = GetDisplayName(parameter) };
				if (parameterAttributes.Length > 0)
				{
					var attribute = (ParameterAttribute)parameterAttributes[0];
					parameterMetadata.Description = attribute.Description;
					if (attribute is OptionalAttribute)
					{
						parameterMetadata.DefaultValue = ((OptionalAttribute)attribute).Default;
					}

				}
				result.Add(parameterMetadata);

			}
			return result;
		}

		private void PrintParameterUsage(IList<ParameterMetadata> parameters)
		{
			var identation = "    ";
			var maxParameterNameLength = MaxKeyLength(parameters);
			foreach (var parameter in parameters)
			{
				if (!string.IsNullOrEmpty( parameter.Description) || parameter.DefaultValue != null)
				{
					var difference = maxParameterNameLength - parameter.Name.Length + 2;

					var message = identation + parameter.Name;
					if(!string.IsNullOrEmpty(parameter.Description))
					{
						message += new string(' ', difference) + parameter.Description;
					}

					_messenger.Write(message);
				}
				if (parameter.DefaultValue != null)
				{
					var valueText = parameter.DefaultValue.ToString();
					if (parameter.DefaultValue is string)
					{
						valueText = string.Format("'{0}'", valueText);
					}
					_messenger.Write(identation + identation + "default value: " + valueText);
				}
			}
		}

		private static int MaxKeyLength(IList<ParameterMetadata> parameters)
		{
			return parameters.Any() ? parameters.Select(p => p.Name).Max(k => k.Length) : 0;
		}

		public string ProgramName()
		{
			var entryAssembly = Assembly.GetEntryAssembly();
			if (entryAssembly == null)
			{
				return _targetType.Name.ToLower();
			}
			return new AssemblyName(entryAssembly.FullName).Name;
		}

		public bool IsSubcommandHelpRequested()
		{
			return _args.Length > 0
				   && _args[0].ToLower() == "help"
				   && _args.Length == 2;
		}

		private void PrintGeneralMulticommandUsage()
		{
			_messenger.Write(string.Format("usage: {0} <subcommand> [args]", ProgramName()));
			_messenger.Write(string.Format("Type '{0} help <subcommand>' for help on a specific subcommand.", ProgramName()));
			_messenger.Write(string.Empty);
			_messenger.Write("Available subcommands:");

			foreach (var method in _actionMethods)
			{
				_messenger.Write(method.Name.ToLower() + " " + GetMethodDescription(method));
			}
		}

		private string GetDisplayName(ParameterInfo parameter)
		{
			if (_metadata.IsRequired(parameter))
			{
				return parameter.Name;
			}
			var optional = _metadata.GetOptional(parameter);
			var parameterName =
				(optional.AltNames.Length > 0) ? optional.AltNames[0] : parameter.Name;
			if (parameter.ParameterType != typeof(bool))
			{
				parameterName += ":" + ValueDescription(parameter.ParameterType);
			}
			return "[/" + parameterName + "]";
		}

		public string ValueDescription(Type type)
		{
			if (type == typeof(int))
			{
				return "number";
			}
			if (type == typeof(string))
			{
				return "value";
			}
			if (type == typeof(int[]))
			{
				return "number[+number]";
			}
			if (type == typeof(string[]))
			{
				return "value[+value]";
			}
			if (type == typeof(DateTime))
			{
				return "dd-mm-yyyy";
			}
			throw new ArgumentOutOfRangeException(string.Format("Type {0} is unknown", type.Name));
		}

		#endregion
	}
}