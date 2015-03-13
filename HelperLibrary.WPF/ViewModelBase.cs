﻿/* 
 * FileName:    ViewModelBase.cs
 * Author:      functionghw<functionghw@hotmail.com>
 * CreateTime:  3/12/2015 2:25:06 PM
 * Version:     v1.0
 * Description:
 * */

namespace HelperLibrary.WPF
{
    using HelperLibrary.Core.Annotation;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public abstract class ViewModelBase : NotifyPropertyChangedBase, IDataErrorInfo
    {
        /// <summary>
        /// Initialize ViewModelBase
        /// </summary>
        public ViewModelBase()
        {
            thisType = this.GetType();
        }

        #region IDataErrorInfo Members

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        /// <remarks>
        /// default value is an empty string ("").
        /// </remarks>
        public virtual string Error
        {
            get { return string.Empty; }
        }

        private Type thisType;

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <param name="propertyName">The name of the property whose error message to get.</param>
        /// <returns>The error message for the property. The default is an empty string ("").</returns>
        public virtual string this[string propertyName]
        {
            get
            {
                if (string.IsNullOrEmpty(propertyName))
                {
                    return string.Empty;
                }
                var prop = thisType.GetProperty(propertyName);

                if (prop == null)
                {
                    // no such a property
                    return string.Empty;
                }
                object value = prop.GetValue(this, null);

                /* Here we set the propertyName as the context's MemberName so that 
                 * we can support to format the error message.
                 * 
                 * If the property has a DisplayAttribute, the context's DisplayName property 
                 * will find the DisplayAttribute by MemberName; otherwise the DisplayName 
                 * is same as MemberName. If we don't give the MemverName, the name of 
                 * the instance's type(the ViewModel) will be as the DisplayName.
                 * 
                 * For example: [LocalizedRequired(Scope, ErrorMessage="The {0} is required")]
                 *          the argument "{0}" will be replace by the property's DisplayName
                 */

                ValidationContext context = new ValidationContext(this)
                {
                    MemberName = propertyName,
                };

                /* If has LocalizedDisplayAttribute, get the localized name
                 * as the context's DisplayName directly;
                 */
                var lclDisplayAttributes = prop.GetCustomAttributes(typeof(LocalizedDisplayAttribute), true) as LocalizedDisplayAttribute[];

                if (lclDisplayAttributes.Length > 0)
                {
                    context.DisplayName = lclDisplayAttributes[0].GetLocalizedName();
                }

                var attrs = prop.GetCustomAttributes(typeof(ValidationAttribute), true)
                    as ValidationAttribute[];

                /* validate the value and get all validation results that not valid
                 * Note that instead of getting the ErrorMessage directly, 
                 * we get the ValidationResult first, because some ValidationAttribute classes
                 * may implement localization(for example HelperLibrary.Core.Annotation.*). 
                 */
                var errors = from attr in attrs
                             where !attr.IsValid(value)
                             select attr.GetValidationResult(value, context);

                if (errors.Any())
                {
                    return errors.First().ErrorMessage;
                }
                return string.Empty;
            }
        }

        #endregion
    }
}