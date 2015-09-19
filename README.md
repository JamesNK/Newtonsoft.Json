##Let json support "Dollar-Quoted String". 

Dollar-Quoted string is introduced by PostgreSQL. But it not about database. It's a very good idea to express string in source code.
It's has similar effect with xml's CDATA. But Dollar-Quoted string is more powerfull and easier. 

This project can read json with Dollar-Quoted string, write json with Dollar-Quoted string, serialize object to  json with Dollar-Quoted string according C# attributes.

###Dollar-Quoted String:

While the standard syntax for specifying string constants is usually convenient, it can be difficult to understand when the desired string contains many single quotes or backslashes, since each of those must be doubled. To allow more readable queries in such situations, PostgreSQL provides another way, called "dollar quoting", to write string constants. A dollar-quoted string constant consists of a dollar sign ($), an optional "tag" of zero or more characters, another dollar sign, an arbitrary sequence of characters that makes up the string content, a dollar sign, the same tag that began this dollar quote, and a dollar sign. For example, here are two different ways to specify the string "Dianne's horse" using dollar quoting:

```c
$$Dianne's horse$$
$SomeTag$Dianne's horse$SomeTag$
```

Notice that inside the dollar-quoted string, single quotes can be used without needing to be escaped. Indeed, no characters inside a dollar-quoted string are ever escaped: the string content is always written literally. Backslashes are not special, and neither are dollar signs, unless they are part of a sequence matching the opening tag.

It is possible to nest dollar-quoted string constants by choosing different tags at each nesting level. This is most commonly used in writing function definitions. For example:

```c
$function$
BEGIN
    RETURN ($1 ~ $q$[\t\r\n\v\\]$q$);
END;
$function$
```

Here, the sequence $q$[\t\r\n\v\\]$q$ represents a dollar-quoted literal string [\t\r\n\v\\], which will be recognized when the function body is executed by PostgreSQL. But since the sequence does not match the outer dollar quoting delimiter $function$, it is just some more characters within the constant so far as the outer string is concerned.

The tag, if any, of a dollar-quoted string follows the same rules as an unquoted identifier, except that it cannot contain a dollar sign. Tags are case sensitive, so $tag$String content$tag$ is correct, but $TAG$String content$tag$ is not.

A dollar-quoted string that follows a keyword or identifier must be separated from it by whitespace; otherwise the dollar quoting delimiter would be taken as part of the preceding identifier.

###A example of json with "Dollar-Quoted String"

```c
{
  "Config": {
    "Tasks": {
      "Task": [ 
      {
        "IntervalSecondsForCheck": "60",
        "Start": "2014-09-21T05:00:00+08:00",
   property     "LastActiveTime": "2014-09-25T06:12:58.8958167+08:00",
        "Interval": "1.00:00:00",
        "Message": $tag$
hi, !@#$%^&*()_+{}:"|<>?,./;'[]\-=
abc"abc"abc\abc/abc$abc
hi
$tag$
      },
      {
        "IntervalSecondsForCheck": "60",
        "Start": "2014-11-21T05:00:00+08:00",
        $aa$LastActiveTime$aa$: $bb$2013-11-22T07:19:44.5380022+08:00$bb$,
        "Interval": "1.00:00:00",
        "Message": "teset"
      }]
    }
  }   
}
```

###A example of serialize a property by "Dollar-Quoted String"
```c
    public class Test1
    {
        [JsonProperty(DollarTag="aa")]
        public string Name { get; set; }
    }
```

###A example of write all string by "Dollar-Quoted String"
```c
    jsonTextWriter.DollarTag = null;   //don't use "Dollar-Quoted String"
    jsonTextWriter.DollarTag = "";     //use "Dollar-Quoted String" by $$
	jsonTextWriter.DollarTag = "tag";  //use "Dollar-Quoted String" by $tag$
```
	