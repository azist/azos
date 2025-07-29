/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Linq;

using Azos.Collections;

namespace Azos.Data
{
  public partial class Doc
  {
    /// <summary>
    /// Performs validation of data in the document returning exception object that provides description
    /// in cases when validation does not pass. Validation is performed not targeting any particular backend (any target)
    /// </summary>
    public Exception Validate() => Validate(null);

    public Exception Validate(string targetName) => Validate(new ValidState(targetName, ValidErrorMode.Single)).Error;

    /// <summary>
    /// Validates data document using schema/supplied field definitions.
    /// Override to perform custom validations,
    /// i.e. TypeDocs may directly access properties and write some validation type-safe code.
    /// The method is not expected to throw exception in case of failed business logic validation, rather return exception instance because
    ///  throwing exception really impedes validation performance when many docs/rows need to be validated.
    /// A thrown exception indicates an unexpected condition/a bug in the validation logic itself.
    /// </summary>
    /// <param name="state">The <see cref="ValidState"/> that gets passed in and returned</param>
    /// <param name="scope">
    /// Optional logical scope name, such as a name of parent document used for complex structure validation,
    /// for example you can use the value to add to special validation exception details to include the "parent path" of validation.
    /// This is passed as a separate parameter from ValidState because in practice it is needed mostly for nested IValidatable such as
    /// custom data types so they can report a name of the field that contains them
    /// </param>
    public virtual ValidState Validate(ValidState state, string scope = null)
    {
      foreach(var fd in Schema)
      {
        state = ValidateField(state, fd, scope);
        if (state.ShouldStop) break;
      }

      return state;
    }

    /// <summary>
    /// Validates document field using Schema.FieldDef settings.
    /// This method is invoked by base Validate() implementation.
    /// The method is not expected to throw exception in case of failed validation, rather return exception instance because
    ///  throwing exception really impedes validation performance when many document instances need to be validated
    /// </summary>
    public virtual ValidState ValidateField(ValidState state, Schema.FieldDef fdef, string scope = null)
    {
      if (fdef == null)
        throw new FieldValidationException(Schema.DisplayName,
                                           CoreConsts.NULL_STRING,
                                           StringConsts.ARGUMENT_ERROR + ".ValidateField(fdef=null)", scope);

      var atr = fdef[state.TargetName];
      if (atr==null) return state; //not found per target

      var value = GetFieldValue(fdef);

      var (hasValue, error) = CheckValueRequired(state.TargetName, fdef, atr, value, scope);
      if (error != null) return new ValidState(state, error);
      if (!hasValue) return state;//nothing else left to check


      state = CheckValueIValidatable(state, fdef, atr, value, scope);
      if (state.ShouldStop) return state;

      error = CheckValueLength(state.TargetName, fdef, atr, value, scope);
      if (error != null)
      {
        state = new ValidState(state, error);
        if (state.ShouldStop) return state;
      }

      error = CheckValueKind(state.TargetName, fdef, atr, value, scope);
      if (error != null)
      {
        state = new ValidState(state, error);
        if (state.ShouldStop) return state;
      }

      error = CheckValueMinMax(state.TargetName, fdef, atr, value, scope);
      if (error != null)
      {
        state = new ValidState(state, error);
        if (state.ShouldStop) return state;
      }

      error = CheckValueRegExp(state.TargetName, fdef, atr, value, scope);
      if (error != null)
      {
        state = new ValidState(state, error);
        if (state.ShouldStop) return state;
      }

      //this is at the end as ValueList check might induce a database call  to get a pick list (when it is not cached)
      error = CheckValueList(state.TargetName, fdef, atr, value, scope);
      if (error != null)
      {
        state = new ValidState(state, error);
        if (state.ShouldStop) return state;
      }

      return state;
    }

    protected virtual (bool hasValue, Exception error) CheckValueRequired(string targetName, Schema.FieldDef fdef, FieldAttribute atr, object value, string scope)
    {
      var missing =
           (value == null) ||
           (value is string strv && strv.IsNullOrWhiteSpace()) || //string null, or whitespace are treated as missing
           (value is IRequiredCheck ireq && !ireq.CheckRequired(targetName));

      if (missing)
      {
        if (atr.Required)
          return (false, new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_REQUIRED_ERROR, scope));

        return (false, null); //no other validations are needed as field is null anyway
      }

      return (true, null);
    }

