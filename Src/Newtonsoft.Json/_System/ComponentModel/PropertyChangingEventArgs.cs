using System;

namespace System.ComponentModel {
	
	public class PropertyChangingEventArgs : EventArgs {
		
		private string _propertyName;
		
		public PropertyChangingEventArgs(string propertyName) {
			this._propertyName = propertyName;
		}
		
		public virtual string PropertyName {
			get {
				return this._propertyName;
			}
		}
	}
}

