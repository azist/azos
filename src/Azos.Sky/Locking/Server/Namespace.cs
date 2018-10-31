using System.Collections.Generic;
using System.Linq;

using Azos.Collections;

namespace Azos.Sky.Locking.Server
{
  /// <summary>
  /// Represents a namespace that contains tables in locking server process
  /// </summary>
  internal sealed class Namespace : INamed
  {

    internal Namespace(string name)
    {
      if (name.IsNullOrWhiteSpace())
        throw new LockingException(StringConsts.ARGUMENT_ERROR+"Namespace.ctor(name==null|empty)");

      m_Name = name;
    }

    private string m_Name;
    private Registry<Table> m_Tables = new Registry<Table>();

    internal HashSet<Table> m_MutatedTables = new HashSet<Table>();


    public string Name {get{ return m_Name;}}

    public IRegistry<Table> Tables{ get{ return m_Tables;} }

    public Table GetExistingOrMakeTableByName(string name)
    {
       return m_Tables.GetOrRegister(name, (_) => new Table(this, name), 1);
    }

    public bool RemoveTableIfEmpty(Table tbl)
    {
       if (tbl.Count>0) return false;
       return m_Tables.Unregister(tbl);
    }

    public override int GetHashCode()
    {
      return m_Name.GetHashCodeOrdIgnoreCase();
    }

    public override bool Equals(object obj)
    {
      var other = obj as Namespace;
      return other!=null && this.m_Name.EqualsOrdIgnoreCase(other.m_Name);
    }
  }

}
