/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Globalization;

namespace MySqlConnector.Core
{
	internal sealed class ServerVersion
	{
		public ServerVersion(string versionString)
		{
			OriginalString = versionString;

			var last = 0;
			var index = versionString.IndexOf('.', last);
			var major = int.Parse(versionString.Substring(last, index - last), CultureInfo.InvariantCulture);
			last = index + 1;

			index = versionString.IndexOf('.', last);
			var minor = int.Parse(versionString.Substring(last, index - last), CultureInfo.InvariantCulture);
			last = index + 1;

			do
			{
				index++;
			} while (index < versionString.Length && versionString[index] >= '0' && versionString[index] <= '9');
			var build = int.Parse(versionString.Substring(last, index - last), CultureInfo.InvariantCulture);

			Version = new Version(major, minor, build);
		}

		public string OriginalString { get; }
		public Version Version { get; }
	}
}