    protected virtual Exception CheckValueLength(string targetName, Schema.FieldDef fdef, FieldAttribute atr, object value, string scope)
    {
      if (atr.MinLength < 1 && atr.MaxLength < 1) return null;

      if (value is ILengthCheck lc)
      {
        if (atr.MinLength > 0 && !lc.CheckMinLength(targetName, atr.MinLength))
          return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_MIN_LENGTH_ERROR.Args(atr.MinLength), scope);

        if (atr.MaxLength > 0 && !lc.CheckMaxLength(targetName, atr.MaxLength))
          return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_MAX_LENGTH_ERROR.Args(atr.MaxLength), scope);

        return null;
      }

      var isString = value is string;
      var eobj = value as IEnumerable;
      var ecount = !isString && eobj != null ? eobj.Cast<object>().Count() : -1;

      if (atr.MinLength > 0)
      {
        if (ecount >= 0)
        {
          if (ecount < atr.MinLength)
            return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_MIN_LENGTH_ERROR.Args(atr.MinLength), scope);
        }
        else
        {
          if (value.ToString().Length < atr.MinLength)
            return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_MIN_LENGTH_ERROR.Args(atr.MinLength), scope);
        }
      }

      if (atr.MaxLength > 0)
      {
        if (ecount >= 0)
        {
          if (ecount > atr.MaxLength)
            return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_MAX_LENGTH_ERROR.Args(atr.MaxLength), scope);
        }
        else
        {
          if (value.ToString().Length > atr.MaxLength)
            return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_MAX_LENGTH_ERROR.Args(atr.MaxLength), scope);
        }
      }

      return null;
    }

    protected virtual Exception CheckValueKind(string targetName, Schema.FieldDef fdef, FieldAttribute atr, object value, string scope)
    {
      if (atr.Kind == DataKind.ScreenName)
      {
        if (!Azos.Text.DataEntryUtils.CheckScreenName(value.ToString()))
          return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_SCREEN_NAME_ERROR, scope);
      }
      else if (atr.Kind == DataKind.EMail)
      {
        if (!Azos.Text.DataEntryUtils.CheckEMail(value.ToString()))
          return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_EMAIL_ERROR, scope);
      }
      else if (atr.Kind == DataKind.Telephone)
      {
        if (!Azos.Text.DataEntryUtils.CheckTelephone(value.ToString()))
          return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_PHONE_ERROR, scope);
      }
      else if (atr.Kind == DataKind.Uri)
      {
        if (!Uri.TryCreate(value.ToString(), UriKind.RelativeOrAbsolute, out var _))
          return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_PHONE_ERROR, scope);
      }

      return null;
    }

    protected string GetInnerScope(Schema.FieldDef fdef, string scope) => scope.IsNullOrWhiteSpace() ? fdef.Name : scope + "." + fdef.Name;

    private static ValidState validateIValidatable(Doc self, ObjectGraph graph, IValidatable validatable, ValidState state, string scope)
    => !graph.Visited(validatable) ? validatable.Validate(state, scope) : state;

    private static ValidState validateIDictionary(Doc self, ObjectGraph graph, IDictionary dict, ValidState state, string scope)
    {
      foreach (var v in dict.Values)
      {
        if (state.ShouldStop) break;
        if (v is IValidatable vv && !graph.Visited(vv))
          state = vv.Validate(state, scope);
      }
      return state;
    }

    private static ValidState validateIEnumerable(Doc self, ObjectGraph graph, IEnumerable enm, ValidState state, string scope)
    {
      foreach (var v in enm)
      {
        if (state.ShouldStop) break;
        if (v is IValidatable vv && !graph.Visited(vv))
          state = vv.Validate(state, scope);
      }
      return state;
    }

    protected virtual ValidState CheckValueIValidatable(ValidState state, Schema.FieldDef fdef, FieldAttribute atr, object value, string scope)
    {
      //20200517 DKh using ObjectGraph.Scope()
      //--------------------------------------

      if (value is IValidatable validatable)
      {
        var got = ObjectGraph.Scope("Doc.CheckValueIValidatable.IVal", //name of the state machine
                                         this, //reference to cycle subject - the document itself
                                         validatable,//arg1
                                         state, //arg2
                                         GetInnerScope(fdef, scope),//arg3
                                         body: validateIValidatable
                                       );//machine
        return got.OK ? got.result : state;
      }

      //precedence of IFs is important, IDictionary is IEnumerable
      if (value is IDictionary dict)//Dictionary<string, IValidatable>
      {
        var got = ObjectGraph.Scope("Doc.CheckValueIValidatable.IDict",
                                          this,
                                          dict,
                                          state,
                                          GetInnerScope(fdef, scope),
                                          body: validateIDictionary
                                        );
        return got.OK ? got.result : state;
      }

      if (value is IEnumerable enm)//List<IValidatable>, IValidatable[]
      {
        var got = ObjectGraph.Scope("Doc.CheckValueIValidatable.IEnum",
                                            this,
                                            enm,
                                            state,
                                            GetInnerScope(fdef, scope),
                                            body: validateIEnumerable
                                        );
        return got.OK ? got.result : state;
      }

      return state;
    }

    /// <summary>
    ///Dynamic value lists override static ones:
    /// * If a dynamic value list is null then hard coded ValueList is enforced if it is specified;
    /// * If a dynamic list is non-null then it is enforced if it is not blank, otherwise nothing is checked;
    /// Therefore: you may return an empty non-null dynamic list to prevent application of ValueList check for specific field/target
    /// </summary>
    protected virtual Exception CheckValueList(string targetName, Schema.FieldDef fdef,  FieldAttribute atr, object value, string scope)
    {
      //try to obtain dynamic value list
      var dynValueList = GetDynamicFieldValueList(fdef, targetName, Atom.ZERO);
      if (dynValueList != null)//check dynamic list is supplied
      {
        if (dynValueList.Count == 0) return null;//Nothing to check against; this is used to return empty list to override ValueList list
        var fv = value.ToString();
        if (!dynValueList.ContainsKey(fv))
         return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_IS_NOT_IN_LIST_ERROR.Args(fv.TakeFirstChars(9, "..")), scope);
      }
      else if (atr.HasValueList)//check ValueList dictionary
      {
        var parsed = atr.ParseValueList();
        var fv = value.ToString();
        if (!parsed.ContainsKey(fv))
          return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_IS_NOT_IN_LIST_ERROR.Args(fv.TakeFirstChars(9, "..")), scope);
      }

      return null;
    }

    protected virtual Exception CheckValueMinMax(string targetName, Schema.FieldDef fdef, FieldAttribute atr, object value, string scope)
    {
      if (!(value is IComparable val)) return null;

      if (atr.Min != null)
      {
        var bound = atr.Min as IComparable;
        if (bound != null)
        {
            var tval = val.GetType();

            //The conversion of the bound may not be required, there is no way to know in general
            //if the target type is capable of comparison with the bound type
            // so we try to convert the type, if it fails we proceed with unconverted bound
            try { bound = Convert.ChangeType(bound, tval) as IComparable; } catch {  }

            if (val.CompareTo(bound) < 0)
                return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_MIN_BOUND_ERROR, scope);
        }
      }

      if (atr.Max != null)
      {
        var bound = atr.Max as IComparable;
        if (bound != null)
        {
            var tval = val.GetType();

            //The conversion of the bound may not be required, there is no way to know in general
            //if the target type is capable of comparison with the bound type
            // so we try to convert the type, if it fails we proceed with unconverted bound
            try { bound = Convert.ChangeType(bound, tval) as IComparable; } catch {  }

            if (val.CompareTo(bound) > 0)
                return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_MAX_BOUND_ERROR, scope);
        }
      }

      return null;
    }

    protected virtual Exception CheckValueRegExp(string targetName, Schema.FieldDef fdef, FieldAttribute atr, object value, string scope)
    {
      if (atr.FormatRegExp.IsNotNullOrWhiteSpace())
      {
        //For those VERY RARE cases when RegExpFormat may need to be applied to complex types, i.e. StringBuilder
        //set the flag in metadata to true, otherwise regexp gets matched only for STRINGS
        var complex = atr.Metadata == null ? false
                                        : atr.Metadata
                                            .AttrByName("validate-format-regexp-complex-types")
                                            .ValueAsBool(false);
        if (complex || value is string)
        {
          if (!System.Text.RegularExpressions.Regex.IsMatch(value.ToString(), atr.FormatRegExp))
            return new FieldValidationException(Schema.DisplayName, fdef.Name,
              StringConsts.CRUD_FIELD_VALUE_REGEXP_ERROR.Args(atr.FormatDescription ?? "Input format: {0}".Args(atr.FormatRegExp)), scope);
        }
      }

      return null;
    }

  }
}
