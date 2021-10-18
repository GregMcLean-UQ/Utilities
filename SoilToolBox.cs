using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Utilities
    {
    public class SoilToolBox
        {
        
        XmlDocument toolBox;        //Apsim Soil Toolbox
        XmlDocument apsimSoils;     //Simgen Soil XML
        List<string> _soilNames;

        bool _useIFESW;


        public bool useIFESW
            {
            get
                {
                return _useIFESW;
                }
            }
        //--------------------------------------------------------------------------
        public SoilToolBox(string fileName)
            {
            apsimSoils = new XmlDocument();
            apsimSoils.Load(fileName);

            if (apsimSoils.InnerText.Contains("IFESW$"))
                {
                _useIFESW = true;
                }

            //Get the soil names
            _soilNames = new List<string>();

            XmlNodeList SoilNodes = apsimSoils.SelectNodes("//*[contains(@name, 'Water')]");

            for (int i = 0; i < SoilNodes.Count; i++)
                {
                _soilNames.Add(SoilNodes[i].Attributes["name"].ToString().Replace(" Water", ""));
                }
            }

        //--------------------------------------------------------------------------
        public SoilToolBox(string fileName, bool useIFESW)
            {
            try
                {
                toolBox = new XmlDocument();
                toolBox.Load(fileName);
                }
            catch (Exception e)
                {
                throw (new Exception("Encountered the following problem: " + e.Message));
                }

            _useIFESW = useIFESW;

            XmlNodeList toolBoxSoilNodes = toolBox.SelectNodes("//*[translate(name(),'abcdefghijklmnopqrstuvwxyz',"
                                    + " 'ABCDEFGHIJKLMNOPQRSTUVWXYZ') = 'SOIL']");

            _soilNames = new List<string>();

            if (toolBoxSoilNodes.Count > 0)
                {
                foreach (XmlNode node in toolBoxSoilNodes)
                    {
                    _soilNames.Add(node.Attributes["name"].Value);
                    }
                }

            apsimSoils = new XmlDocument();
            XmlElement docElement = apsimSoils.CreateElement("Soils");
            apsimSoils.AppendChild(docElement);

            foreach (XmlNode toolBoxSoilNode in toolBoxSoilNodes)
                {
                createApsimWaterComponent(toolBoxSoilNode, apsimSoils);
                createApsimNComponent(toolBoxSoilNode, apsimSoils);
                createApsimCropComponents(toolBoxSoilNode, apsimSoils);
                }
            }
        //--------------------------------------------------------------------------
        void createApsimWaterComponent(XmlNode toolBoxSoilNode, XmlDocument apsimSoils)
            {
            XmlElement water = apsimSoils.CreateElement("component");
            water.SetAttribute("name", toolBoxSoilNode.Attributes["name"].Value + " Water");
            water.SetAttribute("executable", @"WaterDLL$");

            apsimSoils.DocumentElement.AppendChild(water);

            //XmlElement init = apsimSoils.CreateElement("initdata");

            //Add the elements for the water data
            water.AppendChild(createElement("include", @"WaterINI$", apsimSoils));
            water.AppendChild(createElement("DiffusConst", "diffus_const", toolBoxSoilNode, apsimSoils));
            water.AppendChild(createElement("DiffusSlope", "diffus_slope", toolBoxSoilNode, apsimSoils));
            water.AppendChild(createElement("CN2Bare", "cn2_bare", toolBoxSoilNode, apsimSoils));
            water.AppendChild(createElement("CNRed", "cn_red", toolBoxSoilNode, apsimSoils));
            water.AppendChild(createElement("CNCov", "cn_cov", toolBoxSoilNode, apsimSoils));
            water.AppendChild(createElement("Salb", "salb", toolBoxSoilNode, apsimSoils));

            string nodeXPath = XPathHelper.findXPath(toolBoxSoilNode);

            XmlNodeList nodes = toolBoxSoilNode.SelectNodes(nodeXPath + "//*[translate(name(),'abcdefghijklmnopqrstuvwxyz',"
                                    + " 'ABCDEFGHIJKLMNOPQRSTUVWXYZ') = 'CONA']");

            if (nodes.Count > 0)
                {
                water.AppendChild(createElement("Cona", "Cona", toolBoxSoilNode, apsimSoils));
                water.AppendChild(createElement("U", "U", toolBoxSoilNode, apsimSoils));
                }
            else
                {
                water.AppendChild(createElement("SummerCona", "SummerCona", toolBoxSoilNode, apsimSoils));
                water.AppendChild(createElement("WinterCona", "WinterCona", toolBoxSoilNode, apsimSoils));
                water.AppendChild(createElement("SummerU", "SummerU", toolBoxSoilNode, apsimSoils));
                water.AppendChild(createElement("WinterU", "WinterU", toolBoxSoilNode, apsimSoils));
                water.AppendChild(createElement("SummerDate", "SummerDate", toolBoxSoilNode, apsimSoils));
                water.AppendChild(createElement("WinterDate", "WinterDate", toolBoxSoilNode, apsimSoils));
                }

            XmlNode profileNode = toolBoxSoilNode.SelectSingleNode(XPathHelper.findXPath(toolBoxSoilNode) + "//*[translate(name(),'abcdefghijklmnopqrstuvwxyz',"
                                    + " 'ABCDEFGHIJKLMNOPQRSTUVWXYZ') = 'PROFILE']");

            if (profileNode == null)
                {
                profileNode = toolBoxSoilNode.SelectSingleNode(XPathHelper.findXPath(toolBoxSoilNode) + "//*[translate(name(),'abcdefghijklmnopqrstuvwxyz',"
                                    + " 'ABCDEFGHIJKLMNOPQRSTUVWXYZ') = 'WATER']");
                }

            XmlNodeList lls = toolBoxSoilNode.SelectNodes(nodeXPath + "//*[translate(name(),'abcdefghijklmnopqrstuvwxyz',"
                                    + " 'ABCDEFGHIJKLMNOPQRSTUVWXYZ') = 'LL']");

            if (lls[0].ParentNode.Name.ToString() == "SoilCrop")
                {
                water.AppendChild(getChildText("thickness", "dlayer", profileNode, apsimSoils, false));
                water.AppendChild(getChildText("sat", "sat", toolBoxSoilNode, apsimSoils));
                water.AppendChild(getChildText("dul", "dul", toolBoxSoilNode, apsimSoils));
                water.AppendChild(getChildText("ll15", "ll15", toolBoxSoilNode, apsimSoils));
                water.AppendChild(getChildText("airdry", "air_dry", toolBoxSoilNode, apsimSoils));
                water.AppendChild(getChildText("swcon", "swcon", toolBoxSoilNode, apsimSoils));
                water.AppendChild(getChildText("bd", "bd", toolBoxSoilNode, apsimSoils));
                }
            else
                {
                water.AppendChild(createElement("thickness", "dlayer", profileNode, apsimSoils, false));
                water.AppendChild(createElement("sat", "sat", toolBoxSoilNode, apsimSoils));
                water.AppendChild(createElement("dul", "dul", toolBoxSoilNode, apsimSoils));
                water.AppendChild(createElement("ll15", "ll15", toolBoxSoilNode, apsimSoils));
                water.AppendChild(createElement("airdry", "air_dry", toolBoxSoilNode, apsimSoils));
                water.AppendChild(createElement("swcon", "swcon", toolBoxSoilNode, apsimSoils));
                water.AppendChild(createElement("bd", "bd", toolBoxSoilNode, apsimSoils));
                }

            if (!_useIFESW)
                {
                XmlElement testSW = null;
                if (lls[0].ParentNode.Name.ToString() == "SoilCrop")
                    {
                    try
                        {
                        getChildText("sw", "sw", toolBoxSoilNode, apsimSoils);
                        }
                    catch (Exception)
                        {
                        }
                    if (testSW == null)
                        {
                        water.AppendChild(getChildText("dul", "sw", toolBoxSoilNode, apsimSoils));
                        }
                    else
                        {
                        water.AppendChild(testSW);
                        }
                    }
                else
                    {
                    try
                        {
                        createElement("sw", "sw", toolBoxSoilNode, apsimSoils);
                        }
                    catch (Exception)
                        {
                        }
                    if (testSW == null)
                        {
                        water.AppendChild(createElement("dul", "sw", toolBoxSoilNode, apsimSoils));
                        }
                    else
                        {
                        water.AppendChild(testSW);
                        }
                    }
                }
            else
                {
                water.AppendChild(createElement("profile_fesw", "IFESW$", apsimSoils));
                }
            //water.AppendChild(init);
            }
        //--------------------------------------------------------------------------
        void createApsimNComponent(XmlNode toolBoxSoilNode, XmlDocument apsimSoils)
            {
            XmlElement N = apsimSoils.CreateElement("component");
            N.SetAttribute("name", toolBoxSoilNode.Attributes["name"].Value + " Nitrogen");
            N.SetAttribute("executable", @"NDLL$");

            apsimSoils.DocumentElement.AppendChild(N);

            //XmlElement init = apsimSoils.CreateElement("initdata");

            //Add the elements for the water data
            N.AppendChild(createElement("include", @"NINI$", apsimSoils));
            //init.AppendChild(createElement("SoilType", "soiltype", toolBoxSoilNode, apsimSoils));
            N.AppendChild(createElement("soiltype", "", apsimSoils));
            N.AppendChild(createElement("RootCN", "root_cn", toolBoxSoilNode, apsimSoils));
            N.AppendChild(createElement("RootWT", "root_wt", toolBoxSoilNode, apsimSoils));
            N.AppendChild(createElement("SoilCN", "soil_cn", toolBoxSoilNode, apsimSoils));
            N.AppendChild(createElement("EnrACoeff", "enr_a_coeff", toolBoxSoilNode, apsimSoils));
            N.AppendChild(createElement("EnrBCoeff", "enr_b_coeff", toolBoxSoilNode, apsimSoils));
            N.AppendChild(createElement("profile_reduction", "off", apsimSoils));

            XmlNodeList lls = toolBoxSoilNode.SelectNodes("//*[translate(name(),'abcdefghijklmnopqrstuvwxyz',"
                                    + " 'ABCDEFGHIJKLMNOPQRSTUVWXYZ') = 'LL']");

             if (lls[0].ParentNode.Name.ToString() == "SoilCrop")
                 {
                 N.AppendChild(getChildText("oc", "oc", toolBoxSoilNode, apsimSoils));
                 N.AppendChild(getChildText("ph", "ph", toolBoxSoilNode, apsimSoils));
                 N.AppendChild(getChildText("fbiom", "fbiom", toolBoxSoilNode, apsimSoils));
                 N.AppendChild(getChildText("finert", "finert", toolBoxSoilNode, apsimSoils));
                 N.AppendChild(getChildText("nh4", "nh4ppm", toolBoxSoilNode, apsimSoils));
                 N.AppendChild(getChildText("no3", "no3ppm", toolBoxSoilNode, apsimSoils));
                 try
                     {
                     N.AppendChild(createElement("ec", "ec", toolBoxSoilNode, apsimSoils));
                     }
                 catch (Exception)
                     {
                     }
                 }
             else
                 {
                 N.AppendChild(createElement("oc", "oc", toolBoxSoilNode, apsimSoils));
                 N.AppendChild(createElement("ph", "ph", toolBoxSoilNode, apsimSoils));
                 N.AppendChild(createElement("fbiom", "fbiom", toolBoxSoilNode, apsimSoils));
                 N.AppendChild(createElement("finert", "finert", toolBoxSoilNode, apsimSoils));
                 N.AppendChild(createElement("nh4", "nh4ppm", toolBoxSoilNode, apsimSoils));
                 N.AppendChild(createElement("no3", "no3ppm", toolBoxSoilNode, apsimSoils));
                 try
                     {
                     N.AppendChild(createElement("ec", "ec", toolBoxSoilNode, apsimSoils));
                     }
                 catch (Exception)
                     {
                     }
                 }

           // N.AppendChild(init);
            }
        //--------------------------------------------------------------------------
        void createApsimCropComponents(XmlNode toolBoxSoilNode, XmlDocument apsimSoils)
            {
            string nodeXPath = XPathHelper.findXPath(toolBoxSoilNode);

            XmlNodeList lls = toolBoxSoilNode.SelectNodes(nodeXPath + "//*[translate(name(),'abcdefghijklmnopqrstuvwxyz',"
                                    + " 'ABCDEFGHIJKLMNOPQRSTUVWXYZ') = 'LL']");
            XmlNodeList kls = toolBoxSoilNode.SelectNodes(nodeXPath + "//*[translate(name(),'abcdefghijklmnopqrstuvwxyz',"
                                    + " 'ABCDEFGHIJKLMNOPQRSTUVWXYZ') = 'KL']");
            XmlNodeList xfs = toolBoxSoilNode.SelectNodes(nodeXPath + "//*[translate(name(),'abcdefghijklmnopqrstuvwxyz',"
                                    + " 'ABCDEFGHIJKLMNOPQRSTUVWXYZ') = 'XF']");

            List<string> crops = new List<string>();

            if (lls[0].ParentNode.Name.ToString() == "SoilCrop")
            //Apsim 7.5
                {
                for (int i = 0; i < lls.Count; i++)
                    {
                    string crop = lls[i].ParentNode.Attributes["name"].Value;

                    if (crops.IndexOf(crop) == -1)
                        {
                        crops.Add(crop);
                        }
                    }
                }

            else if (lls[0].ParentNode.Name.ToString() == "Layer") 
                //Apsim 7.2
                {
                for (int i = 0; i < lls.Count; i++)
                    {
                    string crop = lls[i].ParentNode.ParentNode.Attributes["name"].Value;

                    if (crops.IndexOf(crop) == -1)
                        {
                        crops.Add(crop);
                        }
                    }
                }
            else  
                //Apsim 7.1
                {
                //See how many different crops ther are
                for (int i = 0; i < lls.Count; i++)
                    {
                    string crop = lls[i].Attributes["name"].Value;

                    if (crops.IndexOf(crop) == -1)
                        {
                        crops.Add(crop);
                        }
                    }
                }

            if (lls[0].ParentNode.Name.ToString() == "SoilCrop")
            //Apsim 7.5
                {
                for (int i = 0; i < crops.Count; i++)
                    {
                    XmlElement soilCrop = apsimSoils.CreateElement("component");
                    soilCrop.SetAttribute("name", toolBoxSoilNode.Attributes["name"].Value + " " + crops[i].ToLower());

                    apsimSoils.DocumentElement.AppendChild(soilCrop);

                    XmlElement ll = apsimSoils.CreateElement("ll");
                    XmlElement kl = apsimSoils.CreateElement("kl");
                    XmlElement xf = apsimSoils.CreateElement("xf");

                    string llvals = "";
                    string klvals= "";
                    string xfvals= "";


                    for (int j = 0; j < lls.Count; j++)
                        {
                        if (lls[j].ParentNode.Attributes["name"].Value == crops[i])
                            {
                            llvals = getChildText(lls[j]);
                            klvals = getChildText(kls[j]);
                            xfvals = getChildText(xfs[j]);
                            }
                        }

                    ll.AppendChild(apsimSoils.CreateTextNode(llvals));
                    kl.AppendChild(apsimSoils.CreateTextNode(klvals));
                    xf.AppendChild(apsimSoils.CreateTextNode(xfvals));

                    soilCrop.AppendChild(ll);
                    soilCrop.AppendChild(kl);
                    soilCrop.AppendChild(xf);
                    }
                }
            else if (lls[0].ParentNode.Name.ToString() == "Layer")
            //Apsim 7.2
                {
                for (int i = 0; i < crops.Count; i++)
                    {
                    XmlElement soilCrop = apsimSoils.CreateElement("component");
                    soilCrop.SetAttribute("name", toolBoxSoilNode.Attributes["name"].Value + " " + crops[i].ToLower());

                    apsimSoils.DocumentElement.AppendChild(soilCrop);

                    XmlElement ll = apsimSoils.CreateElement("ll");
                    XmlElement kl = apsimSoils.CreateElement("kl");
                    XmlElement xf = apsimSoils.CreateElement("xf");

                    List<string> llvals = new List<string>();
                    List<string> klvals = new List<string>();
                    List<string> xfvals = new List<string>();


                    for (int j = 0; j < lls.Count; j++)
                        {
                        if (lls[j].ParentNode.ParentNode.Attributes["name"].Value == crops[i])
                            {
                            llvals.Add(lls[j].InnerText);
                            klvals.Add(kls[j].InnerText);
                            xfvals.Add(xfs[j].InnerText);
                            }
                        }

                    ll.AppendChild(apsimSoils.CreateTextNode(string.Join(" ", llvals.ToArray())));
                    kl.AppendChild(apsimSoils.CreateTextNode(string.Join(" ", klvals.ToArray())));
                    xf.AppendChild(apsimSoils.CreateTextNode(string.Join(" ", xfvals.ToArray())));

                    soilCrop.AppendChild(ll);
                    soilCrop.AppendChild(kl);
                    soilCrop.AppendChild(xf);
                    }
                }
            else
            //Apsim 7.1
                {
                for (int i = 0; i < crops.Count; i++)
                    {
                    XmlElement soilCrop = apsimSoils.CreateElement("component");
                    soilCrop.SetAttribute("name", toolBoxSoilNode.Attributes["name"].Value + " " + crops[i].ToLower());

                    apsimSoils.DocumentElement.AppendChild(soilCrop);

                    XmlElement ll = apsimSoils.CreateElement("ll");
                    XmlElement kl = apsimSoils.CreateElement("kl");
                    XmlElement xf = apsimSoils.CreateElement("xf");

                    List<string> llvals = new List<string>();
                    List<string> klvals = new List<string>();
                    List<string> xfvals = new List<string>();

                    for (int j = 0; j < lls.Count; j++)
                        {
                        if (lls[j].Attributes["name"].Value == crops[i])
                            {
                            llvals.Add(lls[j].InnerText);
                            klvals.Add(kls[j].InnerText);
                            xfvals.Add(xfs[j].InnerText);
                            }
                        }

                    ll.AppendChild(apsimSoils.CreateTextNode(string.Join(" ", llvals.ToArray())));
                    kl.AppendChild(apsimSoils.CreateTextNode(string.Join(" ", klvals.ToArray())));
                    xf.AppendChild(apsimSoils.CreateTextNode(string.Join(" ", xfvals.ToArray())));

                    soilCrop.AppendChild(ll);
                    soilCrop.AppendChild(kl);
                    soilCrop.AppendChild(xf);
                    }
                }
            }

        string getChildText(XmlNode node)
            {
            List<string> vals = new List<string>();

            for (int i = 0; i < node.ChildNodes.Count; i++)
                {
                vals.Add(node.ChildNodes[i].InnerText);
                }

            return String.Join(" ", vals.ToArray());
            }
        //--------------------------------------------------------------------------
        XmlElement getChildText(string inName, string outName, XmlNode tbSoilNode, XmlDocument doc, bool lookOutside)
            {
            string nodeXPath = XPathHelper.findXPath(tbSoilNode);

            XmlNodeList varNodes = tbSoilNode.SelectNodes(nodeXPath + "//*[translate(name(),'abcdefghijklmnopqrstuvwxyz',"
                                    + " 'ABCDEFGHIJKLMNOPQRSTUVWXYZ') = '" + inName.ToUpper() + "']");

            string text = getChildText(varNodes[0]);

            XmlElement element = doc.CreateElement(outName);

            element.AppendChild(doc.CreateTextNode(text));

            return element;
            }
        //--------------------------------------------------------------------------
        XmlElement getChildText(string inName, string outName, XmlNode tbSoilNode, XmlDocument doc)
            {
            return getChildText(inName, outName, tbSoilNode, doc, true);
            }
        //--------------------------------------------------------------------------
        XmlElement createElement(string name, string text, XmlDocument doc)
            {
            XmlElement element = doc.CreateElement(name);
            element.AppendChild(doc.CreateTextNode(text));

            return element;
            }
        //--------------------------------------------------------------------------
        XmlElement createElement(string inName, string outName, XmlNode tbSoilNode, XmlDocument doc, bool lookOutside)
            {
            string nodeXPath = XPathHelper.findXPath(tbSoilNode);

            XmlNodeList varNodes = tbSoilNode.SelectNodes(nodeXPath + "//*[translate(name(),'abcdefghijklmnopqrstuvwxyz',"
                                    + " 'ABCDEFGHIJKLMNOPQRSTUVWXYZ') = '" + inName.ToUpper() + "']");

            List<string> textList = new List<string>();

            foreach (XmlNode node in varNodes)
                {
                if ((lookOutside == false && node.ParentNode.ParentNode == tbSoilNode) || lookOutside == true)
                    {
                    textList.Add(node.InnerText);
                    }
                }

            string text = string.Join(" ", textList.ToArray());

            XmlElement element = doc.CreateElement(outName);

            element.AppendChild(doc.CreateTextNode(text));

            return element;
            }
        //--------------------------------------------------------------------------
        XmlElement createElement(string inName, string outName, XmlNode tbSoilNode, XmlDocument doc)
            {
            return createElement(inName, outName, tbSoilNode, doc, true);
            }
        //--------------------------------------------------------------------------
        public void write(string fileName)
            {
            apsimSoils.Save(fileName);
            }
        //--------------------------------------------------------------------------
        public List<string> soilNames
            {
            get
                {
                return _soilNames;
                }
            }

        }
    }
