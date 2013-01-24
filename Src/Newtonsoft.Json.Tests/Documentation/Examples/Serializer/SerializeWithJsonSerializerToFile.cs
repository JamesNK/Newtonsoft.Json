using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using File = System.IO.File;

namespace Newtonsoft.Json.Tests.Documentation.Examples.Serializer
{
  public class SerializeWithJsonSerializerToFile
  {
    public class Movie
    {
      public string Name { get; set; }
      public int Year { get; set; }
    }

    public void Example()
    {
      Movie movie = new Movie
        {
          Name = "Bad Boys",
          Year = 1995
        };

      // serialize JSON to a string and then write string to a file
      File.WriteAllText(@"c:\movie.json", JsonConvert.SerializeObject(movie));

      // serialize JSON directly to a file
      using (StreamWriter file = File.CreateText(@"c:\movie.json"))
      {
        JsonSerializer serializer = new JsonSerializer();
        serializer.Serialize(file, movie);
      }
    }
  }
}