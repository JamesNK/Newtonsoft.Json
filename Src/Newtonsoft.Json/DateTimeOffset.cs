#if PocketPC
#pragma warning disable 1591

// This class is... borrowed from .NET and Microsoft for a short time.
// Hopefully Microsoft will add DateTimeOffset to the compact framework
// or I will rewrite a striped down version of this file myself

using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System;

namespace System
{
  public struct DateTimeOffset : IComparable, IFormattable, IComparable<DateTimeOffset>, IEquatable<DateTimeOffset>
  {
    private DateTime _dateTime;
    private short _offsetMinutes;
    internal const long MaxOffset = 0x7558bdb000L;
    public static readonly DateTimeOffset MaxValue;
    internal const long MinOffset = -504000000000L;
    public static readonly DateTimeOffset MinValue;

    static DateTimeOffset()
    {
      MinValue = new DateTimeOffset(0L, TimeSpan.Zero);
      MaxValue = new DateTimeOffset(0x2bca2875f4373fffL, TimeSpan.Zero);
    }

    public DateTimeOffset(System.DateTime dateTime)
    {
      TimeSpan utcOffset;
      if (dateTime.Kind != DateTimeKind.Utc)
      {
        utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(dateTime);
      }
      else
      {
        utcOffset = new TimeSpan(0L);
      }
      this._offsetMinutes = ValidateOffset(utcOffset);
      this._dateTime = ValidateDate(dateTime, utcOffset);
    }

    public DateTimeOffset(System.DateTime dateTime, TimeSpan offset)
    {
      if (dateTime.Kind == DateTimeKind.Local)
      {
        if (offset != TimeZone.CurrentTimeZone.GetUtcOffset(dateTime))
        {
          throw new ArgumentException("The UTC Offset of the local dateTime parameter does not match the offset argument.", "offset");
        }
      }
      else if ((dateTime.Kind == DateTimeKind.Utc) && (offset != TimeSpan.Zero))
      {
        throw new ArgumentException("The UTC Offset for Utc DateTime instances must be 0.", "offset");
      }
      this._offsetMinutes = ValidateOffset(offset);
      this._dateTime = ValidateDate(dateTime, offset);
    }

    public DateTimeOffset(long ticks, TimeSpan offset)
    {
      this._offsetMinutes = ValidateOffset(offset);
      System.DateTime dateTime = new System.DateTime(ticks);
      this._dateTime = ValidateDate(dateTime, offset);
    }

    public DateTimeOffset(int year, int month, int day, int hour, int minute, int second, TimeSpan offset)
    {
      this._offsetMinutes = ValidateOffset(offset);
      this._dateTime = ValidateDate(new System.DateTime(year, month, day, hour, minute, second), offset);
    }

    public DateTimeOffset(int year, int month, int day, int hour, int minute, int second, int millisecond, TimeSpan offset)
    {
      this._offsetMinutes = ValidateOffset(offset);
      this._dateTime = ValidateDate(new System.DateTime(year, month, day, hour, minute, second, millisecond), offset);
    }

    public DateTimeOffset(int year, int month, int day, int hour, int minute, int second, int millisecond, Calendar calendar, TimeSpan offset)
    {
      this._offsetMinutes = ValidateOffset(offset);
      this._dateTime = ValidateDate(new System.DateTime(year, month, day, hour, minute, second, millisecond, calendar), offset);
    }

    public DateTimeOffset Add(TimeSpan timeSpan)
    {
      return new DateTimeOffset(this.ClockDateTime.Add(timeSpan), this.Offset);
    }

    public DateTimeOffset AddDays(double days)
    {
      return new DateTimeOffset(this.ClockDateTime.AddDays(days), this.Offset);
    }

    public DateTimeOffset AddHours(double hours)
    {
      return new DateTimeOffset(this.ClockDateTime.AddHours(hours), this.Offset);
    }

    public DateTimeOffset AddMilliseconds(double milliseconds)
    {
      return new DateTimeOffset(this.ClockDateTime.AddMilliseconds(milliseconds), this.Offset);
    }

