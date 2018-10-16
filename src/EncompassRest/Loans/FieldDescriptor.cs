﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using EncompassRest.Loans.Enums;
using EncompassRest.Schema;
using EncompassRest.Utilities;
using EnumsNET;
using EnumsNET.NonGeneric;

namespace EncompassRest.Loans
{
    public class FieldDescriptor
    {
        internal readonly ModelPath _modelPath;
        private LoanFieldValueType? _valueType;
        private bool _loanEntityIsSet;
        private LoanEntity? _loanEntity;
        private bool _propertyAttributeIsSet;
        private LoanFieldPropertyAttribute _propertyAttribute;
        private ReadOnlyCollection<FieldOption> _options;
        private bool _declaredTypeIsSet;
        private Type _declaredType;
        private bool _enumTypeIsSet;
        private Type _enumType;

        public string FieldId { get; }

        /// <summary>
        /// For use with loan field locking.
        /// </summary>
        public string ModelPath { get; }

        /// <summary>
        /// For use with Webhook filter attributes.
        /// </summary>
        public string AttributePath
        {
            get
            {
                if (MultiInstance && !IsInstance)
                {
                    throw new InvalidOperationException("field descriptor must be an instance descriptor for multi-instance descriptors");
                }

                return _modelPath.ToString(name => JsonHelper.CamelCaseNamingStrategy.GetPropertyName(name, false), true).Replace("/currentApplication", "/applications/*");
            }
        }

        public bool MultiInstance { get; }

        public string InstanceSpecifier { get; }

        public bool IsInstance => MultiInstance && !string.IsNullOrEmpty(InstanceSpecifier);

        public bool IsBorrowerPairSpecific { get; }

        public virtual LoanFieldValueType ValueType
        {
            get
            {
                var valueType = ParentDescriptor?.ValueType ?? _valueType;
                if (!valueType.HasValue)
                {
                    var declaredType = DeclaredType;
                    if (declaredType == typeof(string))
                    {
                        valueType = LoanFieldValueType.String;
                    }
                    else if (declaredType == typeof(DateTime?))
                    {
                        valueType = LoanFieldValueType.DateTime;
                    }
                    else if (declaredType == typeof(decimal?))
                    {
                        valueType = LoanFieldValueType.Decimal;
                    }
                    else if (declaredType == typeof(int?))
                    {
                        valueType = LoanFieldValueType.Int32;
                    }
                    else if (declaredType == typeof(bool?))
                    {
                        valueType = LoanFieldValueType.Boolean;
                    }
                    else
                    {
                        valueType = LoanFieldValueType.Unknown;
                        if (declaredType != null)
                        {
                            var typeInfo = declaredType.GetTypeInfo();
                            if (typeInfo.IsGenericType && !typeInfo.IsGenericTypeDefinition)
                            {
                                var genericTypeDefinition = typeInfo.GetGenericTypeDefinition();
                                if (genericTypeDefinition == TypeData.OpenStringEnumValueType)
                                {
                                    valueType = LoanFieldValueType.String;
                                }
                                else if (genericTypeDefinition == TypeData.OpenNaType)
                                {
                                    valueType = LoanFieldValueType.NADecimal;
                                }
                            }
                        }
                    }
                    _valueType = valueType;
                }
                return valueType.GetValueOrDefault();
            }
        }

        public LoanFieldType Type { get; }

        public virtual LoanFieldFormat? Format
        {
            get
            {
                var format = ParentDescriptor?.Format ?? PropertyAttribute?._format;
                if (format.HasValue)
                {
                    return format;
                }
                if (EnumType == TypeData<State>.Type)
                {
                    return LoanFieldFormat.STATE;
                }
                switch (ValueType)
                {
                    case LoanFieldValueType.Boolean:
                        return LoanFieldFormat.YN;
                    case LoanFieldValueType.DateTime:
                        return LoanFieldFormat.DATE;
                    case LoanFieldValueType.Int32:
                        return LoanFieldFormat.INTEGER;
                    case LoanFieldValueType.String:
                        return LoanFieldFormat.STRING;
                    default:
                        return null;
                }
            }
        }

        public virtual bool ReadOnly => ParentDescriptor?.ReadOnly ?? Type == LoanFieldType.Virtual || PropertyAttribute?.ReadOnly == true;

        public string Description { get; }

        public virtual LoanEntity? LoanEntity
        {
            get
            {
                var loanEntity = _loanEntity;
                if (!_loanEntityIsSet)
                {
                    loanEntity = ParentDescriptor?.LoanEntity;
                    if (loanEntity == null)
                    {
                        if (Type == LoanFieldType.Virtual)
                        {
                            loanEntity = EncompassRest.Loans.LoanEntity.VirtualFields;
                        }
                        else if (_modelPath.Segments.Count == 1)
                        {
                            loanEntity = EncompassRest.Loans.LoanEntity.Loan;
                        }
                        else
                        {
                            var finalSegmentIndex = _modelPath.Segments.Count - 2;

                            var declaredType = TypeData<Loan>.Type;
                            for (var i = 0; i <= finalSegmentIndex && declaredType != null; ++i)
                            {
                                declaredType = _modelPath.Segments[i].GetDeclaredType(declaredType);
                            }
                            if (declaredType != null && EnumsNET.Enums.TryParse<LoanEntity>(declaredType.Name, out var newLoanEntity, EnumFormat.Name))
                            {
                                loanEntity = newLoanEntity;
                            }
                        }
                    }

                    _loanEntity = loanEntity;
                    _loanEntityIsSet = true;
                }
                return loanEntity;
            }
        }

