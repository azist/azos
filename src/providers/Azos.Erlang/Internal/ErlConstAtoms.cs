
namespace Azos.Erlang
{
  public static class ConstAtoms
  {
    internal static AtomTable AtomTable = AtomTable.Instance; // To ensure it gets created before the following atoms

    public static ErlAtom ANY           = new ErlAtom(ErlConsts.ANY);
    public static ErlAtom BadRpc        = new ErlAtom("badrpc");
    public static ErlAtom Call          = new ErlAtom("call");
    public static ErlAtom Cast          = new ErlAtom("cast");
    public static ErlAtom Erlang        = new ErlAtom("erlang");
    public static ErlAtom Error         = new ErlAtom("error");
    public static ErlAtom Format        = new ErlAtom("format");
    public static ErlAtom GenCast       = new ErlAtom("$gen_cast");
    public static ErlAtom Io_Lib        = new ErlAtom("io_lib");
    public static ErlAtom Latin1        = new ErlAtom("latin1");
    public static ErlAtom Normal        = new ErlAtom("normal");
    public static ErlAtom NoProc        = new ErlAtom("noproc");
    public static ErlAtom NoConnection  = new ErlAtom("noconnection");
    public static ErlAtom Ok            = new ErlAtom("ok");
    public static ErlAtom Request       = new ErlAtom("request");
    public static ErlAtom Rex           = new ErlAtom("rex");
    public static ErlAtom Rpc           = new ErlAtom("rpc");
    public static ErlAtom Undefined     = new ErlAtom("undefined");
    public static ErlAtom Unsupported   = new ErlAtom("unsupported");
    public static ErlAtom User          = new ErlAtom("user");
  }
}
