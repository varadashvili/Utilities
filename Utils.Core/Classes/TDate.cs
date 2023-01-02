using System;
using System.Collections.Generic;
using System.Text;

using Utils.Core.Code;
using Utils.Core.StaticData;

namespace Utils.Core.Classes
{
    /// <summary>
    /// custom date object, uses nodatime package.
    /// if used as method parameter, one of DateTicks, DateUnixMilliseconds, DateUnixMillisecondsUTC should be specified.
    /// method parameter exlample {"DateTicks":637744041335435706}
    /// </summary>
    public class TDate
    {
        private DateTime? _DateValue = null;

        public TDate()
        {
            _DateValue = null;
            DateTicks = (long?)null;
            DateUnixMilliseconds = (double?)null;
            DateUnixMillisecondsUTC = (double?)null;
            DateString = null;
        }

        public TDate(DateTime? dt)
        {
            _DateValue = dt.HasValue ? DateTime.SpecifyKind(dt.Value, DateTimeKind.Local) : (DateTime?)null;
            DateTicks = _DateValue.HasValue ? _DateValue.Value.Ticks : (long?)null;
            DateUnixMilliseconds = _DateValue.ToUnixMilliseconds();
            DateUnixMillisecondsUTC = _DateValue.ToUnixMillisecondsUTC();
            DateString = _DateValue.HasValue ? _DateValue.Value.ToString("u") : null;
        }

        /// <summary>
        /// desc: date value (readonly)
        /// </summary>
        public DateTime? DateValue
        {
            get
            {
                var dt1 = _DateValue;

                var dt2 = DateTicks > StaticDataShared.Jan1st1980.Value.AddYears(-50).Ticks ?
                    new DateTime(DateTicks.Value, DateTimeKind.Local) : (DateTime?)null;

                var dt3 = DateUnixMilliseconds > (StaticDataShared.Jan1st1980?.AddYears(-50)).ToUnixMilliseconds() ?
                    UtilitiesShared.FromUnixMilliseconds(DateUnixMilliseconds) : (DateTime?)null;

                var dt4 = DateUnixMillisecondsUTC > (StaticDataShared.Jan1st1980?.AddYears(-50)).ToUnixMillisecondsUTC() ?
                    UtilitiesShared.FromUnixMillisecondsUTC(DateUnixMillisecondsUTC) : (DateTime?)null;

                dt1 = dt1 ?? dt2 ?? dt3 ?? dt4;

                if (!dt1.HasValue)
                {
                    return null;
                }


                dt2 = dt2 ?? dt1;
                dt3 = dt3 ?? dt1;
                dt4 = dt4 ?? dt1;

                var dt1Milliseconds = dt1.ToUnixMillisecondsUTC().Value;
                var dt2Milliseconds = dt2.ToUnixMillisecondsUTC().Value;
                var dt3Milliseconds = dt3.ToUnixMillisecondsUTC().Value;
                var dt4Milliseconds = dt4.ToUnixMillisecondsUTC().Value;



                if (Math.Abs(dt1Milliseconds - dt2Milliseconds) > 30000 || Math.Abs(dt1Milliseconds - dt3Milliseconds) > 30000 ||
                    Math.Abs(dt1Milliseconds - dt4Milliseconds) > 30000)
                {
                    throw new Exception("values mismach");
                }

                _DateValue = dt1;

                return dt1;
            }

            //set
            //{
            //    _DateValue = value;
            //    DateTicks = _DateValue.HasValue ? _DateValue.Value.Ticks : (long?)null;
            //    DateUnixMilliseconds = _DateValue.ToUnixMilliseconds();
            //    //DateUnixMillisecondsUTC = _DateValue.ToUnixMillisecondsUTC();
            //    DateString = _DateValue.HasValue ? _DateValue.Value.ToString("u") : null;
            //}
        }

        /// <summary>
        /// desc: date to c# date ticks (local date)
        /// </summary>
        /// <example>637744041335435706</example>
        public long? DateTicks { get; set; }

        /// <summary>
        /// date to local unix milliseconds for java
        /// </summary>
        /// <example>1638807333543</example>
        public double? DateUnixMilliseconds { get; set; }

        /// <summary>
        /// date to utc unix milliseconds for java
        /// </summary>
        /// <example>1638792933543</example>
        public double? DateUnixMillisecondsUTC { get; set; }

        /// <summary>
        /// date value in sortable string  (readonly)
        /// </summary>
        public string DateString { get; }
    }
}