        public virtual ReadOnlyCollection<FieldOption> Options
        {
            get
            {
                var options = ParentDescriptor?.Options ?? _options;
                if (options == null)
                {
                    var dictionary = new Dictionary<string, string>();
                    var declaredType = DeclaredType;
                    if (declaredType != null)
                    {
                        if (declaredType == TypeData<bool?>.Type)
                        {
                            dictionary.Add("true", "Yes");
                            dictionary.Add("false", "No");
                        }
                        else
                        {
                            var enumType = EnumType;
                            if (enumType != null)
                            {
                                foreach (var member in NonGenericEnums.GetMembers(enumType))
                                {
                                    var value = member.AsString(EnumFormat.EnumMemberValue, EnumFormat.Name);
                                    if (!dictionary.ContainsKey(value))
                                    {
                                        dictionary.Add(value, member.AsString(EnumFormat.Description, EnumFormat.EnumMemberValue, EnumFormat.Name));
                                    }
                                }
                            }
                        }
                    }
                    var propertyOptions = PropertyAttribute?.Options;
                    if (propertyOptions != null)
                    {
                        foreach (var pair in propertyOptions)
                        {
                            dictionary[pair.Key] = pair.Value;
                        }
                    }
                    var missingOptions = PropertyAttribute?.MissingOptions;
                    if (missingOptions != null)
                    {
                        foreach (var missingOption in missingOptions)
                        {
                            dictionary.Remove(missingOption);
                        }
                    }

                    options = new ReadOnlyCollection<FieldOption>(dictionary.Select(p => new FieldOption(p.Key, p.Value)).ToList());
                    _options = options;
                }
                return options;
            }
        }

        public FieldDescriptor ParentDescriptor { get; }

        private LoanFieldPropertyAttribute PropertyAttribute
        {
            get
            {
                var propertyAttribute = _propertyAttribute;
                if (!_propertyAttributeIsSet)
                {
                    propertyAttribute = _modelPath.GetAttribute<LoanFieldPropertyAttribute>(TypeData<Loan>.Type);
                    _propertyAttribute = propertyAttribute;
                    _propertyAttributeIsSet = true;
                }
                return propertyAttribute;
            }
        }

        private Type DeclaredType
        {
            get
            {
                var declaredType = _declaredType;
                if (!_declaredTypeIsSet)
                {
                    declaredType = _modelPath.GetDeclaredType(TypeData<Loan>.Type);
                    _declaredType = declaredType;
                    _declaredTypeIsSet = true;
                }
                return declaredType;
            }
        }

        private Type EnumType
        {
            get
            {
                var enumType = _enumType;
                if (!_enumTypeIsSet)
                {
                    var declaredType = DeclaredType;
                    if (declaredType != null)
                    {
                        var typeInfo = declaredType.GetTypeInfo();
                        if (typeInfo.IsGenericType && !typeInfo.IsGenericTypeDefinition)
                        {
                            var genericTypeDefinition = typeInfo.GetGenericTypeDefinition();
                            if (genericTypeDefinition == TypeData.OpenStringEnumValueType)
                            {
                                enumType = typeInfo.GenericTypeArguments[0];
                                _enumType = enumType;
                            }
                        }
                    }
                    _enumTypeIsSet = true;
                }
                return enumType;
            }
        }

        internal FieldDescriptor(string fieldId, ModelPath modelPath, string modelPathString, string description, bool multiInstance = false, string instanceSpecifier = null, FieldDescriptor parentDescriptor = null)
        {
            FieldId = fieldId;
            _modelPath = modelPath;
            ModelPath = modelPathString;
            
            if (modelPathString.StartsWith("Loan.CustomFields", StringComparison.OrdinalIgnoreCase))
            {
                Type = LoanFieldType.Custom;
            }
            else if (modelPathString.StartsWith("Loan.VirtualFields", StringComparison.OrdinalIgnoreCase))
            {
                Type = LoanFieldType.Virtual;
            }
            else if (modelPathString.StartsWith("Loan.CurrentApplication.", StringComparison.OrdinalIgnoreCase))
            {
                IsBorrowerPairSpecific = true;
            }
            
            Description = description ?? string.Empty;
            MultiInstance = multiInstance;
            InstanceSpecifier = instanceSpecifier;
            ParentDescriptor = parentDescriptor;
        }

        public FieldDescriptor GetInstanceDescriptor(string instanceSpecifier)
        {
            Preconditions.NotNullOrEmpty(instanceSpecifier, nameof(instanceSpecifier));

            if (!MultiInstance)
            {
                throw new InvalidOperationException("field must be multi-instance to retrieve an instance descriptor");
            }
            if (IsInstance)
            {
                throw new InvalidOperationException("cannot retrieve an instance descriptor of an instance descriptor");
            }

            var formatArg = int.TryParse(instanceSpecifier, NumberStyles.None, null, out var i) ? (object)i : instanceSpecifier;
            var modelPathString = string.Format(ModelPath, formatArg);
            return new FieldDescriptor(string.Format(FieldId, formatArg), LoanFieldDescriptors.CreateModelPath(modelPathString), modelPathString, string.Format(Description, formatArg), MultiInstance, instanceSpecifier, this);
        }
    }
}