using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
  public class DeserializeConstructorHandling
  {
    #region Types
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
    #endregion

    public void Example()
    {
      #region Usage
      string json = @"{'Url':'http://www.google.com'}";

      try
      {
        JsonConvert.DeserializeObject<Website>(json);
      }
      catch (TargetInvocationException ex)
      {
        Console.WriteLine(ex);
        // Value cannot be null.
        // Parameter name: website
      }
      
      Website website = JsonConvert.DeserializeObject<Website>(json, new JsonSerializerSettings
        {
          ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        });

      Console.WriteLine(website.Url);
      // http://www.google.com
      #endregion
    }
  }
}