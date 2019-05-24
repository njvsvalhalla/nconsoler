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

using System;
using NConsoler;

namespace Quickstart
{
    class Program
    {
        static void Main(string[] args)
        {
            Consolery.Run(typeof(Program), args);
        }

        [Action]
        public static void DoWork(
              [Required] int count,
              [Optional(false)] bool flag)
        {
            Console.WriteLine("DoWork {0} {1}", count, flag);
        }
    }
}
