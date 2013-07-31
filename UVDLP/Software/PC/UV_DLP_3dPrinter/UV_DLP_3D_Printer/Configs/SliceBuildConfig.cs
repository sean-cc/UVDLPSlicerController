﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
namespace UV_DLP_3D_Printer
{
    /*
     * This class holds some information about the 
     * slicing and building parameters
     */
    public class SliceBuildConfig
    {
        public static int FILE_VERSION = 1;
        public string m_filename; // for housekeeping
        public enum eBuildDirection 
        {
            Top_Down,
            Bottom_Up
        }
        public double dpmmX; // dots per mm x
        public double dpmmY; // dots per mm y
        public int xres, yres; // the resolution of the output image in pixels
        public double ZThick; // thickness of the z layer - slicing height
        public int layertime_ms; // time to project image per layer in milliseconds
        public int firstlayertime_ms; // first layer exposure time 
        public int numfirstlayers;
        public int blanktime_ms; // blanking time between layers
       // public int raise_time_ms; // time delay for the z axis to raise on a per-layer basis
        public int plat_temp; // desired platform temperature in celsius 
        public bool exportgcode; // export the gcode file when slicing
        public bool exportsvg; // export the svg slices when building
        public bool exportimages; // export image slices when building
        public eBuildDirection direction;
        public double liftdistance; // distance to lift and retract
        public double slidetiltval; // a value used for slide / tilt 

        private String m_headercode; // inserted at beginning of file
        private String m_footercode; // inserted at end of file
        private String m_preliftcode; // inserted before each slice
        private String m_postliftcode; // inserted after each slice
        private String m_preslicecode; // inserted before each slice

        public int XOffset, YOffset; // the X/Y pixel offset used 

        private String[] m_defheader = 
        {
            "(********** Header Start ********)\r\n", // 
            "(Generated by UV - DLP Slicer)\r\n",
            "G21 (Set units to be mm)\r\n", 
            "G91 (Relative Positioning)\r\n",
            "M17 (Enable motors)\r\n",
            "(********** Header End **********)\r\n", // 
            //"()\r\n"
        };
        private String[] m_deffooter = 
        {
            "(********** Footer Start ********)\r\n", // 
            "\r\n",
            "(<Completed>)\r\n", // a marker for completed            
            "(********** Footer End ********)\r\n", // 
        };

        private String[] m_defprelift = 
        {
            "\r\n"
        };

        private String[] m_defpostlift = 
        {
            "\r\n"
        };

        private String[] m_defpreslice = 
        {
            "\r\n"
        };

            

        private void SetDefaultCodes()
        {
            StringBuilder sb = new StringBuilder();
            foreach (String s in m_defheader)
                sb.Append(s);
            HeaderCode = sb.ToString();

            sb = new StringBuilder(); // clear
            foreach (String s in m_deffooter)
                sb.Append(s);
            FooterCode = sb.ToString();

            sb = new StringBuilder();
            foreach (String s in m_defprelift)
                sb.Append(s);
            PreLiftCode = sb.ToString();

            sb = new StringBuilder();
            foreach (String s in m_defpostlift)
                sb.Append(s);
            PostLiftCode = sb.ToString();

            sb = new StringBuilder();
            foreach (String s in m_defpreslice)
                sb.Append(s);
            PreSliceCode = sb.ToString();          
        }

        public String HeaderCode
        {
            get { return m_headercode; }
            set { m_headercode = value; }
        }
        public String FooterCode
        {
            get { return m_footercode; }
            set { m_footercode = value; }
        }
        public String PreLiftCode
        {
            get { return m_preliftcode; }
            set { m_preliftcode = value; }
        }

        public String PostLiftCode
        {
            get { return m_postliftcode; }
            set { m_postliftcode = value; }
        }

        public String PreSliceCode
        {
            get { return m_preslicecode; }
            set { m_preslicecode = value; }
        }

        /*
         Copy constructor
         */
        public SliceBuildConfig(SliceBuildConfig source) 
        {
            dpmmX = source.dpmmX; // dots per mm x
            dpmmY = source.dpmmY; // dots per mm y
            xres = source.xres;
            yres = source.yres; // the resolution of the output image
            ZThick = source.ZThick; // thickness of the z layer - slicing height
            layertime_ms = source.layertime_ms; // time to project image per layer in milliseconds
            firstlayertime_ms = source.firstlayertime_ms;
            blanktime_ms = source.blanktime_ms;
            plat_temp = source.plat_temp; // desired platform temperature in celsius 
            exportgcode = source.exportgcode; // export the gcode file when slicing
            exportsvg = source.exportsvg; // export the svg slices when building
            exportimages = source.exportimages; // export image slices when building
            m_headercode = source.m_headercode; // inserted at beginning of file
            m_footercode = source.m_footercode; // inserted at end of file
            m_preliftcode = source.m_preliftcode; // inserted between each slice            
            m_postliftcode = source.m_postliftcode; // inserted between each slice    
            m_preslicecode = source.m_preslicecode; // inserted before each slice

            liftdistance = source.liftdistance;
            direction = source.direction;
            numfirstlayers = source.numfirstlayers;
            XOffset = source.XOffset;
            YOffset = source.YOffset;
            slidetiltval = source.slidetiltval;
            //raise_time_ms = source.raise_time_ms;
        }

