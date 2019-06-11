# TestController Api Scope

Here comes a paragraph describing this controller.
This paragraph spans many lines  and different
formatting can
be used if needed.

## How Doc File Works
Doc file is just a markdown file with headings.
The `Endpoint` heading has a special meaning - it is being regenerated of the endpoints data in doc set.
The content will be replaced with generated content using individual endpoint sections as template.

You can use variables in endpoints: `{<path>}` the path is per config navigation syntax, so for example to get 
a title of the endpoint one could use ` {$title} `. The variables are evaluated only in Endpoints section.
You can escape ``{{variable}}`` like so.



## Security
The controller is protected by permissions and requires snake oil to work

## Endpoints

### custom name
This is a content for custom-named doc anchor

Uri for this is `{$uri}`.

### /test/list - `{$title}`
ZZZZZZZZZZZZZZZZZZZZZZZZZ: `{$title}`

Url is: `{$uri}`
Returns an array of `@xyz` for the supplied `@TestFilter`. The content is posted

Example C# code:
```csharp
 var x = 1;
 call.service(now);//call them today
```

## Notes
None, it is very **easy** to use

a|b|c
-|-|-
1|2|3
d|t|e
-|-|-

1. list
1. list
1. list

ssssssssssssssssss
  - list
- a 