    public DateTimeOffset AddMinutes(double minutes)
    {
      return new DateTimeOffset(this.ClockDateTime.AddMinutes(minutes), this.Offset);
    }

    public DateTimeOffset AddMonths(int months)
    {
      return new DateTimeOffset(this.ClockDateTime.AddMonths(months), this.Offset);
    }

    public DateTimeOffset AddSeconds(double seconds)
    {
      return new DateTimeOffset(this.ClockDateTime.AddSeconds(seconds), this.Offset);
    }

    public DateTimeOffset AddTicks(long ticks)
    {
      return new DateTimeOffset(this.ClockDateTime.AddTicks(ticks), this.Offset);
    }

    public DateTimeOffset AddYears(int years)
    {
      return new DateTimeOffset(this.ClockDateTime.AddYears(years), this.Offset);
    }

    public static int Compare(DateTimeOffset first, DateTimeOffset second)
    {
      return System.DateTime.Compare(first.UtcDateTime, second.UtcDateTime);
    }

    public int CompareTo(DateTimeOffset other)
    {
      System.DateTime utcDateTime = other.UtcDateTime;
      System.DateTime time2 = this.UtcDateTime;
      if (time2 > utcDateTime)
      {
        return 1;
      }
      if (time2 < utcDateTime)
      {
        return -1;
      }
      return 0;
    }

    public bool Equals(DateTimeOffset other)
    {
      return this.UtcDateTime.Equals(other.UtcDateTime);
    }

    public override bool Equals(object obj)
    {
      if (obj is DateTimeOffset)
      {
        DateTimeOffset offset = (DateTimeOffset)obj;
        return this.UtcDateTime.Equals(offset.UtcDateTime);
      }
      return false;
    }

    public static bool Equals(DateTimeOffset first, DateTimeOffset second)
    {
      return System.DateTime.Equals(first.UtcDateTime, second.UtcDateTime);
    }

    public bool EqualsExact(DateTimeOffset other)
    {
      return (((this.ClockDateTime == other.ClockDateTime) && (this.Offset == other.Offset)) && (this.ClockDateTime.Kind == other.ClockDateTime.Kind));
    }

    public static DateTimeOffset FromFileTime(long fileTime)
    {
      return new DateTimeOffset(System.DateTime.FromFileTime(fileTime));
    }

    public override int GetHashCode()
    {
      return this.UtcDateTime.GetHashCode();
    }

    public static DateTimeOffset operator +(DateTimeOffset dateTimeTz, TimeSpan timeSpan)
    {
      return new DateTimeOffset(dateTimeTz.ClockDateTime + timeSpan, dateTimeTz.Offset);
    }

    public static bool operator ==(DateTimeOffset left, DateTimeOffset right)
    {
      return (left.UtcDateTime == right.UtcDateTime);
    }

    public static bool operator >(DateTimeOffset left, DateTimeOffset right)
    {
      return (left.UtcDateTime > right.UtcDateTime);
    }

    public static bool operator >=(DateTimeOffset left, DateTimeOffset right)
    {
      return (left.UtcDateTime >= right.UtcDateTime);
    }

    public static implicit operator DateTimeOffset(System.DateTime dateTime)
    {
      return new DateTimeOffset(dateTime);
    }

    public static bool operator !=(DateTimeOffset left, DateTimeOffset right)
    {
      return (left.UtcDateTime != right.UtcDateTime);
    }

    public static bool operator <(DateTimeOffset left, DateTimeOffset right)
    {
      return (left.UtcDateTime < right.UtcDateTime);
    }

    public static bool operator <=(DateTimeOffset left, DateTimeOffset right)
    {
      return (left.UtcDateTime <= right.UtcDateTime);
    }

    public static TimeSpan operator -(DateTimeOffset left, DateTimeOffset right)
    {
      return (TimeSpan)(left.UtcDateTime - right.UtcDateTime);
    }

    public static DateTimeOffset operator -(DateTimeOffset dateTimeTz, TimeSpan timeSpan)
    {
      return new DateTimeOffset(dateTimeTz.ClockDateTime - timeSpan, dateTimeTz.Offset);
    }

