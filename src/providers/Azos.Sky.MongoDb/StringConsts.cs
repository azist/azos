/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


namespace Azos.Sky.MongoDb
{
  internal static class StringConsts
  {

    public const string ARGUMENT_ERROR = "Argument error: ";


    public const string TODO_QUEUE_BSON_READ_ERROR = "Todo queue '{0}', error reading BSON: {1}";

    public const string TODO_QUEUE_CONVERSION_ERROR = "Todo queue '{0}', could not convert Todo '{1}'. Error: {2}";


    public const string PROCESS_BSON_READ_ERROR = "Process error reading BSON: {0}";


    public const string KDB_SHARDSET_NO_SHARDS_ERROR = "KDB ShardSet has no shards configured: ";

    public const string KDB_SHARDSET_DUPLICATE_SHARD_ORDER_ERROR = "KDB ShardSet has duplicate shard order: ";

    public const string KDB_SHARDSET_CONFIG_SHARD_CSTR_ERROR = "KDB Shard missing connect string '{0}': {1}";

    public const string KDB_STORE_ROOT_SHARDSET_NOT_CONFIGURED = "KDB Store is missing root ShardSet";
  }
}