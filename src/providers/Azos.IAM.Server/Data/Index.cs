
using Azos.Data;

namespace Azos.IAM.Server.Data
{
  /// <summary>
  /// Indexes entities on their ad hoc traits, such as their properties
  /// </summary>
  public sealed class Index : Entity
  {

    [Field(required: true,
           description: "Entity",
           metadata: "idx{name='main' order='1'}")]
    [Field(typeof(Index), nameof(Entity), TMONGO, backendName: "e")]
    public string    Entity{ get; set;}

    [Field(required: true,
           description: "Indexed trait name",
           metadata: "idx{name='trait' order='1'}")]
    [Field(typeof(Index), nameof(TraitName), TMONGO, backendName: "tn")]
    public string    TraitName { get; set; }

    [Field(required: true,
           description: "Indexed trait value",
           metadata: "idx{name='trait' order='0'}")]
    [Field(typeof(Index), nameof(TraitValue), TMONGO, backendName: "tv")]
    public string    TraitValue {  get; set;}


    [Field(required: true,
           description: "Entitities GDID",
           metadata: "idx{name='main' order='0' dir=asc}")]
    [Field(typeof(Index), nameof(G_Entity), TMONGO, backendName: "g_e")]
    public GDID    G_Entity { get; set; }

  }
}
