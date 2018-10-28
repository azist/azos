/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Threading.Tasks;

namespace MySqlConnector.Utilities
{
	internal static class ValueTaskExtensions
	{
		public static async ValueTask<TResult> ContinueWith<T, TResult>(this ValueTask<T> valueTask, Func<T, ValueTask<TResult>> continuation) => await continuation(await valueTask.ConfigureAwait(false)).ConfigureAwait(false);

		public static ValueTask<T> FromException<T>(Exception exception) => new ValueTask<T>(Utility.TaskFromException<T>(exception));
	}
}
