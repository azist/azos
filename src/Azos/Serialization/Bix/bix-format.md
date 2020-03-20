# Bix Format

## Grammar
Grammar of Bit Exchange data format.

```
 STREAM = [header] doc_value;
 header = 0xAA, 1 = root polymorphism | 2 = field/array polymorphism
```

A field is a named value:
```
 FIELD  = name value | 0x00 (null name = eom);
 name = atom
```

A value is a piece of typed data:
```
 VALUE = type data;
 type = (byte) WireType;
 data = bix_format_data;
```

Every data document has an optional type header and a stream of fields:
```
 DOC_VALUE = null_flag [doc_type][{field 1 [...[field x]}] eom;
 doc_type = (guid_hi, guid_low);
 null_flag = bool;
 eom = bool;
```


