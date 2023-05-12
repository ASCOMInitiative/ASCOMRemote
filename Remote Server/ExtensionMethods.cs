using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASCOM.Remote
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Lookup dictionary to translate lower case method names to mixed case.
        /// The device type is included in order to disambiguate the same method name if used in different device types AND cased differently/>
        /// </summary>
        private static readonly Dictionary<string, string> methodLookup = new Dictionary<string, string>()
        {
            { "connected","Connected" },

            // Telescope
            { "tracking","Tracking" },
            { "doesrefraction","DoesRefraction" },
            { "slewsettletime","SlewSettleTime" },
            { "declinationrate","DeclinationRate" },
            { "rightascensionrate","RightAscensionRate" },
            { "guideratedeclination","GuideRateDeclination" },
            { "guideraterightascension","GuideRateRightAscension" },
            { "sideofpier", "SideOfPier" },
            { "siteelevation","SiteElevation" },
            { "sitelatitude","SiteLatitude" },
            { "sitelongitude","SiteLongitude" },
            { "targetdeclination","TargetDeclination" },
            { "targetrightascension","TargetRightAscension" },
            { "utcdate","UTCDate" },
            { "trackingrate","TrackingRate" },
            { SharedConstants.AXIS_PARAMETER_NAME,SharedConstants.AXIS_PARAMETER_NAME },
            { SharedConstants.RA_PARAMETER_NAME,SharedConstants.RA_PARAMETER_NAME },
            { SharedConstants.DEC_PARAMETER_NAME,SharedConstants.DEC_PARAMETER_NAME },
            { SharedConstants.RATE_PARAMETER_NAME,SharedConstants.RATE_PARAMETER_NAME },
            { SharedConstants.DIRECTION_PARAMETER_NAME,SharedConstants.DIRECTION_PARAMETER_NAME },
            { SharedConstants.DURATION_PARAMETER_NAME,SharedConstants.DURATION_PARAMETER_NAME },
            { SharedConstants.ALT_PARAMETER_NAME,SharedConstants.ALT_PARAMETER_NAME },
            { SharedConstants.AZ_PARAMETER_NAME,SharedConstants.AZ_PARAMETER_NAME },

            // Rotator
            { SharedConstants.POSITION_PARAMETER_NAME,SharedConstants.POSITION_PARAMETER_NAME},
            { "reverse","Reverse" },

            // Camera
            { "binx","BinX" },
            { "biny","BinY"},
            { "cooleron","CoolerOn"},
            { "fastreadout","FastReadout"},
            { "gain","Gain"},
            { "numx","NumX"},
            { "numy","NumY"},
            { "offset","Offset"},
            { "readoutmode","ReadoutMode"},
            { "setccdtemperature","SetCCDTemperature"},
            { "startx","StartX"},
            { "starty","StartY"},
            { "subexposureduration","SubExposureDuration"},
            { "slaved","Slaved" },
            { "Brightness","Brightness" },
            { "position","Position" },
            { "tempcomp","TempComp" },
            { SharedConstants.SENSORNAME_PARAMETER_NAME,SharedConstants.SENSORNAME_PARAMETER_NAME },
            { "averageperiod","AveragePeriod" },
            { "sensordescription","SensorDescription" },
            { SharedConstants.ID_PARAMETER_NAME,SharedConstants.ID_PARAMETER_NAME },
            { SharedConstants.NAME_PARAMETER_NAME,SharedConstants.NAME_PARAMETER_NAME }, 
            { SharedConstants.VALUE_PARAMETER_NAME,SharedConstants.VALUE_PARAMETER_NAME },
            { SharedConstants.STATE_PARAMETER_NAME,SharedConstants.STATE_PARAMETER_NAME },
            { "id","Id" }
        };

        /// <summary>
        /// Look up the cased version of a method name.
        /// </summary>
        /// <param name="methodName">The method name to look up</param>
        /// <param name="deviceType">Optional device type to disambiguate method names that are used in more than 1 device type AND cased differently</param>
        /// <exception cref="KeyNotFoundException">When the required method name key has not yet been added to the lookup dictionary.</exception>
        /// <returns>The mixed case equivalent of the supplied method name</returns>
        public static string ToCorrectCase(this string methodName, string deviceType = null)
        {
            string lookupKey = "ValueNotSet";
            // Look up the cased version of the method. If this fails a KeyNotFoiund exception will be thrown
            try
            {
                lookupKey = $"{methodName}{(deviceType is null ? "" : $".{deviceType}")}";
                return methodLookup[lookupKey];
            }
            catch (Exception ex)
            {
                return $"UnknownMethod:'{lookupKey}' - {ex.ToString()}";
            }
        }

        /// <summary>
        /// Append a byte array to an existing array
        /// </summary>
        /// <param name="first">First array</param>
        /// <param name="second">Second array</param>
        /// <returns>Concatenated array of byte</returns>
        public static byte[] Append(this byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        public static string ToConcatenatedString(this List<string> list, string separator)
        {
            string concatenatedList = "";
            foreach (string item in list)
            {
                concatenatedList += item + separator;
            }
            return concatenatedList.Trim(separator.ToCharArray());
        }

        public static void FromConcatenatedString(this List<string> list, string concatenatedString, string separator)
        {
            string[] items = concatenatedString.Split(separator.ToCharArray());

            list.Clear();

            foreach (string item in items)
            {
                list.Add(item);
            }
        }

        public static List<StringValue> ToListStringValue(this List<string> fromList)
        {
            List<StringValue> toList = new List<StringValue>();

            foreach (string item in fromList)
            {
                toList.Add(new StringValue(item));
            }

            return toList;
        }

        public static List<string> ToListString(this List<StringValue> fromList)
        {
            List<string> toList = new List<string>();

            foreach (StringValue item in fromList)
            {
                toList.Add(item.Value);
            }

            return toList;
        }
    }
}
