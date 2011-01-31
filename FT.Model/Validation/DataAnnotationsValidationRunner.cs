using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using xVal.ServerSide;

namespace FT.Model
{
	public static class DataAnnotationsValidationRunner
	{
		public static IEnumerable<ErrorInfo> GetErrors(object instance)
		{
			TypeDescriptor.AddProvider(new AssociatedMetadataTypeTypeDescriptionProvider(instance.GetType()), instance);

			return from prop in TypeDescriptor.GetProperties(instance).Cast<PropertyDescriptor>()
				   from attribute in prop.Attributes.OfType<ValidationAttribute>()
				   where !attribute.IsValid(prop.GetValue(instance))
				   select new ErrorInfo(prop.Name, attribute.FormatErrorMessage(string.Empty), instance);
		}
	}
}