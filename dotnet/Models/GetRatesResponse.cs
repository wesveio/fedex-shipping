﻿using System;
using System.Collections.Generic;

namespace ShippingUtilities.Models
{
    public class GetRatesResponse
    {
        public string itemId { get; set; }
        public int numberOfPackages { get; set; }
        public string shippingMethod { get; set; }
        public string carrierId { get; set; }
        public decimal price { get; set; }
        public DateTimeOffset estimateDate { get; set; }
        public TimeSpan transitTime { get; set; }
        public List<Schedule> carrierSchedule { get; set; }
        public WeekendAndHolidays weekendAndHolidays { get; set; }
        public List<BusinessHour> carrierBusinessHours { get; set; }
        public string deliveryChannel { get; set; }
        public Destination pickupAddress { get; set; }
    }

    public class Schedule
    {
        public DayOfWeek dayOfWeek { get; set; }
        public TimeSpan timeLimit { get; set; }
    }

    public class WeekendAndHolidays
    {
        public bool saturday { get; set; } = false;
        public bool sunday { get; set; } = false;
        public bool holiday { get; set; } = false;
    }

    public class BusinessHour
    {
        public BusinessHour (DayOfWeek dayOfWeek, TimeSpan openingTime, TimeSpan closingTime)
        {
            this.dayOfWeek = dayOfWeek;
            this.openingTime = openingTime;
            this.closingTime = closingTime;
        }
        public DayOfWeek dayOfWeek { get; set; }
        public TimeSpan openingTime { get; set; }
        public TimeSpan closingTime { get; set; }
    }
}
