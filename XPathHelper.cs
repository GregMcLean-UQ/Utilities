using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Utilities
   {
   public class XPathHelper
      {
      //-----------------------------------------------------------------------------------
      public XPathHelper()
         {
         }

      //-----------------------------------------------------------------------------------
      public static XmlNodeList findAllNodes(XmlDocument doc, string label)
          {
          return doc.SelectNodes("//*[name() = '" + label + "']");
          }
      //-----------------------------------------------------------------------------------
      public static XmlNodeList findChildNodes(XmlNode node, string label)
          {
          return node.SelectNodes(findXPath(node) + "//*[name() = '" + label + "']");
          }     

       //-----------------------------------------------------------------------------------
      public static XmlNode findChildNode(XmlNode node, string label)
          {
          return node.SelectSingleNode(findXPath(node) + "//*[name() = '" + label + "']");
          }     
      //-----------------------------------------------------------------------------------
      public static string findXPath(XmlNode node)
          {
         StringBuilder builder = new StringBuilder();
         while (node != null)
            {
            switch (node.NodeType)
               {
               case XmlNodeType.Attribute:
                  builder.Insert(0, "/@" + node.Name);
                  node = ((XmlAttribute)node).OwnerElement;
                  break;
               case XmlNodeType.Element:
                  int index = findElementIndex((XmlElement)node);
                  builder.Insert(0, "/" + node.Name + "[" + index + "]");
                  node = node.ParentNode;
                  break;
               case XmlNodeType.Document:
                  return builder.ToString();
               default:
                  throw new ArgumentException("Only elements and attributes are supported");
               }
            }
         throw new ArgumentException("Node was not in a document");
         }
      //-----------------------------------------------------------------------------------
      static int findElementIndex(XmlElement element)
         {
         XmlNode parentNode = element.ParentNode;
         if (parentNode is XmlDocument)
            {
            return 1;
            }
         XmlElement parent = (XmlElement)parentNode;
         int index = 1;
         foreach (XmlElement candidate in parent.GetElementsByTagName(element.Name))
            {
            if (candidate == element)
               {
               return index;
               }
            index++;
            }
         throw new ArgumentException("Couldn't find element within parent");
         }
      }
   }
