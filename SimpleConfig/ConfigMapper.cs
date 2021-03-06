﻿using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Xml;
using SimpleConfig.Helpers;

namespace SimpleConfig
{
    public class ConfigMapper : DynamicObject
    {
        private readonly XmlElement _configDocument;

        public ConfigMapper(XmlElement configDocument)
        {
            _configDocument = configDocument;
        }

        public T GetObjectFromXml<T>()
        {
            return (T)GetObjectFromXml(typeof(T), _configDocument);
        }

        public object GetObjectFromXml(Type type)
        {
            return GetObjectFromXml(type, _configDocument);
        }

        public object GetObjectFromXml(Type type, XmlElement element)
        {
            var result = type.CreateFromObjectOrInterface();
            PopulateObject(result, element);
            return result;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = GetObjectFromXml(binder.ReturnType);
            return true;
        }

        public void PopulateObject(object destination, XmlElement element)
        {
            var type = destination.GetType();
            
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x=>x.CanWrite || x.PropertyType.IsGenericEnumerable() || x.IsComplexType()))
            {
                PopulateProperty(destination, property, element);
            }
        }

        private void PopulateProperty(object destinationInstance, PropertyInfo destinationProperty, XmlElement element)
        {
            var mappingStrategies = destinationProperty.GetMappingStrategies();

            if (mappingStrategies==null)
            {
                throw new ConfigMappingException("It is not possible to map property {0} (of type {1}) because no mapping strategy could be found", destinationProperty.Name, destinationProperty.PropertyType.FullName);
            }

            foreach (var strategy in mappingStrategies)
            {
                if (strategy.Map(destinationInstance, destinationProperty, element, _configDocument, this))
                {
                    break;
                }
            }
        }
    }
}