using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Ujeby.UgUi.Nodes
{
	public interface INode
	{
		void Execute();
	}

	public interface ISerializableNode
	{
		string SerializeData();
		void DeserializeData(string serializedData);
	}

	public interface ILoggable
	{
		string[] GetInputs();
		string[] GetOutputs();
	}

	public abstract class NodeBase : INotifyPropertyChanged, INode, ILoggable, ISerializableNode
	{
		#region INode

		public virtual void Execute()
		{

		}

		public virtual string[] GetInputs()
		{
			return new string[] { };
		}

		public virtual string[] GetOutputs()
		{
			return new string[] { };
		}

		#endregion

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected bool SetField<T>(ref T field, T value, string propertyName)
		{
			if (EqualityComparer<T>.Default.Equals(field, value))
				return false;

			field = value;

			OnPropertyChanged(propertyName);

			return true;
		}

		#endregion

		protected static string SerializeProperties(string[] properties)
		{
			return "{" + string.Join(",", properties) + "}";
		}

		protected static string SerializeProperty(string name, string value)
		{
			return $"\"{ name }\":{ JsonConvert.ToString(value) }";
		}

		public string SerializeData()
		{
			var propertiesToSerialize = new List<string>();

			var allSerializableProperties = this.GetType().GetProperties()
				.Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(InputAttribute)))
				.Where(pi => AttributeHelper.GetValue<InputAttribute, bool>(pi, nameof(InputAttribute.Serializable)));

			foreach (var serializableProperty in allSerializableProperties)
				propertiesToSerialize.Add(SerializeProperty(serializableProperty.Name, this.GetType().GetProperty(serializableProperty.Name).GetValue(this)?.ToString()));

			return SerializeProperties(propertiesToSerialize.ToArray());
		}

		public void DeserializeData(string serializedData)
		{
			dynamic deserialized = JsonConvert.DeserializeObject(serializedData);

			var allSerializableProperties = this.GetType().GetProperties()
				.Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(InputAttribute)))
				.Where(pi => AttributeHelper.GetValue<InputAttribute, bool>(pi, nameof(InputAttribute.Serializable)));

			foreach (var serializableProperty in allSerializableProperties)
			{
				var value = deserialized[serializableProperty.Name];
				if (value != null)
					serializableProperty.SetValue(this, Ujeby.Common.Tools.Convert.ChangeType(deserialized[serializableProperty.Name].Value as string, serializableProperty.PropertyType));
			}
		}
	}
}
