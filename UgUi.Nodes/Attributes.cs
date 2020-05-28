using System;
using System.Linq;
using System.Reflection;

namespace Ujeby.UgUi.Nodes
{
	public class AttributeHelper
	{
		public static TAttributeValue GetValue<TAttribute, TAttributeValue>(Type type, string attributeName)
		{
			return GetValue<TAttribute, TAttributeValue>(type.CustomAttributes.SingleOrDefault(a => a.AttributeType == typeof(TAttribute)), attributeName);
		}

		public static TAttributeValue GetValue<TAttribute, TAttributeValue>(PropertyInfo propertyInfo, string attributeName)
		{
			return GetValue<TAttribute, TAttributeValue>(propertyInfo.CustomAttributes.SingleOrDefault(a => a.AttributeType == typeof(TAttribute)), attributeName);
		}

		public static TAttributeValue GetValue<TAttribute, TAttributeValue>(CustomAttributeData attributeData, string attributeName)
		{
			if (attributeData == null)
				return default;

			var namedArgument = attributeData.NamedArguments.SingleOrDefault(na => na.MemberName == attributeName);
			if (namedArgument == null)
				return default;

			if (namedArgument.TypedValue == null || namedArgument.TypedValue.Value == null)
				return default;

			return (TAttributeValue)namedArgument.TypedValue.Value;
		}
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class IgnoredPropertyAttribute : Attribute
	{
		/// <summary>
		/// name of property that should be ignored in user control construction
		/// </summary>
		public string Name { get; set; } = null;
	}

	public class BaseAttribute : Attribute
	{
		/// <summary>
		/// name to display
		/// </summary>
		public string DisplayName { get; set; } = null;
	}

	public class NodeInfoAttribute : BaseAttribute
	{
		/// <summary>
		///  if true, wont be used in workspace
		/// </summary>
		public bool Abstract { get; set; } = false;
	}

	public class InputAttribute : BaseAttribute
	{
		/// <summary>
		/// if true, property has anchor and label only (no user-input element)
		/// </summary>
		public bool AnchorOnly { get; set; } = false;

		/// <summary>
		/// if false, property has no input anchor
		/// </summary>
		public bool InputAnchor { get; set; } = true;

		/// <summary>
		/// if true, property has also output anchor
		/// </summary>
		public bool OutputAnchor { get; set; } = false;

		/// <summary>
		/// if ReadOnly is true, no anchor is used
		/// </summary>
		public bool ReadOnly { get; set; } = false;

		/// <summary>
		/// name of property that is used for input anchor
		/// </summary>
		public string InputAnchorProperty { get; set; } = null;

		/// <summary>
		/// name of property that is used for output anchor
		/// </summary>
		public string OutputAnchorProperty { get; set; } = null;

		/// <summary>
		/// used as name for control user-input element (TextBox, ...)
		/// </summary>
		public string InputName { get; set; } = null;

		/// <summary>
		/// order from top to bottom
		/// </summary>
		public int Order { get; set; } = 0;

		/// <summary>
		/// 
		/// </summary>
		public bool Serializable { get; set; }
	}

	public class OutputAttribute : BaseAttribute
	{
		/// <summary>
		/// if false, property also has user-input element (read only)
		/// </summary>
		public bool AnchorOnly { get; set; } = true;

		/// <summary>
		/// name of property that is used for output anchor
		/// </summary>
		public string OutputAnchorProperty { get; set; } = null;

		/// <summary>
		/// if ReadOnly is true, user is not able to change value
		/// </summary>
		public bool ReadOnly { get; set; } = false;

		/// <summary>
		/// if true, there wont be no output anchor
		/// </summary>
		public bool NoAnchor { get; set; } = false;
	}

	public class ImageBindingsAttribute : Attribute
	{
		public string Width { get; set; }
		public string Height { get; set; }
	}
}