        public SliceBuildConfig() 
        {
            layertime_ms = 5000;// 5 seconds default
            numfirstlayers = 3;
            CreateDefault();
        }
        public void UpdateFrom(MachineConfig mf)
        {
            dpmmX = mf.PixPerMMX; //10 dots per mm
            dpmmY = mf.PixPerMMY;// 10;
            xres = mf.XRes;
            yres = mf.YRes;            
        }
        public void CreateDefault() 
        {
            layertime_ms = 1000;// 1 second default
            firstlayertime_ms = 5000;
            blanktime_ms = 2000; // 2 seconds blank
            xres = 1024;
            yres = 768;
            ZThick = .05;
            plat_temp = 75;
            dpmmX = 102.4;
            dpmmY = 76.8;
            XOffset = 0;
            YOffset = 0;
            numfirstlayers = 3;
            exportgcode = true;
            exportsvg = false;
            exportimages = false;
            direction = eBuildDirection.Bottom_Up;
            liftdistance = 5.0;
            //raise_time_ms = 750;
            slidetiltval = 0.0;
            SetDefaultCodes(); // set up default gcodes
        }

        /*This is used to serialize to the GCode post-header info*/
        public bool Load(String filename) 
        {
            try
            {
                m_filename = filename;
                LoadGCodes();
                XmlReader xr = (XmlReader)XmlReader.Create(filename);
                xr.ReadStartElement("SliceBuildConfig");
                int ver = int.Parse(xr.ReadElementString("FileVersion"));
                if (ver != FILE_VERSION) 
                {
                    return false; // I may try to implement some backward compatibility here...
                }
                dpmmX = double.Parse(xr.ReadElementString("DotsPermmX"));
                dpmmY = double.Parse(xr.ReadElementString("DotsPermmY"));
                xres = int.Parse(xr.ReadElementString("XResolution"));
                yres = int.Parse(xr.ReadElementString("YResolution"));
                ZThick = double.Parse(xr.ReadElementString("SliceHeight"));
                layertime_ms = int.Parse(xr.ReadElementString("LayerTime"));
                firstlayertime_ms = int.Parse(xr.ReadElementString("FirstLayerTime"));
                blanktime_ms = int.Parse(xr.ReadElementString("BlankTime"));
                plat_temp = int.Parse(xr.ReadElementString("PlatformTemp"));
                exportgcode = bool.Parse(xr.ReadElementString("ExportGCode"));
                exportsvg = bool.Parse(xr.ReadElementString("ExportSVG"));
                exportimages = bool.Parse(xr.ReadElementString("ExportImages")); ;
                XOffset = int.Parse(xr.ReadElementString("XOffset"));
                YOffset = int.Parse(xr.ReadElementString("YOffset"));
                numfirstlayers = int.Parse(xr.ReadElementString("NumberofBottomLayers"));
                direction = (eBuildDirection)Enum.Parse(typeof(eBuildDirection), xr.ReadElementString("Direction"));
                liftdistance = double.Parse(xr.ReadElementString("LiftDistance"));
                slidetiltval = double.Parse(xr.ReadElementString("SlideTiltValue"));
                //raise_time_ms = int.Parse(xr.ReadElementString("Raise_Time_Delay"));
                xr.ReadEndElement();
                xr.Close();
                
                return true;
            }
            catch (Exception ex)
            {
                DebugLogger.Instance().LogRecord(ex.Message);
                return false;
            }       
        }
        public bool Save(String filename)
        {
            try 
            {
                m_filename = filename;
                XmlWriter xw =XmlWriter.Create(filename);
                xw.WriteStartElement("SliceBuildConfig");
                xw.WriteElementString("FileVersion",FILE_VERSION.ToString());
                xw.WriteElementString("DotsPermmX", dpmmX.ToString());
                xw.WriteElementString("DotsPermmY", dpmmY.ToString());
                xw.WriteElementString("XResolution", xres.ToString());
                xw.WriteElementString("YResolution", yres.ToString());
                xw.WriteElementString("SliceHeight", ZThick.ToString());
                xw.WriteElementString("LayerTime", layertime_ms.ToString());
                xw.WriteElementString("FirstLayerTime", firstlayertime_ms.ToString());
                xw.WriteElementString("BlankTime", blanktime_ms.ToString());                
                xw.WriteElementString("PlatformTemp", plat_temp.ToString());
                xw.WriteElementString("ExportGCode", exportgcode.ToString());
                xw.WriteElementString("ExportSVG", exportsvg.ToString());
                xw.WriteElementString("ExportImages", exportimages.ToString());
                xw.WriteElementString("XOffset", XOffset.ToString());
                xw.WriteElementString("YOffset", YOffset.ToString());
                xw.WriteElementString("NumberofBottomLayers", numfirstlayers.ToString());
                xw.WriteElementString("Direction", direction.ToString());
                xw.WriteElementString("LiftDistance", liftdistance.ToString());
                xw.WriteElementString("SlideTiltValue", slidetiltval.ToString());                

               // xw.WriteElementString("Raise_Time_Delay",raise_time_ms.ToString());
                xw.WriteEndElement();
                xw.Close();
                SaveGCodes();
                return true;
            }
            catch (Exception ex) 
            {
                DebugLogger.Instance().LogRecord(ex.Message);
                return false;
            }            
        }

