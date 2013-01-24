using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Examples.Serializer
{
  public class DeserializeConstructorHandling
  {
    public class Website
    {
      public string Url { get; set; }

      private Website()
      {
      }

      public Website(Website website)
      {
        if (website == null)
          throw new ArgumentNullException("website");

        Url = website.Url;
      }
    }

    public void Example()
    {
      string json = @"{""Url"":""http://www.google.com""}";

      try
      {
        JsonConvert.DeserializeObject<Website>(json);
      }
      catch (TargetInvocationException)
      {
        // Value cannot be null.
        // Parameter name: website
      }
      
      Website website = JsonConvert.DeserializeObject<Website>(json, new JsonSerializerSettings
        {
          ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        });

      Console.WriteLine(website.Url);
      // http://www.google.com
    }
  }
}