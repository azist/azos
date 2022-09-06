# **Azos Framework** Quick Start Cheat Sheet for Developers

- [GuardUtils](#GuardUtils)
- [DateUtils](#DateUtils)
- [IOUtils](#IOUtils)
- [CoreUtils](#CoreUtils)
- [DataEntryUtils](#DataEntryUtils)
- [Text.Utils](#Text.Utils)
- [CollectionUtils](#CollectionUtils)
- [StringValueConversion](#StringValueConversion)
- [ObjectValueConversion](#ObjectValueConversion)


## G8 `CommonExtensions`

=======================================

Extension methods commonly used on both client and server tiers

---

### `DefaultAndAlignOnPolicyBoundary`

Defaults Utc timestamp value from app time source when the supplied one is null, then
aligns the supplied or defaulted timestamp on the `G8Consts.POLICY_REFRESH_WINDOW_MINUTES` (120 minutes)
boundary

```csharp
var asof = asOfUtc.DefaultAndAlignOnPolicyBoundary(App);
```

---


## <span id="GuardUtils">`Azos.GuardUtils`</span>

<a href="src/Azos/GuardUtils.cs" target="_blank">View Source</a>

=======================================

Call guard exceptions thrown by the framework to indicate violations of value constraints. 
Guards are typically applied to method parameters in a fluent manner.

The CallGuardException which is thrown by these methods implements IHttpStatusProvider 
which returns 400 family of code for web consumers signifying data validation relevence.

---

### `IsNotFound`

Create a CallGuard exception instance for a not found clause. You may throw it on your own (e.g. in a ternary conditional expression).

```csharp
if(!result.Any()) throw $"Query Results for {search}".IsNotFound(nameof(result));
```

### `NonNull`

Ensures that a value is not null.

```csharp
 result.BaseAddress = this.Uri.NonNull("`{0}` is not configured".Args(nameof(Uri)));
```

### `NonNull<T>`

Ensures that a nullable value-typed value is not null.

```csharp
public int? AddMore(int? x) => m_CurrentValue + x.NonNull();
```

### `NonEmpty`

Ensures that a config node value is non-null existing node.

```csharp
internal SetConfig(ConfigSectionNode data)
{
    Data = data.NonEmpty(nameof(data));
}
```

### `NonBlank`

Ensures that a string value is non-null/blank/whitespace.

```csharp
stringValue.NonBlank(nameof(stringValue))
```

### `NonBlankMax`

Ensures that a string value is non-null/blank having at most the specified length.

```csharp
stringValue.NonBlankMax(255, nameof(stringValue))
```

### `NonBlankMin`

Ensures that a string value is non-null/blank having at least the specified length.

```csharp
stringValue.NonBlankMin(8, nameof(stringValue))
```

### `NonBlankMinMax`

Ensures that a string value is non-null/blank having its length between the specified min/max bounds.

```csharp
stringValue.NonBlankMinMax(8, 255, nameof(stringValue))
```

### `HasRequiredValue`

Ensures that the required value is present as defined by `Data.IRequiredCheck`
implementation on the instance.

```csharp
id.HasRequiredValue(nameof(id));
```

### `IsTrue`

Ensures that the condition is true.

```csharp
(m_Names.Length > 0).IsTrue("Defined names");
```

### `IsTrue<T>`

Ensures that the condition is true.

```csharp
var utcWhen = when.ToUniversalTime().IsTrue(v => v >= UNIX_EPOCH_START_DATE, "date past nix epoch");
```

---

## <span id="DateUtils">`Azos.DateUtils`</span>

<a href="src/Azos/DateUtils.cs" target="_blank">View Source</a>

=======================================

Provides core date/time-related utility functions used by the majority of projects

---

### `UNIX_EPOCH_START_DATE` const

Unix epoch start. This value MUST be UTC

```csharp
return UNIX_EPOCH_START_DATE.AddSeconds(seconds);
```

### `ToSecondsSinceUnixEpochStart`

Gets number of seconds since Unix epoch start (1970/1/1 0:0:0)

```csharp
long seconds = utcDate?.ToSecondsSinceUnixEpochStart();
```

### `FromSecondsSinceUnixEpochStart`

Gets UTC DateTime from number of seconds since Unix epoch start (1970/1/1 0:0:0)

```csharp
DateTime? utcDate = seconds?.FromSecondsSinceUnixEpochStart();
```

### `ToMillisecondsSinceUnixEpochStart`

Gets number of milliseconds since Unix epoch start (1970/1/1 0:0:0)

```csharp
long seconds = utcDate?.ToMillisecondsSinceUnixEpochStart();
```

### `FromMillisecondsSinceUnixEpochStart`

Gets UTC DateTime from number of milliseconds since Unix epoch start (1970/1/1 0:0:0)

```csharp
DateTime? utcDate = seconds?.FromMillisecondsSinceUnixEpochStart();
```

> See DateUtils.cs for additional "To" and "From" related extension methods.

### `AlignDailyMinutes`

Aligns DateTime on an adjacent minuteBoudary as of midnight.
The time component is adjusted accordingly, e.g. if you align on 60 min boundary
you align the timestamp on hourly basis relative to midnight.
Note: alignment resets as of midnight, so pass the boundary which is divisible equally within a day,
e.g. 15 min, 6 hrs, 2 hrs, are all good, however if you align by 5 hrs (as an example), you are not going to get
equal number of divisions in a 24 hr period.

```csharp
return createdAt.AlignDailyMinutes(minuteBoundary);
```

### `ToRoundtripIso8601String`

Converts a DateTime value into an ISO8601-compatible formatted string
for roundtripping e.g.: '1981-08-26T12:23:55.2340000'

```csharp
var createdAtString = createdAt.ToRoundtripIso8601String();
```

### `ApproximateTimeDistance`

Returns an approximate string representation of the point in time relative to this one,
in the most suitable scale, that is:  "1 year ago" or "in 1 year"; "45 minutes ago" or "in 45 minutes". Supports ISO_LANG=eng only

```csharp
var timeDiffString = createdAt.ApproximateTimeDistance(app.TimeSource.UTCNow);
```

---

## <span id="IOUtils">`Azos.IOUtils`</span>

<a href="src/Azos/IOUtils.cs" target="_blank">View Source</a>

=======================================

Provides IO-related utility extensions.

---

### `ToWebSafeBase64`

Encodes string with standard UTF8 encoder

```csharp
var keyBuffer = app.SecurityManager.Cryptography.GenerateRandomBytes(count);
var keyString = keyBuffer.ToWebSafeBase64();
```

### `FromWebSafeBase64`

Complementing method for ToWebSafeBase64() - reads web-safe base64 encoded string into a byte[].
Returns null for empty string.
Web-safe encoding uses `-` instead of base64 `+` and `_` instead of base64 `/`

```csharp
var keyBuffer = keyString.FromWebSafeBase64();
```

---

## <span id="CoreUtils">`Azos.CoreUtils`</span>

<a href="src/Azos/CoreUtils.cs" target="_blank">View Source</a>

=======================================

Provides core utility functions used by the majority of projects

---


### `IsDeveloperEnvironment`

Returns true for DEV or LOCAL environments.
This method is used to ascertain the "non-prod" nature of the either and is typically used
to disclose or cloak/block sensitive information/details in things like logs and debug endpoints
(e.g. config content listing, debugging etc...)

```csharp
var isDev = App.IsDeveloperEnvironment();
```


### `ToMessageWithType`

Writes exception message with exception type

```csharp
catch (Exception error)
{
    var msg = "Error performing DI into module root:" + error.ToMessageWithType();
    WriteLog(MessageType.CatastrophicError, FROM, msg, error);
    throw new AzosException(msg, error);
}
```


### `FullNameWithExpandedGenericArgs`

Returns the full name of the type optionally prefixed with verbatim id specifier '@'.
The generic arguments are expanded into their full names i.e.
List'1[System.Object]  ->  System.Collections.Generic.List<System.Object>

```csharp
var t = typeof(List<string>);
Aver.AreEqual("System.Collections.Generic.List<System.String>", t.FullNameWithExpandedGenericArgs(false));
```



### `DisplayNameWithExpandedGenericArgs`

Returns the name of the type with expanded generic argument names.
This helper is useful for printing class names to logs/messages.
List'1[System.Object]  ->  List<Object>

```csharp
var typeName = this.GetType().DisplayNameWithExpandedGenericArgs();
```



### `DontLeakAsync`

Encloses an action in try catch and logs the error if it leaked from action. This method never leaks.
Returns true if there was no error on action success, or false if error leaked from action and was logged by component.
The actual logging depends on the component log level

```csharp
var result =  await this.DontLeakAsync(async () => await GetDataAsync()).ConfigureAwait(false);
```



### `GetUtcNow`

Returns current UTC Now using app precision time source

```csharp
var now = App.GetUtcNow();
```


---

## <span id="DataEntryUtils">`Azos.Text.DataEntryUtils`</span>

<a href="src/Azos/Text/DataEntryUtils.cs" target="_blank">View Source</a>

=======================================

Provides misc data-entry parsing routines

---

### `CheckEMail`

Returns true if the value is a valid non-null/empty email address

```csharp
var isValidEml = DataEntryUtils.CheckEMail(email);
```


### `CheckTelephone`

Returns true if the value is a valid non-null/empty telephone

```csharp
var isValidTn = DataEntryUtils.CheckTelephone(phone);
```


### `CheckScreenName`

Returns true if the value starts from primary language char and contains only those chars separated by one of ['.','-','_'].
Subsequent separators not to occur more than once and can not be at the very end. This function supports Latin/Cyrrilic char sets

```csharp
var isValidScreenName = DataEntryUtils.CheckScreenName(screenName);
```


### `NormalizeUSPhone`

Normalizes US phone string so it looks like (999) 999-9999x9999.

```csharp
var n = DataEntryUtils.NormalizeUSPhone("555-222-4415 EXT 2014");
```


### `PhoneNumberToLong`

Converts phone number to long

```csharp
Aver.AreEqual(8004647669, DataEntryUtils.PhoneNumberToLong("800-GO4-SONY"));
Aver.AreEqual(55521910305187, DataEntryUtils.PhoneNumberToLong("555-219-1030x5187", false));
```


---

## <span id="Text.Utils">`Azos.Text.Utils`</span>

<a href="src/Azos/Text/Utils.cs" target="_blank">View Source</a>

=======================================

Provides misc text utils

---


### `IsURLValid`

Checks URL string for validity

```csharp
var isUrl = Utils.IsURLValid("https://www.google.com");
```


### `ParseFieldNameToDescription`

Parses database field names (column names) and converts parts to human-readable description
    like:
    "FIRST_NAME" -> "First Name",
    "FirstName" -> "First Name",
    "CHART_OF_ACCOUNTS" -> "Chart of Accounts"

```csharp
Aver.AreEqual("First Name", "FIRST_NAME".ParseFieldNameToDescription(true));
```


### `CapturePatternMatch`

Returns a captured wildcard segment from string. Pattern uses '*' for match capture by default and may contain a single capture

```csharp
Aver.AreEqual("controller/aaa", "controller/aaa/user".CapturePatternMatch("*/user" comparisonType: StringComparison.InvariantCulture));
```


### `MatchPattern`

Returns a captured wildcard segment from string. Pattern uses '*' for match capture by default and may contain a single capture

```csharp
Aver.IsTrue("We shall overcome".MatchPattern("*********overCOME", senseCase: false));
```

---

## <span id="CollectionUtils">`Azos.CollectionUtils`</span>

<a href="src/Azos/CollectionUtils.cs" target="_blank">View Source</a>

=======================================

Extensions for standard collections

---


### `AddOneAtEnd`

Adds the specified element at the end of the sequence. Sequence may be null

```csharp
var seq = Enumerable.Range(1, 2).AddOneAtEnd(10).ToArray();
```


### `AddOneAtStart`

Adds the specified element at the start of the sequence. Sequence may be null

```csharp
var seq = Enumerable.Range(1, 2).AddOneAtStart(10).ToArray();
```


### `ForEach`

Fluent notation for foreach

```csharp
runSteps.ForEach(a => process(a));
```


### `BatchBy`

Partitions the source sequence into subsequences of up to the specified size.
The operation enumerates the source only once via an internal accumulator, making
this function safe for deferred execution


```csharp
foreach (var batch in rows.BatchBy(10))
{
    SendRecords(batch);
}
```


### `ConcatArray`

Returns an array concatenated from the first element and the rest, similar to JS rest spread operator: let x = [first, ...rest];

```csharp
var allSteps = runSteps.ConcatArray(shutdownStep, finalizeStep);
```


### `ToEnumerable`

Makes an enumerable of T starting from the first element and concatenating others

```csharp
var allSteps = startStep.ToEnumerable(runStep, shutdownStep, finalizeStep);
```



## <span id="StringValueConversion">`Azos.Data.StringValueConversion`</span>

<a href="https://github.com/azist/azos/blob/master/src/Azos/Data/StringValueConversion.cs" target="_blank">View Source</a>

=======================================

Provides extension methods for converting string values to different scalar types

---


### `AsByteArray`

```csharp
byte[] buffer = stringBuffer.AsByteArray();
```

### `AsIntArray`

```csharp
int[] intArr = stringBuffer.AsIntArray();
```

### `AsLongArray`

```csharp
long[] longArr = stringBuffer.AsLongArray();
```

### `AsFloatArray`

```csharp
float[] floatArr = stringBuffer.AsFloatArray();
```

### `AsDoubleArray`

```csharp
double[] doubleArr = stringBuffer.AsDoubleArray();
```

### `AsDecimalArray`

```csharp
decimal[] decimalArr = stringBuffer.AsDecimalArray();
```


### `AsGDID`

```csharp
return id.Address.AsGDID(GDID.ZERO);
```

### `AsNullableGDID`

```csharp
return val.AsNullableGDID(null);
```


### `AsByte`

```csharp
Aver.AreEqual(0xed, "0xed".AsByte());
```

### `AsNullableByte`

```csharp
byte? byt = val.AsNullableByte(null);
```

### `AsSByte`

```csharp
Aver.AreEqual((sbyte)1, cfg.Root.AttrByName("a").ValueAsSByte());
```

### `AsNullableSByte`

```csharp
sbyte? sbyt = val.AsNullableSByte(null);
```

### `AsShort`

```csharp
short versionNumber = version.AsShort();
```

### `AsNullableShort`

```csharp
short? MinQuantity = minQtyString.AsNullableShort(null);
```

### `AsUShort`

```csharp
ushort versionNumber = version.AsUShort();
```

### `AsNullableUShort`

```csharp
ushort? MinQuantity = minQtyString.AsNullableUShort(null);
```

### `AsInt`

```csharp
int versionNumber = version.AsInt();
```

### `AsNullableInt`

```csharp
int? MinQuantity = minQtyString.AsNullableInt(null);
```

### `AsUInt`

```csharp
uint versionNumber = version.AsUInt();
```

### `AsNullableUInt`

```csharp
uint? MinQuantity = minQtyString.AsNullableUInt(null);
```


### `AsLong`

```csharp
long versionNumber = version.AsLong();
```

### `AsNullableInt`

```csharp
long? MinQuantity = minQtyString.AsNullableInt(null);
```

### `AsULong`

```csharp
ulong versionNumber = version.AsULong();
```

### `AsNullableULong`

```csharp
ulong? MinQuantity = minQtyString.AsNullableULong(null);
```

### `AsDouble`

```csharp
double weight = weightString.AsDouble();
```

### `AsNullableDouble`

```csharp
double? weight = weightString.AsNullableDouble(null);
```

### `AsFloat`

```csharp
float weight = weightString.AsFloat();
```

### `AsNullableFloat`

```csharp
float? weight = weightString.AsNullableFloat(null);
```

### `AsDecimal`

```csharp
decimal weight = weightString.AsDecimal();
```

### `AsNullableDecimal`

```csharp
decimal? weight = weightString.AsNullableDecimal(null);
```

### `AsBool`

```csharp
bool isReady = "Yes".AsBool(true);
```

### `AsNullableBool`

```csharp
bool? isReady = isReadyString.AsNullableBool(null);
```

### `AsGUID`

```csharp
bool id = "ca2d49a9-83bd-496b-97ee-099857cce385".AsGUID(Guid.Empty);
```

### `AsNullableGUID`

```csharp
bool? id = "ca2d49a9-83bd-496b-97ee-099857cce385".AsNullableGUID(null);
```

### `AsDateTimeOrThrow`

```csharp
DateTime dt = createdAtString.AsDateTimeOrThrow(DateTimeStyles.None);
```

### `AsDateTime`

```csharp
DateTime dt = createdAtString.AsDateTime(DateTime.MinValue);
```

### `AsDateTime`

```csharp
DateTime dt = createdAtString.AsDateTime(DateTime.MinValue, DateTimeStyles.AssumeUniversal);
```

### `AsDateTimeFormat`

```csharp
string createdAtString = "Mon 16 Jun 8:30 AM 2008";
DateTime dt = createdAtString.AsDateTimeFormat(DateTime.MinValue, "ddd dd MMM h:mm tt yyyy", DateTimeStyles.AssumeUniversal);
```

### `AsNullableDateTime`

```csharp
DateTime? dt = createdAtString.AsNullableDateTime();
```

### `AsTimeSpanOrThrow`

```csharp
TimeSpan ts = "02:20:08".AsTimeSpanOrThrow();
```

### `AsTimeSpan`

```csharp
TimeSpan ts = "02:20:08".AsTimeSpan(TimeSpan.FromSeconds(0));
```

### `AsNullableTimeSpan`

```csharp
TimeSpan? ts = "02:20:08".AsNullableTimeSpan(null);
```

### `AsEnum`

```csharp
DateTimeKind kind = kindString.AsEnum<DateTimeKind>(DateTimeKind.Unspecified);
```

### `AsNullableEnum`

```csharp
DateTimeKind? kind = kindString.AsNullableEnum<DateTimeKind>();
```

### `AsUri`

```csharp
Uri uri = "schema://username:password@example.com:123/path/data?key=value#fragid".AsUri();
```

### `AsAtom`

```csharp
Atom atom = "zcode".AsAtom();
```

### `AsAtom`

```csharp
Atom atom = "zcode".AsAtom(Atom.ZERO);
```

### `AsNullableAtom`

```csharp
Atom? atom = "zcode".AsNullableAtom();
```

### `AsEntityId`

```csharp
EntityId id = "comp.gdid@g8corp::4278192640:0:14743786282287111".AsEntityId();
```

### `AsEntityId`

```csharp
EntityId id = "comp.gdid@g8corp::4278192640:0:14743786282287111".AsEntityId("comp.gdid@g8corp::1:0:1");
```

### `AsNullableEntityId`

```csharp
EntityId? id = "comp.gdid@g8corp::4278192640:0:14743786282287111".AsNullableEntityId();
```

### `AsType`

Tries to get a string value as specified type.
When 'strict=false', tries to do some inference like return "true" for numbers that dont equal to zero etc.
When 'strict=true' throws an exception if deterministic conversion is not possible

```csharp
Guid uid = "CF04F818-6194-48C3-B618-8965ACA4D229".AsType(typeof(Guid));
Short marketId = "10".AsType(typeof(short));
double amt = "-10.0".AsType(typeof(double));
```


## <span id="ObjectValueConversion">`Azos.Data.ObjectValueConversion`</span>

<a href="src/Azos/Data/ObjectValueConversion.cs" target="_blank">View Source</a>

=======================================

Provides extension methods for converting object values to different scalar types

---

### `AsString`

```csharp
string valueDisplay = objectValue.AsString("<NULL>", ConvertErrorHandling.Throw);
```

### `AsNonNullOrEmptyString`

```csharp
string valueDisplay = objectValue.AsString();
```

### `AsLaconicConfig`

```csharp
ConfigSectionNode root = ("meta{"+content+"}").AsLaconicConfig(handling: ConvertErrorHandling.Throw);
```

### `AsJSONConfig`

```csharp
ConfigSectionNode nlsNode = ("{r:"+nlsConf+"}").AsJSONConfig(wrapRootName: null, handling: ConvertErrorHandling.Throw);
```

### `AsXMLConfig`

```csharp
var src = @"
<app a=""1"">3
  <a v=""123""/>
</app>
";

ConfigSectionNode cfg = src.AsXMLConfig(handling: ConvertErrorHandling.Throw);
```

### `AsChar`

```csharp
char charVal = "x".AsChar(handling : ConvertErrorHandling.Throw);
```

### `AsChar`

```csharp
char? charVal = objectVal.AsNullableChar(dflt : null, handling : ConvertErrorHandling.ReturnDefault);
```

### `AsByte`

```csharp
long numObj = 254L;
byte byteVal = numObj.AsByte(handling : ConvertErrorHandling.Throw);
```

### `AsNullableByte`

```csharp
long? numObj = null;
byte? byteVal = numObj.AsNullableByte();
```

### `AsSByte`

```csharp
long numObj = 254L;
sbyte sbyteVal = numObj.AsSByte(handling : ConvertErrorHandling.Throw);
```

### `AsNullableSByte`

```csharp
long? numObj = null;
sbyte? sbyteVal = numObj.AsNullableSByte();
```

### `AsShort`

```csharp
long numObj = 32_000L;
short shortVal = numObj.AsShort();
```

### `AsNullableShort`

```csharp
long? numObj = null;
short? shortVal = numObj.AsNullableShort();
```

### `AsUShort`

```csharp
long numObj = 32_000L;
ushort ushortVal = numObj.AsUShort();
```

### `AsNullableUShort`

```csharp
long? numObj = null;
ushort? ushortVal = numObj.AsNullableUShort();
```

### `AsInt`

```csharp
long numObj = 150_000L;
int intVal = numObj.AsInt();
```

### `AsNullableInt`

```csharp
long? numObj = null;
int? intVal = numObj.AsNullableInt();
```

### `AsUInt`

```csharp
ulong numObj = 150_000UL;
uint uintVal = numObj.AsUInt();
```

### `AsNullableUInt`

```csharp
ulong? numObj = null;
uint? uintVal = numObj.AsNullableUInt();
```

### `AsLong`

```csharp
int numObj = 250_000;
long longVal = numObj.AsLong();
```

### `AsNullableLong`

```csharp
int? numObj = null;
long? longVal = numObj.AsNullableLong();
```

### `AsULong`

```csharp
uint numObj = 250_000U;
ulong ulongVal = numObj.AsULong();
```

### `AsNullableULong`

```csharp
uint? numObj = null;
ulong? ulongVal = numObj.AsNullableULong();
```

### `AsDouble`

```csharp
decimal numObj = 250_000.55M;
double doubleVal = numObj.AsDouble();
```

### `AsNullableDouble`

```csharp
decimal? numObj = null;
double? doubleVal = numObj.AsNullableDouble();
```

### `AsFloat`

```csharp
decimal numObj = 500.55M;
float floatVal = numObj.AsFloat();
```

### `AsNullableFloat`

```csharp
decimal? numObj = null;
float? floatVal = numObj.AsNullableFloat();
```


### `AsDecimal`

```csharp
double numObj = 500.55D;
decimal decimalVal = numObj.AsDecimal();
```

### `AsNullableFloat`

```csharp
double? numObj = null;
decimal? decimalVal = numObj.AsNullableFloat();
```


### `TriStateBool` enum

```csharp
/// <summary>
/// A "hack" enum used to provide tri-state checkbox functionality in some systems, i.e. HTML checkmarks
/// do not understand "nulls". This is a surrogate type not used in server-side programming
/// </summary>
public enum TriStateBool { Unspecified = 0, False = 1, True = 2 }
```

### `AsNullableBool`

```csharp
TriStateBool boolObj = TriStateBool.Unspecified;
bool? boolVal = numObj.AsNullableBool();
```

### `AsBool`

```csharp
double numObj = 1D;
bool boolVal = numObj.AsBool();
```

### `AsNullableBool`

```csharp
double? numObj = null;
decimal? boolVal = numObj.AsNullableBool();
```

### `AsGUID`

```csharp
Guid? guidObj = null;
Guid guidVal = guidObj.AsGUID(Guid.Empty);
```

### `AsNullableGUID`

```csharp
byte[]? guidObj = null;
Guid? guidVal = guidObj.AsNullableGUID();
```

### `AsDateTime`

```csharp
long numObj = utcDate?.ToSecondsSinceUnixEpochStart();
DateTime dateVal = numObj.AsDateTime();
```

### `AsDateTime`

```csharp
long? numObj = null;
DateTime dateVal = numObj.AsDateTime(App.GetUtcNow(), styles : System.Globalization.DateTimeStyles.AssumeUniversal);
```

### `AsNullableDateTime`

```csharp
int? numObj = null;
DateTime? dateVal = numObj.AsNullableDateTime();
```

### `AsGDID`

```csharp
long numObj = 4000000000L;
GDID gdidVal = numObj.AsGDID();
```

### `AsGDID`

```csharp
long? numObj = null;
GDID gdidVal = numObj.AsGDID(GDID.ZERO, ConvertErrorHandling.ReturnDefault);
```

### `AsNullableGDID`

```csharp
long? numObj = null;
GDID? gdidVal = numObj.AsNullableGDID();
```

### `AsTimeSpan`

```csharp
int secondsObj = 120;
TimeSpan timeVal = secondsObj.AsTimeSpan();
```

### `AsTimeSpan`

```csharp
int? secondsObj = null;
TimeSpan timeVal = secondsObj.AsTimeSpan(TimeSpan.Zero);
```

### `AsNullableTimeSpan`

```csharp
int? secondsObj = null;
TimeSpan? timeVal = secondsObj.AsNullableTimeSpan();
```

### `AsEnum<TEnum>`

```csharp
var cmdTypeObj = (byte)CmdType.Write;
CmdType cmdTypeVal = cmdTypeObj.AsEnum();
```

### `AsNullableEnum<TEnum>`

```csharp
string cmdTypeObj = null;
CmdType? cmdTypeVal = cmdTypeObj.AsNullableEnum();
```

### `AsUri`

```csharp
object uriObj = "https://example.com";
Uri uriVal = uriObj.AsUri();
```

### `AsAtom`

```csharp
object atomObj = Atom.Encode("abc123").ID;
Atom atomVal = atomObj.AsAtom();
```

### `AsAtom`

```csharp
object atomObj = "abc123";
Atom atomVal = atomObj.AsAtom(Atom.ZERO);
```

### `AsNullableAtom`

```csharp
object? atomObj = null;
Atom? atomVal = atomObj.AsNullableAtom();
```

### `AsEntityId`

```csharp
object eidObj = "vendor.gdid@ecom::1234";
EntityId eidVal = eidObj.AsEntityId();
```

### `AsEntityId`

```csharp
object eidObj = "vendor.gdid@ecom::1234";
EntityId eidVal = eidObj.AsEntityId(EntityId.EMPTY);
```

### `AsNullableEntityId`

```csharp
object? eidObj = null;
EntityId? eidVal = eidObj.AsNullableEntityId();
```