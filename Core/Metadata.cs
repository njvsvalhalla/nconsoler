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

	public class Metadata
	{
		private readonly IList<MethodInfo> _actionMethods;

		public Metadata(IList<MethodInfo> actionMethods)
		{
			_actionMethods = actionMethods;
		}

		public bool IsMulticommand
		{
			get { return _actionMethods.Count > 1; }
		}

		public bool SingleActionWithOnlyOptionalParametersSpecified()
		{
			if (IsMulticommand) return false;
			var method = _actionMethods[0];
			return OnlyOptionalParametersSpecified(method);
		}

		private bool OnlyOptionalParametersSpecified(MethodBase method)
		{
			return method.GetParameters().All(parameter => !IsRequired(parameter));
		}

		public bool IsRequired(ParameterInfo info)
		{
			var attributes = info.GetCustomAttributes(typeof(ParameterAttribute), false);
			return !info.IsOptional && (attributes.Length == 0 || attributes[0].GetType() == typeof(RequiredAttribute));
		}

		public bool IsOptional(ParameterInfo info)
		{
			return !IsRequired(info);
		}

		public Consolery.OptionalData GetOptional(ParameterInfo info)
		{
			if (info.IsOptional)
			{
				return new Consolery.OptionalData { Default = info.DefaultValue };
			}
			var attributes = info.GetCustomAttributes(typeof(OptionalAttribute), false);
			var attribute = (OptionalAttribute)attributes[0];
			return new Consolery.OptionalData { AltNames = attribute.AltNames, Default = attribute.Default };
		}

		public int RequiredParameterCount(MethodInfo method)
		{
			return method.GetParameters().Count(IsRequired);
		}

		public MethodInfo GetMethodByName(string name)
		{
			return _actionMethods.FirstOrDefault(method => method.Name.ToLower() == name);
		}

		public MethodInfo FirstActionMethod()
		{
			return _actionMethods.FirstOrDefault();
		}
	}
}