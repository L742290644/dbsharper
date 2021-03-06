﻿using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

using DbSharper2.Schema.Configuration;
using DbSharper2.Schema.Database;
using DbSharper2.Schema.Infrastructure;

namespace DbSharper2.Schema
{
	internal class MappingRuleManager
	{
		#region Fields

		private MappingConfiguration configuration;
		private Collection<Rule> dataAccessTrimPrefixRules;
		private Collection<Rule> modelTrimPrefixRules;
		private Regex regexClassMethod;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Constructor, initialize MappingRuleManager.
		/// </summary>
		/// <param name="configuration">Mapping configuration.</param>
		internal MappingRuleManager(MappingConfiguration configuration)
		{
			if (configuration == null)
			{
				throw new ArgumentNullException("configuration");
			}

			this.configuration = configuration;

			this.regexClassMethod = new Regex(this.configuration.DataAccess.ClassMethodMask, RegexOptions.Compiled);
			this.dataAccessTrimPrefixRules = new Collection<Rule>();
			this.modelTrimPrefixRules = new Collection<Rule>();

			foreach (var rule in this.configuration.Model.IncludeRules)
			{
				if (rule.TrimPrefix.Value && !string.IsNullOrEmpty(rule.Prefix))
				{
					modelTrimPrefixRules.Add(rule);
				}
			}

			foreach (var rule in this.configuration.DataAccess.IncludeRules)
			{
				if (rule.TrimPrefix.Value && !string.IsNullOrEmpty(rule.Prefix))
				{
					dataAccessTrimPrefixRules.Add(rule);
				}
			}
		}

		#endregion Constructors

		#region Methods

		internal ClassMethodContainer GetClassMethod(ISchema procedure)
		{
			string input = TrimPrefix(procedure);

			Match match = this.regexClassMethod.Match(input);

			if (string.IsNullOrEmpty(match.Groups["Method"].Value))
			{
				// TODO: Embed string into resource file later.
				throw new DbSharperException("Can not find method match in method mask.");
			}

			switch (match.Groups.Count)
			{
				case 2:
					{

						return new ClassMethodContainer()
						{
							ClassName = "_Global",
							MethodName = match.Groups["Method"].Value.ToPascalCase()
						};
					}
				case 3:
					{
						if (string.IsNullOrEmpty(match.Groups["Class"].Value))
						{
							// TODO: Embed string into resource file later.
							throw new DbSharperException("Can not find class match in class mask.");
						}

						return new ClassMethodContainer()
						{
							ClassName = match.Groups["Class"].Value.ToPascalCase(),
							MethodName = match.Groups["Method"].Value.ToPascalCase()
						};
					}
				default:
					{
						return null;
					}
			}
		}

		internal bool IsAutoDiscoverReference()
		{
			return configuration.Model.AutoDiscoverReference;
		}

		/// <summary>
		/// If database object is included in rule.
		/// </summary>
		/// <param name="databaseObject">Database object.</param>
		/// <returns>If is included.</returns>
		internal bool IsIncluded(ISchema databaseObject)
		{
			if (this.IsExcluded(databaseObject))
			{
				return false;
			}

			if (databaseObject is Procedure)
			{
				return IsIncludedInternal(databaseObject, this.configuration.DataAccess.IncludeRules);
			}
			else if (databaseObject is Table || databaseObject is View)
			{
				return IsIncludedInternal(databaseObject, this.configuration.Model.IncludeRules);
			}

			throw new ArgumentOutOfRangeException("databaseObject");
		}

		/// <summary>
		/// Trim prefix of a database object name.
		/// </summary>
		/// <param name="databaseObject">Database object.</param>
		/// <returns>Trimmed database object name.</returns>
		internal string TrimPrefix(ISchema databaseObject)
		{
			if (databaseObject is Procedure)
			{
				return TrimPrefixInternal(databaseObject, dataAccessTrimPrefixRules);
			}
			else if (databaseObject is Table || databaseObject is View)
			{
				return TrimPrefixInternal(databaseObject, modelTrimPrefixRules);
			}

			throw new ArgumentOutOfRangeException("databaseObject");
		}

		private static bool IsExcludedInternal(ISchema databaseObject, Collection<Rule> rules)
		{
			if (rules.Count == 0)
			{
				return false;
			}

			foreach (Rule rule in rules)
			{
				if (rule.RuleType == RuleType.Schema)
				{
					if (string.Compare(databaseObject.Schema, rule.Name, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return true;
					}
				}
				else
				{
					if (!string.IsNullOrEmpty(rule.Name))
					{
						if (string.Compare(databaseObject.Name, rule.Name, StringComparison.OrdinalIgnoreCase) == 0)
						{
							return true;
						}
					}
					else if (!string.IsNullOrEmpty(rule.Prefix))
					{
						if (databaseObject.Name.StartsWith(rule.Prefix, StringComparison.OrdinalIgnoreCase))
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		private static bool IsIncludedInternal(ISchema databaseObject, Collection<Rule> rules)
		{
			if (rules.Count == 0)
			{
				return true;
			}

			foreach (Rule rule in rules)
			{
				if (rule.RuleType == RuleType.Schema)
				{
					if (string.Compare(databaseObject.Schema, rule.Name, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return true;
					}
				}
				else
				{
					if (!string.IsNullOrEmpty(rule.Name))
					{
						if (string.Compare(databaseObject.Name, rule.Name, StringComparison.OrdinalIgnoreCase) == 0)
						{
							return true;
						}
					}
					else if (!string.IsNullOrEmpty(rule.Prefix))
					{
						if (databaseObject.Name.StartsWith(rule.Prefix, StringComparison.OrdinalIgnoreCase))
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		private static string TrimPrefixInternal(ISchema databaseObject, Collection<Rule> rules)
		{
			string name = databaseObject.Name;

			foreach (Rule rule in rules)
			{
				if (rule.TrimPrefix.Value &&
					!string.IsNullOrEmpty(rule.Prefix) &&
					name.StartsWith(rule.Prefix, StringComparison.OrdinalIgnoreCase))
				{
					return name.Substring(rule.Prefix.Length);
				}
			}

			return name;
		}

		private bool IsExcluded(ISchema databaseObject)
		{
			if (databaseObject is Procedure)
			{
				return IsExcludedInternal(databaseObject, this.configuration.DataAccess.ExcludeRules);
			}
			else if(databaseObject is Table || databaseObject is View)
			{
				return IsExcludedInternal(databaseObject, this.configuration.Model.ExcludeRules);
			}

			throw new ArgumentOutOfRangeException("databaseObject");
		}

		#endregion Methods
	}
}