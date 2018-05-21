using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
namespace ASCOM.Remote
{
    public class TelescopeStaticData : RestResponseBase
    {
        public int AlignmentMode { get; set; }
        public int ApertureArea { get; set; }
        public bool CanFindHome { get; set; }
        public bool CanPark { get; set; }
        public bool CanPulseGuide { get; set; }
        public bool CanSetDeclinationRate { get; set; }
        public bool CanSetGuideRates { get; set; }
        public bool CanSetPark { get; set; }
        public bool CanSetPerSide { get; set; }
        public bool CanSetRightAscensionRate { get; set; }
        public bool CanSetTracking { get; set; }
        public bool CanSlew { get; set; }
        public bool CanSlewAltAz { get; set; }
        public bool CanSlewAltAzAsync { get; set; }
        public bool CanSlewAsync { get; set; }
        public bool CanSync { get; set; }
        public bool CanSyncAlrAz { get; set; }
        public bool CanUnpark { get; set; }
        public string Description { get; set; }
        public bool DoesRefraction { get; set; }
        public string DriverInfo { get; set; }
        public string DriverVersion { get; set; }
        public int EquatorialSystem { get; set; }
        public double FocalLength { get; set; }
        public short InterfaceVersion { get; set; }
        public string Name { get; set; }
        public double SiteElevation { get; set; }
        public double SiteLatitude { get; set; }
        public double SiteLongitude { get; set; }
        public string[] SupportedActions { get; set; }
        public int[] TrackingRates { get; set; }
        public string Contents()
        {
            try
            {
                string r = "";
                r += "\r\nTransactionID: " + ServerTransactionID.ToString();
                r += "\r\nTransactionError: " + DriverException.ToString();
                r += "\r\nAlignmentMode: " + AlignmentMode.ToString();
                r += "\r\nApertureArea: " + ApertureArea;
                r += "\r\nCanFindHome: " + CanFindHome;
                r += "\r\nCanPark: " + CanPark;
                r += "\r\nCanPulseGuide: " + CanPulseGuide;
                r += "\r\nCanSetDeclinationRate: " + CanSetDeclinationRate;
                r += "\r\nDescription: " + Description;
                r += "\r\nDriverInfo: " + DriverInfo;
                r += "\r\nSiteElevation: " + SiteElevation;
                r += "\r\nSiteLatitude: " + SiteLatitude;
                r += "\r\nSiteLongitude: " + SiteLongitude;
                foreach (int i in TrackingRates)
                {
                    r += "\r\nTrackingRates: " + i.ToString();
                }

                return r;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}
