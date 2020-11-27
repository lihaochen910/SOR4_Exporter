namespace CommonLib
{
	public interface IYamlCustomSerialization
	{
		string yaml_serialize();

		void yaml_deserialize(string src);
	}
}
