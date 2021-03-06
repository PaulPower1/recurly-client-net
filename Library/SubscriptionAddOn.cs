﻿using System;
using System.Xml;

namespace Recurly
{
    public class SubscriptionAddOn : RecurlyEntity
    {
        public string AddOnCode { get; set; }
        public AddOn.Type? AddOnType { get; set; }
        public int UnitAmountInCents { get; set; }
        public int Quantity { get; set; }
        public Adjustment.RevenueSchedule? RevenueScheduleType { get; set; }

        public SubscriptionAddOn(XmlTextReader reader)
        {
            ReadXml(reader);
        }

        // keep old constructor
        public SubscriptionAddOn(string addOnCode, int unitAmountInCents, int quantity = 1)
        {
            AddOnCode = addOnCode;
            UnitAmountInCents = unitAmountInCents;
            Quantity = quantity;
        }

        // new constructor including addOnType (recommended)
        public SubscriptionAddOn(string addOnCode, AddOn.Type? addOnType, int unitAmountInCents, int quantity = 1)
        {
            AddOnCode = addOnCode;
            AddOnType = addOnType;
            UnitAmountInCents = unitAmountInCents;
            Quantity = quantity;
        }

        internal override void ReadXml(XmlTextReader reader)
        {
            while (reader.Read())
            {
                if (reader.Name == "subscription_add_on" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                if (reader.NodeType != XmlNodeType.Element) continue;

                switch (reader.Name)
                {
                    case "add_on_code":
                        AddOnCode = reader.ReadElementContentAsString();
                        break;

                    case "add_on_type":
                        AddOnType = reader.ReadElementContentAsString().ParseAsEnum<AddOn.Type>();
                        break;

                    case "quantity":
                        Quantity = reader.ReadElementContentAsInt();
                        break;

                    case "unit_amount_in_cents":
                        int unitAmountInCents;
                        if (Int32.TryParse(reader.ReadElementContentAsString(), out unitAmountInCents))
                            UnitAmountInCents = unitAmountInCents;
                        break;

                    case "revenue_schedule_type":
                        var revenueScheduleType = reader.ReadElementContentAsString();
                        if (!revenueScheduleType.IsNullOrEmpty())
                            RevenueScheduleType = revenueScheduleType.ParseAsEnum<Adjustment.RevenueSchedule>();
                        break;
                }
            }
        }

        internal override void WriteXml(XmlTextWriter writer)
        {
            writer.WriteStartElement("subscription_add_on");

            writer.WriteElementString("add_on_code", AddOnCode);
            writer.WriteElementString("quantity", Quantity.AsString());			
            writer.WriteElementString("unit_amount_in_cents", UnitAmountInCents.AsString());

            if (RevenueScheduleType.HasValue)
                writer.WriteElementString("revenue_schedule_type", RevenueScheduleType.Value.ToString().EnumNameToTransportCase());

            writer.WriteEndElement();
        }
    }
}
