/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Azos.Data
{
  public partial class Doc
  {
    /// <summary>
    /// Performs validation of data in the document returning exception object that provides description
    /// in cases when validation does not pass. Validation is performed not targeting any particular backend (any target)
    /// </summary>
    public Exception Validate() => Validate(null);

    /// <summary>
    /// Validates data document using schema/supplied field definitions.
    /// Override to perform custom validations,
    /// i.e. TypeDocs may directly access properties and write some validation type-safe code.
    /// The method is not expected to throw exception in case of failed business logic validation, rather return exception instance because
    ///  throwing exception really hampers validation performance when many docs/rows need to be validated.
    /// The thrown exception indicates an unexpected condition/a bug in the validation logic itself.
    /// </summary>
    public virtual Exception Validate(string targetName)
    {
      foreach(var fd in Schema)
      {
        var error = ValidateField(targetName, fd);
        if (error!=null) return error;
      }

      return null;
    }

    /// <summary>
    /// Validates document field by name.
    /// Shortcut to ValidateField(Schema.FieldDef)
    /// </summary>
    public Exception ValidateField(string targetName, string fname)
    {
      var fdef = Schema[fname];
      return ValidateField(targetName, fdef);
    }

    /// <summary>
    /// Validates row field using Schema.FieldDef settings.
    /// This method is invoked by base Validate() implementation.
    /// The method is not expected to throw exception in case of failed validation, rather return exception instance because
    ///  throwing exception really hampers validation performance when many rows need to be validated
    /// </summary>
    public virtual Exception ValidateField(string targetName, Schema.FieldDef fdef)
    {
      if (fdef == null)
        throw new FieldValidationException(Schema.DisplayName,
                                                CoreConsts.NULL_STRING,
                                                StringConsts.ARGUMENT_ERROR + ".ValidateField(fdef=null)");

      var atr = fdef[targetName];
      if (atr==null) return null; //not found per target

      var value = GetFieldValue(fdef);

      var (hasValue, error) = CheckValueRequired(targetName, fdef, atr, value);
      if (error != null) return error;
      if (!hasValue) return null;//nothing left to check


      error = CheckValueIValidatable(targetName, fdef, atr, value);
      if (error!=null) return error;

      error = CheckValueLength(targetName, fdef, atr, value);
      if (error != null) return error;

      error = CheckValueKind(targetName, fdef, atr, value);
      if (error != null) return error;

      error = CheckValueMinMax(targetName, fdef, atr, value);
      if (error!=null) return error;

      error = CheckValueRegExp(targetName, fdef, atr, value);
      if (error != null) return error;

      //this is at the end as ValueList check might induce a database call  to get a pick list (when it is not cached)
      error = CheckValueList(targetName, fdef, atr, value);
      if (error!=null) return error;

      return null;//no validation errors detected
    }


    protected virtual (bool hasValue, Exception error) CheckValueRequired(string targetName, Schema.FieldDef fdef, FieldAttribute atr, object value)
    {
      if (value == null ||
         (value is string strv && strv.IsNullOrWhiteSpace()) ||
         (value is GDID gdid && gdid.IsZero))
      {
        if (atr.Required)
          return (false, new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_REQUIRED_ERROR));

        return (false, null); //no other validations are needed as field is null anyway
      }

      return (true, null);
    }

    protected virtual Exception CheckValueLength(string targetName, Schema.FieldDef fdef, FieldAttribute atr, object value)
    {
      if (atr.MinLength < 1 && atr.MaxLength < 1) return null;

      var isString = value is string;
      var eobj = value as IEnumerable;
      var ecount = !isString && eobj != null ? eobj.Cast<object>().Count() : -1;

      if (atr.MinLength > 0)
      {
        if (ecount >= 0)
        {
          if (ecount < atr.MinLength)
            return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_MIN_LENGTH_ERROR.Args(atr.MinLength));
        }
        else
        {
          if (value.ToString().Length < atr.MinLength)
            return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_MIN_LENGTH_ERROR.Args(atr.MinLength));
        }
      }

      if (atr.MaxLength > 0)
      {
        if (ecount >= 0)
        {
          if (ecount > atr.MaxLength)
            return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_MAX_LENGTH_ERROR.Args(atr.MaxLength));
        }
        else
        {
          if (value.ToString().Length > atr.MaxLength)
            return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_MAX_LENGTH_ERROR.Args(atr.MaxLength));
        }
      }

      return null;
    }

    protected virtual Exception CheckValueKind(string targetName, Schema.FieldDef fdef, FieldAttribute atr, object value)
    {
      if (atr.Kind == DataKind.ScreenName)
      {
        if (!Azos.Text.DataEntryUtils.CheckScreenName(value.ToString()))
          return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_SCREEN_NAME_ERROR);
      }
      else if (atr.Kind == DataKind.EMail)
      {
        if (!Azos.Text.DataEntryUtils.CheckEMail(value.ToString()))
          return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_EMAIL_ERROR);
      }
      else if (atr.Kind == DataKind.Telephone)
      {
        if (!Azos.Text.DataEntryUtils.CheckTelephone(value.ToString()))
          return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_PHONE_ERROR);
      }
      else if (atr.Kind == DataKind.Uri)
      {
        if (!Uri.TryCreate(value.ToString(), UriKind.RelativeOrAbsolute, out var _))
          return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_PHONE_ERROR);
      }

      return null;
    }

    protected virtual Exception CheckValueIValidatable(string targetName, Schema.FieldDef fdef, FieldAttribute atr, object value)
    {
      if (value is IValidatable validatable)
        return validatable.Validate(targetName);

      if (value is IDictionary dict)//Dictionary<string, IValidatable>
      {
        foreach (var v in dict.Values)
        {
          if (v is IValidatable vv)
          {
            var error = vv.Validate(targetName);
            if (error != null) return error;
          }
        }
      }
      else if (value is IEnumerable enm)//List<IValidatable>, IValidatable[]
      {
        foreach (var v in enm)
        {
          if (!(v is IValidatable vv)) continue;
          var error = vv.Validate(targetName);
          if (error != null) return error;
        }
      }

      return null;
    }

    protected virtual Exception CheckValueList(string targetName, Schema.FieldDef fdef,  FieldAttribute atr, object value)
    {
      if (atr.HasValueList)//check dictionary
      {
        var parsed = atr.ParseValueList();
        if (isSimpleKeyStringMap(parsed))
        {
          var fv = value.ToString();
          if (!parsed.ContainsKey(fv))
            return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_IS_NOT_IN_LIST_ERROR.Args(fv.TakeFirstChars(9, "..")));
        }
      }

      //check dynamic value list
      var dynValueList = GetDynamicFieldValueList(fdef, targetName, null);
      if (dynValueList != null)//check dictionary
      {
        var fv = value.ToString();
        if (!dynValueList.ContainsKey(fv))
          return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_IS_NOT_IN_LIST_ERROR.Args(fv.TakeFirstChars(9, "..")));
      }

      return null;
    }


    protected virtual Exception CheckValueMinMax(string targetName, Schema.FieldDef fdef, FieldAttribute atr, object value)
    {
      if (!(value is IComparable val)) return null;

      if (atr.Min != null)
      {
        var bound = atr.Min as IComparable;
        if (bound != null)
        {
            var tval = val.GetType();

            bound = Convert.ChangeType(bound, tval) as IComparable;

            if (val.CompareTo(bound)<0)
                return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_MIN_BOUND_ERROR);
        }
      }

      if (atr.Max != null)
      {
        var bound = atr.Max as IComparable;
        if (bound != null)
        {
            var tval = val.GetType();

            bound = Convert.ChangeType(bound, tval) as IComparable;

            if (val.CompareTo(bound)>0)
                return new FieldValidationException(Schema.DisplayName, fdef.Name, StringConsts.CRUD_FIELD_VALUE_MAX_BOUND_ERROR);
        }
      }

      return null;
    }

    protected virtual Exception CheckValueRegExp(string targetName, Schema.FieldDef fdef, FieldAttribute atr, object value)
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
              StringConsts.CRUD_FIELD_VALUE_REGEXP_ERROR.Args(atr.FormatDescription ?? "Input format: {0}".Args(atr.FormatRegExp)));
        }
      }

      return null;
    }

  }
}
