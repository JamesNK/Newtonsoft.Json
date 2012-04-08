#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System.Collections.Generic;

namespace Newtonsoft.Json.Tests.TestObjects
{
  public class GoogleMapGeocoderStructure
  {
    public string Name;
    public Status Status;
    public List<Placemark> Placemark;
  }

  public class Status
  {
    public string Request;
    public string Code;
  }

  public class Placemark
  {
    public string Address;
    public AddressDetails AddressDetails;
    public Point Point;
  }

  public class AddressDetails
  {
    public int Accuracy;
    public Country Country;
  }

  public class Country
  {
    public string CountryNameCode;
    public AdministrativeArea AdministrativeArea;
  }

  public class AdministrativeArea
  {
    public string AdministrativeAreaName;
    public SubAdministrativeArea SubAdministrativeArea;
  }

  public class SubAdministrativeArea
  {
    public string SubAdministrativeAreaName;
    public Locality Locality;
  }

  public class Locality
  {
    public string LocalityName;
    public Thoroughfare Thoroughfare;
    public PostalCode PostalCode;
  }

  public class Thoroughfare
  {
    public string ThoroughfareName;
  }

  public class PostalCode
  {
    public string PostalCodeNumber;
  }

  public class Point
  {
    public List<decimal> Coordinates;
  }
}