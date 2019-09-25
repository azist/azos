using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azos.Data.Access;

namespace Azos.Data.Directory
{
  /// <summary>
  /// Represents a directory of string ID-keyed entities.
  /// Directories are used as a form of KEY-Value vector databases
  /// </summary>
  public interface IEntityDirectory : IDataStore
  {
    Task<Entity> LookupAsync(EntityId id);
    Task Register(Entity entity);
    Task<bool> Unregister(EntityId id);
    Task<IEnumerable<Entity>> Query(string entityType, string filterExpression);
  }

  public interface IEntityDirectoryImplementation : IEntityDirectory, IDataStoreImplementation
  {

  }

  public class Entity
  {

  }

}
