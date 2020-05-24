using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.Serialization;

namespace RowThon
{
	public class SerializableDictionary<TKey, TValue>
		: Dictionary<TKey, TValue>, IXmlSerializable
	{
		//null
		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return null;
		}

		//
		public void ReadXml(XmlReader reader)
		{
			bool wasEmpty = reader.IsEmptyElement;
			reader.Read();
			if( wasEmpty )
				return;

			//XmlSerializerを用意する
			XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
			XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

			while( reader.NodeType != XmlNodeType.EndElement ) {
				reader.ReadStartElement("KeyValuePair");

				//キーを逆シリアル化する
				reader.ReadStartElement("Key");
				TKey key = (TKey)keySerializer.Deserialize(reader);
				reader.ReadEndElement();

				//値を逆シリアル化する
				reader.ReadStartElement("Value");
				TValue val = (TValue)valueSerializer.Deserialize(reader);
				reader.ReadEndElement();

				reader.ReadEndElement();

				//コレクションに追加する
				this.Add(key, val);

				//次へ
				reader.MoveToContent();
			}

			reader.ReadEndElement();
		}
		//
		public void WriteXml(XmlWriter writer)
		{
			//XmlSerializerを用意する
			XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
			XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

			foreach( TKey key in this.Keys ) {
				writer.WriteStartElement("KeyValuePair");

				//キーを書き込む
				writer.WriteStartElement("Key");
				keySerializer.Serialize(writer, key);
				writer.WriteEndElement();

				//値を書き込む
				writer.WriteStartElement("Value");
				valueSerializer.Serialize(writer, this[key]);
				writer.WriteEndElement();

				writer.WriteEndElement();
			}
		}
	}
}
