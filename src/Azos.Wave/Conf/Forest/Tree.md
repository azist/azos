# Config Tree Management Endpoint

## Configuration Forest

The Configuration Forest is a version-controlled (like Git) repository of named hierarchies 
called trees of structured configuration files having their content attributes and sections 
inherited from the higher tree hierarchy levels. These config files are identified by a full 
"node path" which are a kin to file system files. 

A **Config Forest** may be described as a distributed version-controlled file system of 
structured config files with file content inheritance capabilities.

## Endpoints

### /conf/forest/tree/tree-list/ GET

`{$title}`
`{$description}`

The example below is used to get a list of trees in a named forest (datastore mapping).

**Request Sample:**
```
# Example #1 - Get a List (collection) of Trees in a Forest
# *** Optional `asofutc` querystring parameter ***
###
GET http://{{HOST}}:{{PORT}}/conf/forest/tree/tree-list?forest=g8corp HTTP/1.2
Authorization: {{AUTH}}
Accept: application/json
```

**Response Sample:**
```
{
  "data": [
    "reg"
  ],
  "OK": true
}
```


### /conf/forest/tree/node-list/ GET

`{$title}`
`{$description}`


The below example GETs a List (collection) of child nodes (headers only) for the root path `/`. The example below
uses an EntityId path syntax to specify the forest `g8corp`, tree `reg`, by `path`, at the root path value of `/`.
The request also accepts the default value of UTC now `asofutc` and default (false) caching `nocache` value
(to turn off caching provide a `nocache=true` query string parameter).

**Note** - the URL Encoded value for `/` is expressed as `%2F` in request query strings

**Request Sample:**
```
GET http://{{HOST}}:{{PORT}}/conf/forest/tree/node-list?idparent=reg.path@g8corp::%2F HTTP/1.2
Authorization: {{AUTH}}
Accept: application/json
```

**Response Sample:** (See `{@TreeNodeHeader}`) 
```
{
  "data": [
    {
      "Id": "reg.gnode@g8corp::4278192640:0:16272655858532355",
      "G_Version": "4278192640:0:16272656126967811",
      "PathSegment": "product",
      "StartUtc": "2021-01-01T05:00:00Z"
    }
  ],
  "OK": true
}
```


### /conf/forest/tree/version-list/ GET

`{$title}`
`{$description}`


Retrieves the list of all node version info objects (G_Version, Utc, Origin, Actor, State) 
by it's Entity ID (`gnode` schema **ONLY**!). 

**Request Sample:**
```
GET http://{{HOST}}:{{PORT}}/conf/forest/tree/version-list?id=reg.gnode@g8corp::4278192640:0:16272655858532357 HTTP/1.2
Authorization: {{AUTH}}
Accept: application/json
```

**Response Sample:** (See `{@VersionInfo}`)
```
{
  "data": [
    {
      "G_Version": "4278192640:0:16272656126967813",
      "Utc": "2021-12-02T15:48:00-05:00",
      "Origin": "local",
      "Actor": "usrn@idp::root",
      "State": "Created"
    },
    {
      "G_Version": "4278192640:0:16272656126967814",
      "Utc": "2021-12-02T17:29:55-05:00",
      "Origin": "local",
      "Actor": "usrn@idp::root",
      "State": "Updated"
    }
  ],
  "OK": true
}
```


### /conf/forest/tree/version/ GET

`{$title}`
`{$description}`


Retrieves a specific TreeNodeInfo version object (read-only node info) by it's Entity ID (`gver` schema **ONLY**!).

**Request Sample:**
```
GET http://{{HOST}}:{{PORT}}/conf/forest/tree/version?id=reg.gver@g8corp::4278192640:0:16272656126967814 HTTP/1.2
Authorization: {{AUTH}}
Accept: application/json
```

**Response Sample:** (See `{@TreeNodeInfo}`)
```
{
  "data": {
    "Forest": "g8corp",
    "Tree": "reg",
    "Gdid": "4278192640:0:16272655858532357",
    "G_Parent": "4278192640:0:16272655858532355",
    "Id": "reg.gnode@g8corp::4278192640:0:16272655858532357",
    "ParentId": "reg.gnode@g8corp::4278192640:0:16272655858532355",
    "PathSegment": "SKU",
    "FullPath": "[n\/a for version specific node]",
    "FullPathId": "reg.path@g8corp::[n\/a for version specific node]",
    "DataVersion": {
      "G_Version": "4278192640:0:16272656126967814",
      "Utc": "2021-12-02T17:29:55-05:00",
      "Origin": "local",
      "Actor": "usrn@idp::root",
      "State": "Updated"
    },
    "StartUtc": "2021-02-01T05:00:00-05:00",
    "Properties": "{\"r\":{\"test\":\"v1\"}}",
    "LevelConfig": "{\"r\":{\"name\":\"tuck\"}}",
    "EffectiveConfig": null
  },
  "OK": true
}
```

**Note** - FullPath and EffectiveConfig data are not available on TreeNodeInfo version objects!


### /conf/forest/tree/node/ GET

`{$title}`
`{$description}`


Retrieves a specific TreeNodeInfo object (read-only node info) by it's Entity ID as of a specific date.

**Query String Parameters**:

id = `reg.path@g8corp::%2Fproduct%2Fsku%2F` or `reg.gnode@g8corp::4278192640:0:16272655858532357` (**Required**)

asofutc = `2021-12-01T00:00:00-05:00` (defaults to UTC now)

nocache = `true` (defaults to false)

**Note** - the URL Encoded value for `/` is expressed as `%2F` in request query strings

**Request Sample:**
```
GET http://{{HOST}}:{{PORT}}/conf/forest/tree/node?id=reg.gnode@g8corp::4278192640:0:16272655858532357&nocache=true HTTP/1.2
Authorization: {{AUTH}}
Accept: application/json
```

**Response Sample:** (See `{@TreeNodeInfo}`)
```
{
  "data": {
    "Forest": "g8corp",
    "Tree": "reg",
    "Gdid": "4278192640:0:16272655858532357",
    "G_Parent": "4278192640:0:16272655858532355",
    "Id": "reg.gnode@g8corp::4278192640:0:16272655858532357",
    "ParentId": "reg.gnode@g8corp::4278192640:0:16272655858532355",
    "PathSegment": "SKU",
    "FullPath": "\/product\/SKU",
    "FullPathId": "reg.path@g8corp::\/product\/SKU",
    "DataVersion": {
      "G_Version": "4278192640:0:16272656126967814",
      "Utc": "2021-12-02T17:29:55-05:00",
      "Origin": "local",
      "Actor": "usrn@idp::root",
      "State": "Updated"
    },
    "StartUtc": "2021-02-01T05:00:00-05:00",
    "Properties": "{\"r\":{\"test\":\"v1\"}}",
    "LevelConfig": "{\"r\":{\"name\":\"tuck\"}}",
    "EffectiveConfig": "{\"r\":{\"name\":\"tuck\"}}"
  },
  "OK": true
}
```

**Note** - FullPath and EffectiveConfig data elements are calculated as of the specified date. 
The node will ONLY be returned if all ancestor parent nodes are also effective on the specified date.