    public static DateTimeOffset Parse(string input)
    {
      return new DateTimeOffset(DateTime.Parse(input, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None));
    }

    public static DateTimeOffset Parse(string input, IFormatProvider formatProvider)
    {
      return Parse(input, formatProvider, DateTimeStyles.None);
    }

    public static DateTimeOffset Parse(string input, IFormatProvider formatProvider, DateTimeStyles styles)
    {
      styles = ValidateStyles(styles, "styles");
      return new DateTimeOffset(DateTime.Parse(input, DateTimeFormatInfo.GetInstance(formatProvider), styles));
    }

    public static DateTimeOffset ParseExact(string input, string format, IFormatProvider formatProvider)
    {
      return ParseExact(input, format, formatProvider, DateTimeStyles.None);
    }

    public static DateTimeOffset ParseExact(string input, string format, IFormatProvider formatProvider, DateTimeStyles styles)
    {
      styles = ValidateStyles(styles, "styles");
      return new DateTimeOffset(DateTime.ParseExact(input, format, DateTimeFormatInfo.GetInstance(formatProvider), styles));
    }

    public static DateTimeOffset ParseExact(string input, string[] formats, IFormatProvider formatProvider, DateTimeStyles styles)
    {
      styles = ValidateStyles(styles, "styles");
      return new DateTimeOffset(DateTime.ParseExact(input, formats, DateTimeFormatInfo.GetInstance(formatProvider), styles));
    }

    public TimeSpan Subtract(DateTimeOffset value)
    {
      return this.UtcDateTime.Subtract(value.UtcDateTime);
    }

    public DateTimeOffset Subtract(TimeSpan value)
    {
      return new DateTimeOffset(this.ClockDateTime.Subtract(value), this.Offset);
    }

    int IComparable.CompareTo(object obj)
    {
      if (obj == null)
      {
        return 1;
      }
      if (!(obj is DateTimeOffset))
      {
        throw new ArgumentException("Object must be of type DateTimeOffset.");
      }
      DateTimeOffset offset = (DateTimeOffset)obj;
      System.DateTime utcDateTime = offset.UtcDateTime;
      System.DateTime time2 = this.UtcDateTime;
      if (time2 > utcDateTime)
      {
        return 1;
      }
      if (time2 < utcDateTime)
      {
        return -1;
      }
      return 0;
    }

    public long ToFileTime()
    {
      return this.UtcDateTime.ToFileTime();
    }

    public DateTimeOffset ToLocalTime()
    {
      return new DateTimeOffset(this.UtcDateTime.ToLocalTime());
    }

    public DateTimeOffset ToOffset(TimeSpan offset)
    {
      System.DateTime time = this._dateTime + offset;
      return new DateTimeOffset(time.Ticks, offset);
    }

    public string ToString(string format, IFormatProvider formatProvider)
    {
      if (!string.IsNullOrEmpty(format))
      {
        format = format.Replace("K", "zzz");
        format = format.Replace("zzz", Offset.Hours.ToString("+00;-00", CultureInfo.InvariantCulture) + ":" + Offset.Minutes.ToString("00;00", CultureInfo.InvariantCulture));
        format = format.Replace("zz", Offset.Hours.ToString("+00;-00", CultureInfo.InvariantCulture));
        format = format.Replace("z", Offset.Hours.ToString("+0;-0", CultureInfo.InvariantCulture));
      }

      return ClockDateTime.ToString(format, formatProvider);
    }

    public DateTimeOffset ToUniversalTime()
    {
      return new DateTimeOffset(this.UtcDateTime);
    }

    private static System.DateTime ValidateDate(System.DateTime dateTime, TimeSpan offset)
    {
      long ticks = dateTime.Ticks - offset.Ticks;
      if ((ticks < 0L) || (ticks > 0x2bca2875f4373fffL))
      {
        throw new ArgumentOutOfRangeException("offset", "The UTC time represented when the offset is applied must be between year 0 and 10,000.");
      }
      return new System.DateTime(ticks, DateTimeKind.Unspecified);
    }

