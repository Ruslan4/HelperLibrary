﻿/* 
 * FileName:    XmlConfigurationFile.cs
 * Author:      functionghw<functionghw@hotmail.com>
 * CreateTime:  3/18/2015 10:07:48 AM
 * Version:     v1.1
 * Description:
 * */

namespace HelperLibrary.Core.Configurations
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    /// <summary>
    /// Xml xonfiguration file
    /// </summary>
    public class XmlConfigurationFile : IConfigurationFile
    {
        #region Static members

        /* define tags and attributes using in XML file.
         */
        public static readonly string RootElementName = "configurations";
        public static readonly string ItemElementName = "setting";
        public static readonly string NameAttributeName = "name";
        public static readonly string ValueAttributeName = "value";

        /// <summary>
        /// a helper method to create an XML configuration file.
        /// This file only contains a empty root node.
        /// </summary>
        /// <param name="fullPath">full path to save the file</param>
        /// <returns></returns>
        private static XDocument CreateXmlFile(string fullPath)
        {
            XDocument doc = new XDocument(new XElement(RootElementName));
            doc.Save(fullPath);
            return doc;
        }

        #endregion

        #region Fields

        // dict to store configurations
        private IDictionary<string, string> configurationsDict;

        // sync object of the xml file
        private readonly object xmlFileSyncObj = new object();

        // the xml document object
        private XDocument xmlFile;

        // indecate if is loading configurations from file.
        private bool isLoading = false;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize the instance using a file path.
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <param name="isCreateNew">indicate whether to create a new file.
        /// True for creating new file, and false for loading exists file.</param>
        public XmlConfigurationFile(string filePath, bool isCreateNew = false)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException("filePath");

            string fullPath = Path.GetFullPath(filePath);

            bool fileExists = File.Exists(fullPath);
            if (fileExists && isCreateNew)
                throw new IOException("file already exists. " + fullPath);

            this.FullPath = fullPath;

            if (isCreateNew)
            {
                this.xmlFile = CreateXmlFile(fullPath);
            }
            else
            {
                this.xmlFile = XDocument.Load(fullPath);
            }
            LoadAllConfigurations(this.xmlFile);
        }

        /// <summary>
        /// try to load all configurations from file.
        /// </summary>
        private void LoadAllConfigurations(XDocument doc)
        {
            Contract.Ensures(doc != null);

            if (isLoading)
                return;

            lock (xmlFileSyncObj)
            {
                isLoading = true;
                try
                {
                    var root = doc.Root;
                    if (root == null
                        || root.Name != RootElementName)
                    {
                        throw new InvalidDataException("the format of configuration file is not right");
                    }

                    IDictionary<string, string> newDict = new ConcurrentDictionary<string, string>();
                    foreach (var item in root.Elements(ItemElementName))
                    {
                        var nameAttr = item.Attribute(NameAttributeName);
                        var valueAttr = item.Attribute(ValueAttributeName);
                        if (valueAttr == null)
                        {
                            continue;
                        }
                        if (nameAttr == null || string.IsNullOrEmpty(nameAttr.Value))
                        {
                            continue;
                        }
                        newDict.Add(nameAttr.Value, valueAttr.Value);
                    }
                    this.IsChanged = false;
                    this.xmlFile = doc;
                    this.configurationsDict = newDict;
                }
                finally
                {
                    isLoading = false;
                }
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets full path of the current xml file
        /// </summary>
        public string FullPath { get; private set; }

        /// <summary>
        /// indicate whether the configurations has been changed.
        /// </summary>
        public bool IsChanged { get; private set; }

        #endregion

        #region IConfigurationFile Members

        /// <summary>
        /// Get, add or update value of configuration by name.
        /// When add or update value, if the configuration with specific name not exist, 
        /// do adding, otherwise do updating.
        /// </summary>
        /// <param name="name">name of configuration</param>
        /// <returns>value of configuration if success, otherwise null</returns>
        public string this[string name]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentNullException("name");

                return InternalGetConfiguration(name);
            }
            set
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentNullException("name");

                if (value == null)
                    throw new ArgumentNullException("value");

                InternalAddOrUpdateConfiguration(name, value, ConfigOpt.AddOrUpdate);
            }
        }

        /// <summary>
        /// Check if exists configuraion with specific name.
        /// </summary>
        /// <param name="name">name of configuration.</param>
        /// <returns>true if the configuration exists, otherwise return false.</returns>
        public bool ContainsConfiguration(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            Contract.Ensures(configurationsDict != null);

            return this.configurationsDict.ContainsKey(name);
        }

        /// <summary>
        /// Get value of configuration by name.
        /// </summary>
        /// <param name="name">name of configuration</param>
        /// <returns>value of configuration if success, otherwise null</returns>
        public string GetConfiguration(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            return InternalGetConfiguration(name);
        }

        /// <summary>
        /// Add a new configuration of specific name and value.
        /// </summary>
        /// <param name="name">name of configuration</param>
        /// <param name="value">value of configuration</param>
        public void AddConfiguration(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            if (value == null)
                throw new ArgumentNullException("value");

            InternalAddOrUpdateConfiguration(name, value, ConfigOpt.Add);
        }

        /// <summary>
        /// Update value of specific configuration by name.
        /// </summary>
        /// <param name="name">name of configuration</param>
        /// <param name="newValue">new value of configuration</param>
        public void UpdateConfiguration(string name, string newValue)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            if (newValue == null)
                throw new ArgumentNullException("value");

            InternalAddOrUpdateConfiguration(name, newValue, ConfigOpt.Update);
        }

        /// <summary>
        /// Remove a configuration by name.
        /// </summary>
        /// <param name="name">name of configuration</param>
        public void RemoveConfiguration(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            InternalRemoveConfiguration(name);
        }

        /// <summary>
        /// Get all configurations as a dictionary
        /// </summary>
        /// <returns>a dictionary contains all configurations if success, 
        /// otherwise return null.</returns>
        public IDictionary<string, string> ToDictionary()
        {
            Contract.Ensures(this.configurationsDict != null);

            return new Dictionary<string, string>(this.configurationsDict);
        }

        /// <summary>
        /// Save change to file
        /// </summary>
        public void SaveChange()
        {
            Contract.Ensures(this.xmlFile != null);

            if (IsChanged)
            {
                lock (xmlFileSyncObj)
                {
                    if (IsChanged)
                    {
                        this.xmlFile.Save(FullPath);
                        IsChanged = false;
                    }
                }
            }
        }

        /// <summary>
        /// Reload configurations from file, this will clear all unsaved changes
        /// </summary>
        public void Reload()
        {
            XDocument newDoc = XDocument.Load(FullPath);
            LoadAllConfigurations(newDoc);
        }

        #endregion

        private string InternalGetConfiguration(string name)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Ensures(configurationsDict != null);

            string value = null;

            if (this.configurationsDict.TryGetValue(name, out value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// internal enum to determine which operation to do
        /// </summary>
        private enum ConfigOpt : byte
        {
            // Add a new configuration
            Add = 1,

            // Modify an exist configuration
            Update = 2,

            // Add or modify a configuration
            AddOrUpdate = 3,
        }

        private void InternalAddOrUpdateConfiguration(string name, string value, ConfigOpt opt)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(value != null);

            bool existsConfig = this.configurationsDict.ContainsKey(name);
            switch (opt)
            {
                case ConfigOpt.Add:
                    if (existsConfig)
                        throw new InvalidOperationException("configuration already exists");

                    AddConfigurationToXml(name, value);
                    break;
                case ConfigOpt.Update:
                    if (!existsConfig)
                        throw new InvalidOperationException("configuration not found");

                    UpdateConfigurationToXml(name, value);
                    break;
                case ConfigOpt.AddOrUpdate:
                    if (existsConfig)
                    {
                        UpdateConfigurationToXml(name, value);
                    }
                    else
                    {
                        AddConfigurationToXml(name, value);
                    }
                    break;
            }
            this.configurationsDict[name] = value;
            this.IsChanged = true;
        }

        private bool AddConfigurationToXml(string name, string value)
        {
            Contract.Ensures(this.xmlFile != null);

            lock (xmlFileSyncObj)
            {
                XElement root = this.xmlFile.Root;
                XElement configruation = new XElement(ItemElementName,
                    new XAttribute(NameAttributeName, name),
                    new XAttribute(ValueAttributeName, value));

                root.Add(configruation);
                return true;
            }
        }

        private bool UpdateConfigurationToXml(string name, string value)
        {
            Contract.Ensures(this.xmlFile != null);
            lock (xmlFileSyncObj)
            {
                var xmlValue = (from setting in this.xmlFile.Root.Elements(ItemElementName)
                                let nameAttr = setting.Attribute(NameAttributeName)
                                let valueAttr = setting.Attribute(ValueAttributeName)
                                where nameAttr != null
                                   && valueAttr != null
                                   && nameAttr.Value == name
                                select valueAttr)
                                .SingleOrDefault();

                if (xmlValue != null)
                {
                    xmlValue.Value = value;
                    return true;
                }
                return false;
            }
        }

        private void InternalRemoveConfiguration(string name)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Ensures(this.xmlFile != null);

            lock (xmlFileSyncObj)
            {
                if (this.configurationsDict.ContainsKey(name))
                {
                    var configuration = (from setting in this.xmlFile.Root.Elements(ItemElementName)
                                         let nameAttr = setting.Attribute(NameAttributeName)
                                         where nameAttr != null
                                            && nameAttr.Value == name
                                         select setting)
                                        .SingleOrDefault();

                    configuration.Remove();
                    this.configurationsDict.Remove(name);
                    this.IsChanged = true;
                }
            }
        }

    }
}