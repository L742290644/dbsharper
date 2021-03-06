﻿using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Xml.Serialization;

using DbSharper2.Schema.Infrastructure;

namespace DbSharper2.Schema.Code
{
	[XmlType("model")]
	public class Model : IName
	{
		#region Constructors

		public Model()
		{
			this.Properties = new NamedCollection<Property>();
		}

		#endregion Constructors

		#region Properties

		[XmlAttribute("description")]
		[Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
		public string Description
		{
			get;
			set;
		}

		[XmlAttribute("isView")]
		[ReadOnly(true)]
		public bool IsView
		{
			get;
			set;
		}

		/// <summary>
		/// Relative table or view name.
		/// </summary>
		[XmlAttribute("mappingName")]
		[ReadOnly(true)]
		public string MappingName
		{
			get;
			set;
		}

		[XmlAttribute("name")]
		public string Name
		{
			get;
			set;
		}

		[XmlIgnore]
		public string Namespace
		{
			get;
			set;
		}

		[XmlElement("property")]
		[Browsable(false)]
		public NamedCollection<Property> Properties
		{
			get;
			set;
		}

		#endregion Properties

		#region Methods

		public override string ToString()
		{
			return this.Namespace + "." + this.Name;
		}

		#endregion Methods
	}
}