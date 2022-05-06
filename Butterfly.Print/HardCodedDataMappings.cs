namespace Butterfly.Print
{
    using System;
    using Butterfly.Print.PrintJobObjects;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class HardCodedDataMappings
    {
        public static void RunHardCodedDataMapping(PrintJobLayout printJobLayout, object entityObj)
        {
            try
            {
                string serializedEntityObj = JsonConvert.SerializeObject(entityObj);
                JObject entity = (JObject)JsonConvert.DeserializeObject(serializedEntityObj);

                switch (printJobLayout.LayoutName)
                {
                    case "LYB_AI":
                        RunHardCodedDataMapping_LYB_AI(printJobLayout, entity);
                        break;

                    case "TestSimpleAssignment":
                        RunHardCodedDataMapping_TestSimpleAssignment(printJobLayout, entity);
                        break;

                    default:
                        throw new Exception(string.Format("Hardcoded DataMapping for layout '{0}' not implemented.", printJobLayout.LayoutName));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("RunHardCodedDataMapping failed.", ex);
            }
        }

        private static void RunHardCodedDataMapping_TestSimpleAssignment(PrintJobLayout printJobLayout, JObject assignment)
        {
            printJobLayout.AddDataItem("ASSID", assignment["AssignmentId"].ToString());
            printJobLayout.AddDataItem("ASSTYPE", assignment["AssignmentType"].ToString());

            JArray partyArray = (JArray)assignment["Parties"];
            for (int i = 0; i < partyArray.Count; i++)
            {
                JToken partyObject = partyArray[i];

                printJobLayout.AddDataItem("PARTYTYPE", partyObject["PartyType"].ToString());
                printJobLayout.AddDataItem("PARTYNAME", partyObject["Name"].ToString());
                printJobLayout.AddDataItem("PARTYADDR1", partyObject["Address1"].ToString());
                printJobLayout.AddDataItem("PARTYADDR2", partyObject["Address2"].ToString());
                printJobLayout.AddDataItem("PARTYPOSTADDR", partyObject["PostalNumber"].ToString() + "  " + partyObject["PostalAddress"].ToString());
                printJobLayout.AddDataItem("PARTYCTRY", partyObject["CountryCode"].ToString());
                printJobLayout.AddDataItem("PARTYEMAIL", partyObject["Email"].ToString());
            }
        }

        private static void RunHardCodedDataMapping_LYB_AI(PrintJobLayout printJobLayout, JObject assignment)
        {
            JArray exrefArray = (JArray)assignment["ExternalReferences"];
            for (int i = 0; i < exrefArray.Count; i++)
            {
                JToken exRefObject = exrefArray[i];
                string extRefKey = exRefObject["Key"].ToString();

                if (extRefKey.Equals("KghReferenceSequenceNumber"))
                {
                    printJobLayout.AddDataItem("KGHICREF", exRefObject["Value"].ToString());
                    break;
                }
            }

            printJobLayout.AddDataItem("INTERNALREF", assignment["InternalReference"].ToString());
            printJobLayout.AddDataItem("DEKL2", assignment["AssignmentSubType2"].ToString());

            JArray partyArray = (JArray)assignment["Parties"];
            for (int i = 0; i < partyArray.Count; i++)
            {
                JToken partyObject = partyArray[i];
                if (partyObject["PartyType"].ToString() == "3")
                {
                    printJobLayout.AddDataItem("CNORNAME", partyObject["Name"].ToString());
                    printJobLayout.AddDataItem("CNORADR1", partyObject["Address1"].ToString());
                    printJobLayout.AddDataItem("CNORADR2", partyObject["Address2"].ToString());
                    printJobLayout.AddDataItem("CNORZIP", partyObject["PostalNumber"].ToString());
                    printJobLayout.AddDataItem("CNORPOADR", partyObject["PostalAddress"].ToString());
                    printJobLayout.AddDataItem("CNORCTRY", partyObject["CountryCode"].ToString());
                    printJobLayout.AddDataItem("CNOREMAIL", partyObject["Email"].ToString());
                    printJobLayout.AddDataItem("CNORORGNO", partyObject["EoriNumber"].ToString());
                }
                else if (partyObject["PartyType"].ToString() == "2")
                {
                    printJobLayout.AddDataItem("CNEENAME", partyObject["Name"].ToString());
                    printJobLayout.AddDataItem("CNEEADR1", partyObject["Address1"].ToString());
                    printJobLayout.AddDataItem("CNEEADR2", partyObject["Address2"].ToString());
                    printJobLayout.AddDataItem("CNEEZIP", partyObject["PostalNumber"].ToString());
                    printJobLayout.AddDataItem("CNEEPOADR", partyObject["PostalAddress"].ToString());
                    printJobLayout.AddDataItem("CNEECTRY", partyObject["CountryCode"].ToString());
                }
            }

            printJobLayout.AddDataItem("DTCODE", assignment["DeliveryTermCode"].ToString());
            printJobLayout.AddDataItem("DTPLACE", assignment["DeliveryTermPlace"].ToString());
            printJobLayout.AddDataItem("CYOFDESTCODE", assignment["CountryOfDestinationCode"].ToString());
            printJobLayout.AddDataItem("CYOFDISPCODE", assignment["CountryOfDispatchCode"].ToString());
            printJobLayout.AddDataItem("GOODSLOCATION", assignment["StorageLocationCode"].ToString());
            printJobLayout.AddDataItem("PLOFLOAD", assignment["PlaceOfLoadingCode"].ToString());
            printJobLayout.AddDataItem("TOTINV", assignment["TotalInvoiceAmount"].ToString());
            printJobLayout.AddDataItem("CURRCODE", assignment["CurrencyCode"].ToString());
            printJobLayout.AddDataItem("OCAMOUNT", assignment["OtherChargesAmount"].ToString());
            printJobLayout.AddDataItem("ODAMOUNT", assignment["DiscountAmount"].ToString());
            printJobLayout.AddDataItem("TRANSTYPE", assignment["TransactionType"].ToString());
            printJobLayout.AddDataItem("TRPMODEBRDR", assignment["TransportMeansAtBorderType"].ToString());
            printJobLayout.AddDataItem("TRPIDBRDR", assignment["TransportMeansAtBorderID"].ToString());
            printJobLayout.AddDataItem("TRPNATBRDR", assignment["TransportMeansAtBorderNationality"].ToString());
            printJobLayout.AddDataItem("TRPMODEINLAND", assignment["TransportMeansInlandType"].ToString());
            printJobLayout.AddDataItem("TRPID", assignment["TransportMeansInlandID"].ToString());
            printJobLayout.AddDataItem("CONTAINERIND", assignment["ContainerIndicator"].ToString());
            printJobLayout.AddDataItem("TRPCHARGEPAYMETH", assignment["TransportChargesPaymentMethod"].ToString());
            printJobLayout.AddDataItem("UCR", assignment["UniqueConsignmentReferenceNumber"].ToString());

            string rcc = "";
            JArray rccArray = (JArray)assignment["RoutingCountryCodes"];
            for (int i = 0; i < rccArray.Count; i++)
            {
                JToken rccObject = rccArray[i];
                rcc += rccObject["CountryCode"].ToString() + ", ";
            }
            if (rcc.Length > 2) printJobLayout.AddDataItem("ITINERARY", rcc.Substring(0, rcc.Length - 2));

            JArray coffArray = (JArray)assignment["CustomsOffices"];
            for (int i = 0; i < coffArray.Count; i++)
            {
                JToken coffObject = coffArray[i];
                if (coffObject["Type"].ToString() == "Ext")
                {
                    printJobLayout.AddDataItem("CUOFFEXIT", coffObject["Code"].ToString());
                    break;
                }
            }

            JArray itemArray = (JArray)assignment["Items"];
            for (int i = 0; i < itemArray.Count; i++)
            {
                JToken itemObject = itemArray[i];
                printJobLayout.AddDataItem("ITEMNO", itemObject["Sequence"].ToString());
                printJobLayout.AddDataItem("COMCODE", itemObject["CommodityCode"].ToString());
                printJobLayout.AddDataItem("PROCEDURE", itemObject["ProcedureCode"].ToString());
                printJobLayout.AddDataItem("GOODSDESCR", itemObject["DescriptionForCustomsPurpose"].ToString());
                printJobLayout.AddDataItem("NOOFPKG", itemObject["Packages"][0]["NumberOfPackages"].ToString());
                printJobLayout.AddDataItem("PKGCODE", itemObject["Packages"][0]["PackageCode"].ToString());
                printJobLayout.AddDataItem("GRWEIGHT", itemObject["GrossWeight"].ToString());
                printJobLayout.AddDataItem("NETWEIGHT", itemObject["NetWeight"].ToString());
                printJobLayout.AddDataItem("ITEMAMOUNT", itemObject["ItemAmount"].ToString());
                printJobLayout.AddDataItem("STATVALUE", itemObject["StatisticalValue"].ToString());
                printJobLayout.AddDataItem("UNDGCODE", itemObject["DangerousGoodsCode"].ToString());

                JArray docArray = (JArray)itemObject["AttachedInfoAndDocuments"];
                for (int idoc = 0; idoc < docArray.Count; idoc++)
                {
                    JToken docObject = docArray[idoc];
                    printJobLayout.AddDataItem("DOCCODE", docObject["Code"].ToString());
                    printJobLayout.AddDataItem("DOCDESCR", docObject["Description"].ToString());
                }

                JArray contArray = (JArray)itemObject["Containers"];
                for (int icont = 0; icont < contArray.Count; icont++)
                {
                    JToken contObject = contArray[icont];
                    printJobLayout.AddDataItem("CONTAINERNO", contObject["ContainerNumber"].ToString());
                }
            }
        }
    }
}