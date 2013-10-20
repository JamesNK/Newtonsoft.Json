using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using File = System.IO.File;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    public class DeserializeWithJsonSerializerFromFile
    {
        #region Types
        public class Movie
        {
            public string Name { get; set; }
            public int Year { get; set; }
        }
        #endregion

        public void Example()
        {
            #region Usage
            // read file into a string and deserialize JSON to a type
            Movie movie1 = JsonConvert.DeserializeObject<Movie>(File.ReadAllText(@"c:\movie.json"));

            // deserialize JSON directly from a file
            using (StreamReader file = File.OpenText(@"c:\movie.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                Movie movie2 = (Movie)serializer.Deserialize(file, typeof(Movie));
            }
            #endregion
        }
    }
}