        // these get stored to the gcode file as a reference
        public override String ToString() 
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(****Build and Slicing Parameters****)\r\n");
            sb.Append("(pix per mm X           = " + dpmmX + " px/mm )\r\n");
            sb.Append("(pix per mm Y           = " + dpmmY + " px/mm )\r\n");
            sb.Append("(X resolution            = " + xres + " px )\r\n");
            sb.Append("(Y resolution            = " + yres + " px )\r\n");
            sb.Append("(X Pixel Offset          = " + XOffset + " px )\r\n");
            sb.Append("(Y Pixel Offset          = " + YOffset + " px )\r\n");
            sb.Append("(Layer thickness         = " + ZThick + " mm )\r\n");
            sb.Append("(Layer Time              = " + layertime_ms + " ms )\r\n");
            sb.Append("(First Layer Time        = " + firstlayertime_ms + " ms )\r\n");
            sb.Append("(Number of Bottom Layers = " + numfirstlayers + " )\r\n");
            sb.Append("(Blanking Layer Time     = " + blanktime_ms + " ms )\r\n");
           // sb.Append("(Platform Temp           = " + plat_temp + " degrees celsius)\r\n");
            sb.Append("(Build Direction         = " + direction.ToString() + ")\r\n");
            sb.Append("(Lift Distance           = " + liftdistance.ToString() + " mm )\r\n");
            sb.Append("(Slide/Tilt Value        = " + slidetiltval.ToString() + " mm )\r\n");
            // sb.Append("(Raise Time Delay        = " + raise_time_ms.ToString() + " ms )\r\n");
            return sb.ToString();
        }

        public void LoadGCodes() 
        {
            try
            {

                //String profilepath = Path.GetDirectoryName(UVDLPApp.Instance().m_appconfig.m_cursliceprofilename);
                String profilepath = Path.GetDirectoryName(m_filename);                
                profilepath += UVDLPApp.m_pathsep;
                //profilepath += Path.GetFileNameWithoutExtension(UVDLPApp.Instance().m_appconfig.m_cursliceprofilename);
                profilepath += Path.GetFileNameWithoutExtension(m_filename);
                if (!Directory.Exists(profilepath))
                {
                    Directory.CreateDirectory(profilepath);
                    SetDefaultCodes();
                    SaveGCodes();// save the default gcode files for this machine
                }
                else
                {
                    //load the files
                    m_headercode = LoadFile(profilepath + UVDLPApp.m_pathsep + "start.gcode");
                    m_footercode = LoadFile(profilepath + UVDLPApp.m_pathsep + "end.gcode");
                    m_preliftcode = LoadFile(profilepath + UVDLPApp.m_pathsep + "prelift.gcode");
                    m_postliftcode = LoadFile(profilepath + UVDLPApp.m_pathsep + "postlift.gcode");
                    m_preslicecode = LoadFile(profilepath + UVDLPApp.m_pathsep + "preslice.gcode");
                }
            }
            catch (Exception ex) 
            {
                DebugLogger.Instance().LogRecord(ex.Message);
            }
        }

        public String LoadFile(String filename) 
        {
            try
            {
                return File.ReadAllText(filename);
            }
            catch (Exception ex) 
            {
                DebugLogger.Instance().LogRecord(ex.Message);
                return "";
            }
        }

        public bool SaveFile(String filename, String contents) 
        {
            try
            {
                File.WriteAllText(filename, contents);
                return true;
            }
            catch (Exception ex) 
            {
                DebugLogger.Instance().LogRecord(ex.Message);
                return false;
            }
        }
        public void SaveGCodes() 
        {
            try
            {
                String profilepath = Path.GetDirectoryName(m_filename);
                profilepath += UVDLPApp.m_pathsep;
                profilepath += Path.GetFileNameWithoutExtension(m_filename);
                //create the directory if it doesn't exist
                if (!Directory.Exists(profilepath))
                {
                    Directory.CreateDirectory(profilepath);
                }

                SaveFile(profilepath + UVDLPApp.m_pathsep + "start.gcode", m_headercode);
                SaveFile(profilepath + UVDLPApp.m_pathsep + "end.gcode", m_footercode);
                SaveFile(profilepath + UVDLPApp.m_pathsep + "prelift.gcode", m_preliftcode);
                SaveFile(profilepath + UVDLPApp.m_pathsep + "postlift.gcode", m_postliftcode);
                SaveFile(profilepath + UVDLPApp.m_pathsep + "preslice.gcode", m_preslicecode);
            }
            catch (Exception ex) 
            {
                DebugLogger.Instance().LogError(ex.Message);
            }
        }
    }
}