    private static short ValidateOffset(TimeSpan offset)
    {
      long ticks = offset.Ticks;
      if ((ticks % 0x23c34600L) != 0L)
      {
        throw new ArgumentException("Offset must be specified in whole minutes.", "offset");
      }
      if ((ticks < -504000000000L) || (ticks > 0x7558bdb000L))
      {
        throw new ArgumentOutOfRangeException("offset", "Offset must be within plus or minus 14 hours.");
      }
      return (short)(offset.Ticks / 0x23c34600L);
    }

    private static DateTimeStyles ValidateStyles(DateTimeStyles style, string parameterName)
    {
      if ((style & ~(DateTimeStyles.RoundtripKind | DateTimeStyles.AssumeUniversal | DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal | DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AllowWhiteSpaces)) != DateTimeStyles.None)
      {
        throw new ArgumentException("An undefined DateTimeStyles value is being used.", parameterName);
      }
      if (((style & DateTimeStyles.AssumeLocal) != DateTimeStyles.None) && ((style & DateTimeStyles.AssumeUniversal) != DateTimeStyles.None))
      {
        throw new ArgumentException("The DateTimeStyles values AssumeLocal and AssumeUniversal cannot be used together.", parameterName);
      }
      if ((style & DateTimeStyles.NoCurrentDateDefault) != DateTimeStyles.None)
      {
        throw new ArgumentException("The DateTimeStyles value 'NoCurrentDateDefault' is not allowed when parsing DateTimeOffset.", parameterName);
      }
      style &= ~DateTimeStyles.RoundtripKind;
      style &= ~DateTimeStyles.AssumeLocal;
      return style;
    }

    private System.DateTime ClockDateTime
    {
      get
      {
        System.DateTime time = this._dateTime + this.Offset;
        return new System.DateTime(time.Ticks, DateTimeKind.Unspecified);
      }
    }

    public System.DateTime Date
    {
      get
      {
        return this.ClockDateTime.Date;
      }
    }

    public System.DateTime DateTime
    {
      get
      {
        return this.ClockDateTime;
      }
    }

    public int Day
    {
      get
      {
        return this.ClockDateTime.Day;
      }
    }

    public System.DayOfWeek DayOfWeek
    {
      get
      {
        return this.ClockDateTime.DayOfWeek;
      }
    }

    public int DayOfYear
    {
      get
      {
        return this.ClockDateTime.DayOfYear;
      }
    }

    public int Hour
    {
      get
      {
        return this.ClockDateTime.Hour;
      }
    }

    public System.DateTime LocalDateTime
    {
      get
      {
        return this.UtcDateTime.ToLocalTime();
      }
    }

    public int Millisecond
    {
      get
      {
        return this.ClockDateTime.Millisecond;
      }
    }

    public int Minute
    {
      get
      {
        return this.ClockDateTime.Minute;
      }
    }

    public int Month
    {
      get
      {
        return this.ClockDateTime.Month;
      }
    }

    public static DateTimeOffset Now
    {
      get
      {
        return new DateTimeOffset(System.DateTime.Now);
      }
    }

    public TimeSpan Offset
    {
      get
      {
        return new TimeSpan(0, this._offsetMinutes, 0);
      }
    }

    public int Second
    {
      get
      {
        return this.ClockDateTime.Second;
      }
    }

    public long Ticks
    {
      get
      {
        return this.ClockDateTime.Ticks;
      }
    }

    public TimeSpan TimeOfDay
    {
      get
      {
        return this.ClockDateTime.TimeOfDay;
      }
    }

    public System.DateTime UtcDateTime
    {
      get
      {
        return System.DateTime.SpecifyKind(this._dateTime, DateTimeKind.Utc);
      }
    }

    public static DateTimeOffset UtcNow
    {
      get
      {
        return new DateTimeOffset(System.DateTime.UtcNow);
      }
    }

    public long UtcTicks
    {
      get
      {
        return this.UtcDateTime.Ticks;
      }
    }

    public int Year
    {
      get
      {
        return this.ClockDateTime.Year;
      }
    }
  }
}
#endif