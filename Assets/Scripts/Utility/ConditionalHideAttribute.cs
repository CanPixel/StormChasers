using System.Collections;
using System;
using UnityEngine;

//Original version of the ConditionalHideAttribute created by Brecht Lecluyse (www.brechtos.com)
//Modified by: Sebastian Lague

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | 
	AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class ConditionalHideAttribute : PropertyAttribute {
	public string conditionalSourceField;
	public bool boolIndex;
	public int enumIndex;

	public ConditionalHideAttribute(string boolVar) {
		conditionalSourceField = boolVar;
	}

	public ConditionalHideAttribute(string enumVar, int enumIndex) {
		this.enumIndex = enumIndex;
		conditionalSourceField = enumVar;
	}

	public ConditionalHideAttribute(string enumVar, bool boolIndex) {
		this.boolIndex = boolIndex;
		conditionalSourceField = enumVar;
	}
}
